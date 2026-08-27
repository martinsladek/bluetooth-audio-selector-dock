using System.Runtime.InteropServices;

namespace BluetoothDock;

enum EDataFlow
{
    eRender = 0,
    eCapture = 1,
    eAll = 2
}

enum ERole
{
    eConsole = 0,
    eMultimedia = 1,
    eCommunications = 2
}

[StructLayout(LayoutKind.Sequential)]
struct KsProperty
{
    public Guid Set;
    public uint Id;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
struct PropertyKey
{
    public Guid Fmtid;
    public int Pid;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
struct PropVariantNative
{
    [FieldOffset(0)] public ushort vt;
    [FieldOffset(8)] public IntPtr pointerValue;
}

static class DeviceStates
{
    public const int Active = 0x00000001;
    public const int Disabled = 0x00000002;
    public const int NotPresent = 0x00000004;
    public const int Unplugged = 0x00000008;
    public const int MaskAll = 0x0000000F;
}

static class StorageModes
{
    public const int Read = 0;
}

static class ClsCtx
{
    public const int All = 0x1 | 0x2 | 0x4 | 0x10; // INPROC_SERVER | INPROC_HANDLER | LOCAL_SERVER | REMOTE_SERVER
}

static class VariantTypes
{
    public const ushort Empty = 0;
    public const ushort LpWStr = 31;
    public const ushort Clsid = 72;
}

static class KsPropertyFlags
{
    public const uint Get = 0x00000001;
    public const uint BasicSupport = 0x00000200;
}

static class BtAudioProperty
{
    // KSPROPSETID_BtAudio — documented in ksmedia.h / MSDN.
    public static readonly Guid SetId = new("7FA06C40-B8F6-4C7E-8556-E8C33A12E54D");
    public const uint OneShotReconnect = 0;
    public const uint OneShotDisconnect = 1;
}

static class DevicePropertyKeys
{
    public static PropertyKey FriendlyName => new()
    {
        Fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
        Pid = 14
    };

    public static PropertyKey ContainerId => new()
    {
        Fmtid = new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"),
        Pid = 2
    };
}

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
class MMDeviceEnumeratorCom { }

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IMMDeviceEnumerator
{
    [PreserveSig] int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IMMDeviceCollection devices);
    [PreserveSig] int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
    [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
    [PreserveSig] int RegisterEndpointNotificationCallback(IMMNotificationClient client);
    [PreserveSig] int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
}

[ComImport]
[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IMMDeviceCollection
{
    [PreserveSig] int GetCount(out int count);
    [PreserveSig] int Item(int index, out IMMDevice device);
}

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IMMDevice
{
    [PreserveSig] int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object? iface);
    [PreserveSig] int OpenPropertyStore(int access, out IPropertyStore properties);
    [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
    [PreserveSig] int GetState(out int state);
}

[ComImport]
[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IPropertyStore
{
    [PreserveSig] int GetCount(out int count);
    [PreserveSig] int GetAt(int index, out PropertyKey key);
    [PreserveSig] int GetValue(ref PropertyKey key, out PropVariantNative value);
    [PreserveSig] int SetValue(ref PropertyKey key, ref PropVariantNative value);
    [PreserveSig] int Commit();
}

[ComImport]
[Guid("2A07407E-6497-4A18-9787-32F79BD0D98F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IDeviceTopology
{
    [PreserveSig] int GetConnectorCount(out uint count);
    [PreserveSig] int GetConnector(uint index, out IConnector connector);
    [PreserveSig] int GetSubunitCount(out uint count);
    [PreserveSig] int GetSubunit(uint index, out IntPtr subunit);
    [PreserveSig] int GetPartById(uint id, out IntPtr part);
    [PreserveSig] int GetDeviceId([MarshalAs(UnmanagedType.LPWStr)] out string id);
    [PreserveSig] int GetSignalPath(IntPtr from, IntPtr to, bool rejectMixed, out IntPtr parts);
}

[ComImport]
[Guid("9c2c4058-23f5-41de-877a-df3af236a09e")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IConnector
{
    [PreserveSig] int GetType(out int type);
    [PreserveSig] int GetDataFlow(out int flow);
    [PreserveSig] int ConnectTo(IConnector other);
    [PreserveSig] int Disconnect();
    [PreserveSig] int IsConnected(out bool connected);
    [PreserveSig] int GetConnectedTo(out IConnector other);
    [PreserveSig] int GetConnectorIdConnectedTo([MarshalAs(UnmanagedType.LPWStr)] out string id);
    [PreserveSig] int GetDeviceIdConnectedTo([MarshalAs(UnmanagedType.LPWStr)] out string id);
}

[ComImport]
[Guid("AE2DE0E4-5BCA-4F2D-AA46-5D13F8FDB3A9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IPart
{
    [PreserveSig] int GetName([MarshalAs(UnmanagedType.LPWStr)] out string name);
    [PreserveSig] int GetLocalId(out uint id);
    [PreserveSig] int GetGlobalId([MarshalAs(UnmanagedType.LPWStr)] out string id);
    [PreserveSig] int GetPartType(out int partType);
    [PreserveSig] int GetSubType(out Guid subType);
    [PreserveSig] int GetControlInterfaceCount(out uint count);
    [PreserveSig] int GetControlInterface(uint index, out IntPtr control);
    [PreserveSig] int EnumPartsIncoming(out IntPtr parts);
    [PreserveSig] int EnumPartsOutgoing(out IntPtr parts);
    [PreserveSig] int GetTopologyObject(out IDeviceTopology topology);
}

[ComImport]
[Guid("28F54685-06FD-11D2-B27A-00A0C9223196")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IKsControl
{
    [PreserveSig] int KsProperty(ref KsProperty property, uint propertyLength, IntPtr data, uint dataLength, out uint bytesReturned);
    [PreserveSig] int KsMethod(IntPtr method, uint methodLength, IntPtr data, uint dataLength, out uint bytesReturned);
    [PreserveSig] int KsEvent(IntPtr ksEvent, uint eventLength, IntPtr data, uint dataLength, out uint bytesReturned);
}

[ComImport]
[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IMMNotificationClient
{
    [PreserveSig] int OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, uint newState);
    [PreserveSig] int OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
    [PreserveSig] int OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
    [PreserveSig] int OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);
    [PreserveSig] int OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, PropertyKey key);
}

static class NativeOle
{
    [DllImport("ole32.dll")]
    public static extern int PropVariantClear(ref PropVariantNative pvar);
}

static class NativeUser32
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}

static class ComRelease
{
    public static void Once(object? comObject)
    {
        if (comObject is null)
            return;

        try
        {
            Marshal.ReleaseComObject(comObject);
        }
        catch (InvalidComObjectException)
        {
        }
    }
}
