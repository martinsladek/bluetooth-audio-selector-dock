using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace BluetoothDock;

sealed class BluetoothAudioDevice
{
    public required Guid ContainerId { get; init; }
    public required string Name { get; init; }
    public required bool IsConnected { get; init; }
    public required IReadOnlyList<string> EndpointIds { get; init; }
}

static class BluetoothAudioService
{
    public static IReadOnlyList<BluetoothAudioDevice> Enumerate()
    {
        var groups = new Dictionary<Guid, DeviceAccumulator>();
        IReadOnlyDictionary<Guid, string> containerNames = LoadContainerNames();

        IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
        try
        {
            int hr = enumerator.EnumAudioEndpoints(EDataFlow.eAll, DeviceStates.MaskAll, out IMMDeviceCollection collection);
            if (hr < 0 || collection is null)
                return Array.Empty<BluetoothAudioDevice>();

            try
            {
                collection.GetCount(out int count);
                for (int i = 0; i < count; i++)
                {
                    if (collection.Item(i, out IMMDevice device) < 0 || device is null)
                        continue;

                    try
                    {
                        CollectEndpoint(device, enumerator, groups, containerNames);
                    }
                    finally
                    {
                        ComRelease.Once(device);
                    }
                }
            }
            finally
            {
                ComRelease.Once(collection);
            }
        }
        finally
        {
            ComRelease.Once(enumerator);
        }

        return groups.Values
            .Select(g => new BluetoothAudioDevice
            {
                ContainerId = g.ContainerId,
                Name = g.Name,
                IsConnected = g.IsConnected,
                EndpointIds = g.EndpointIds
            })
            .OrderBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public static bool Connect(Guid containerId) =>
        SendOneShot(containerId, BtAudioProperty.OneShotReconnect);

    public static bool Disconnect(Guid containerId) =>
        SendOneShot(containerId, BtAudioProperty.OneShotDisconnect);

    public static bool IsBluetoothRadioOn()
    {
        try
        {
            IReadOnlyList<Radio> radios = Task.Run(() => Radio.GetRadiosAsync().AsTask()).GetAwaiter().GetResult();
            Radio? bluetooth = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
            return bluetooth is null || bluetooth.State == RadioState.On;
        }
        catch
        {
            return true;
        }
    }

    private static bool SendOneShot(Guid containerId, uint propertyId)
    {
        var devices = Enumerate();
        BluetoothAudioDevice? device = devices.FirstOrDefault(d => d.ContainerId == containerId);
        if (device is null)
            return false;

        IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
        bool anySuccess = false;
        try
        {
            foreach (string endpointId in device.EndpointIds)
            {
                if (enumerator.GetDevice(endpointId, out IMMDevice endpoint) < 0 || endpoint is null)
                    continue;

                try
                {
                    foreach (IKsControl ks in GetKsControls(endpoint, enumerator))
                    {
                        try
                        {
                            var property = new KsProperty
                            {
                                Set = BtAudioProperty.SetId,
                                Id = propertyId,
                                Flags = KsPropertyFlags.Get
                            };
                            int hr = ks.KsProperty(
                                ref property,
                                (uint)Marshal.SizeOf<KsProperty>(),
                                IntPtr.Zero,
                                0,
                                out _);
                            if (hr >= 0)
                                anySuccess = true;
                        }
                        finally
                        {
                            ComRelease.Once(ks);
                        }
                    }
                }
                finally
                {
                    ComRelease.Once(endpoint);
                }
            }
        }
        finally
        {
            ComRelease.Once(enumerator);
        }

        return anySuccess;
    }

    private static void CollectEndpoint(
        IMMDevice device,
        IMMDeviceEnumerator enumerator,
        Dictionary<Guid, DeviceAccumulator> groups,
        IReadOnlyDictionary<Guid, string> containerNames)
    {
        List<IKsControl> controls = GetKsControls(device, enumerator);
        if (controls.Count == 0)
            return;

        bool supportsBtAudio = false;
        foreach (IKsControl ks in controls)
        {
            try
            {
                if (SupportsBtAudio(ks))
                    supportsBtAudio = true;
            }
            finally
            {
                ComRelease.Once(ks);
            }
        }

        if (!supportsBtAudio)
            return;

        if (device.GetId(out string endpointId) < 0 || string.IsNullOrEmpty(endpointId))
            return;

        device.GetState(out int state);
        Guid containerId = ReadContainerId(device);
        if (containerId == Guid.Empty)
            containerId = StableFallbackId(endpointId);

        string name = containerNames.TryGetValue(containerId, out string? containerName)
            ? containerName
            : CleanName(ReadFriendlyName(device));

        if (!groups.TryGetValue(containerId, out DeviceAccumulator? group))
        {
            group = new DeviceAccumulator
            {
                ContainerId = containerId,
                Name = name
            };
            groups[containerId] = group;
        }

        if (!group.EndpointIds.Contains(endpointId, StringComparer.OrdinalIgnoreCase))
            group.EndpointIds.Add(endpointId);

        if (state == DeviceStates.Active)
            group.IsConnected = true;

        if (string.IsNullOrWhiteSpace(group.Name) && !string.IsNullOrWhiteSpace(name))
            group.Name = name;
    }

    private static List<IKsControl> GetKsControls(IMMDevice endpoint, IMMDeviceEnumerator enumerator)
    {
        var result = new List<IKsControl>();
        Guid topologyIid = typeof(IDeviceTopology).GUID;
        if (endpoint.Activate(ref topologyIid, ClsCtx.All, IntPtr.Zero, out object? topologyObj) < 0
            || topologyObj is not IDeviceTopology topology)
        {
            return result;
        }

        try
        {
            if (topology.GetConnectorCount(out uint connectorCount) < 0)
                return result;

            for (uint i = 0; i < connectorCount; i++)
            {
                if (topology.GetConnector(i, out IConnector connector) < 0 || connector is null)
                    continue;

                try
                {
                    if (connector.IsConnected(out bool connected) < 0 || !connected)
                        continue;

                    if (connector.GetConnectedTo(out IConnector other) < 0 || other is null)
                        continue;

                    try
                    {
                        IPart part;
                        try
                        {
                            part = (IPart)other;
                        }
                        catch (InvalidCastException)
                        {
                            continue;
                        }

                        if (part.GetTopologyObject(out IDeviceTopology otherTopology) < 0 || otherTopology is null)
                            continue;

                        try
                        {
                            if (otherTopology.GetDeviceId(out string otherDeviceId) < 0
                                || string.IsNullOrEmpty(otherDeviceId)
                                || otherDeviceId.IndexOf(@"\?\bth", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                continue;
                            }

                            IKsControl? ks = ActivateKsControl(enumerator, otherDeviceId);
                            if (ks is not null)
                                result.Add(ks);
                        }
                        finally
                        {
                            ComRelease.Once(otherTopology);
                        }
                    }
                    finally
                    {
                        ComRelease.Once(other);
                    }
                }
                finally
                {
                    ComRelease.Once(connector);
                }
            }
        }
        finally
        {
            ComRelease.Once(topology);
        }

        return result;
    }

    private static IKsControl? ActivateKsControl(IMMDeviceEnumerator enumerator, string deviceId)
    {
        if (enumerator.GetDevice(deviceId, out IMMDevice device) < 0 || device is null)
            return null;

        try
        {
            Guid ksIid = typeof(IKsControl).GUID;
            if (device.Activate(ref ksIid, ClsCtx.All, IntPtr.Zero, out object? ksObj) < 0 || ksObj is not IKsControl ks)
                return null;

            return ks;
        }
        finally
        {
            ComRelease.Once(device);
        }
    }

    private static bool SupportsBtAudio(IKsControl ks)
    {
        var property = new KsProperty
        {
            Set = BtAudioProperty.SetId,
            Id = BtAudioProperty.OneShotReconnect,
            Flags = KsPropertyFlags.BasicSupport
        };

        IntPtr buffer = Marshal.AllocHGlobal(64);
        try
        {
            int hr = ks.KsProperty(ref property, (uint)Marshal.SizeOf<KsProperty>(), buffer, 64, out _);
            return hr >= 0;
        }
        catch
        {
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static Guid ReadContainerId(IMMDevice device)
    {
        if (device.OpenPropertyStore(StorageModes.Read, out IPropertyStore store) < 0 || store is null)
            return Guid.Empty;

        try
        {
            PropertyKey key = DevicePropertyKeys.ContainerId;
            if (store.GetValue(ref key, out PropVariantNative value) < 0)
                return Guid.Empty;

            try
            {
                if (value.vt != VariantTypes.Clsid || value.pointerValue == IntPtr.Zero)
                    return Guid.Empty;

                return Marshal.PtrToStructure<Guid>(value.pointerValue);
            }
            finally
            {
                NativeOle.PropVariantClear(ref value);
            }
        }
        finally
        {
            ComRelease.Once(store);
        }
    }

    private static string ReadFriendlyName(IMMDevice device)
    {
        if (device.OpenPropertyStore(StorageModes.Read, out IPropertyStore store) < 0 || store is null)
            return string.Empty;

        try
        {
            PropertyKey key = DevicePropertyKeys.FriendlyName;
            if (store.GetValue(ref key, out PropVariantNative value) < 0)
                return string.Empty;

            try
            {
                if (value.vt != VariantTypes.LpWStr || value.pointerValue == IntPtr.Zero)
                    return string.Empty;

                return Marshal.PtrToStringUni(value.pointerValue) ?? string.Empty;
            }
            finally
            {
                NativeOle.PropVariantClear(ref value);
            }
        }
        finally
        {
            ComRelease.Once(store);
        }
    }

    private static IReadOnlyDictionary<Guid, string> LoadContainerNames()
    {
        var names = new Dictionary<Guid, string>();
        try
        {
            DeviceInformationCollection containers = Task.Run(() =>
                    DeviceInformation
                        .FindAllAsync(string.Empty, null, DeviceInformationKind.DeviceContainer)
                        .AsTask())
                .GetAwaiter()
                .GetResult();

            foreach (DeviceInformation info in containers)
            {
                if (Guid.TryParse(info.Id, out Guid id) && !string.IsNullOrWhiteSpace(info.Name))
                    names[id] = info.Name;
            }
        }
        catch
        {
        }

        return names;
    }

    internal static string CleanName(string friendlyName)
    {
        if (string.IsNullOrWhiteSpace(friendlyName))
            return friendlyName;

        int open = friendlyName.IndexOf('(');
        int close = friendlyName.LastIndexOf(')');
        if (open >= 0 && close > open)
            return friendlyName.Substring(open + 1, close - open - 1).Trim();

        return friendlyName.Trim();
    }

    private static Guid StableFallbackId(string endpointId)
    {
        byte[] hash = SHA256.HashData(Encoding.Unicode.GetBytes(endpointId));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed class DeviceAccumulator
    {
        public Guid ContainerId;
        public string Name = string.Empty;
        public bool IsConnected;
        public List<string> EndpointIds { get; } = new();
    }
}
