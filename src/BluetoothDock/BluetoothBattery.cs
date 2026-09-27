using System.Runtime.InteropServices;

namespace BluetoothDock;

/// <summary>
/// Reads the Bluetooth accessory battery percentage Windows Settings shows,
/// from DEVPKEY_Bluetooth_BatteryLevel on present BTHENUM service nodes
/// (typically the Hands-Free profile).
/// </summary>
static class BluetoothBattery
{
    private static readonly DevPropKey BatteryLevelKey = new(new Guid("104EA319-6EE2-4701-BD47-8DDBF425BBE5"), 2);
    private static readonly DevPropKey ContainerIdKey = new(new Guid("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C"), 2);

    public static IReadOnlyDictionary<Guid, byte> ReadLevels()
    {
        var levels = new Dictionary<Guid, byte>();
        try
        {
            IntPtr filter = Marshal.StringToHGlobalUni("BTHENUM");
            try
            {
                uint flags = CfgMgr32.CM_GETIDLIST_FILTER_ENUMERATOR | CfgMgr32.CM_GETIDLIST_FILTER_PRESENT;
                uint size = 0;
                if (CfgMgr32.CM_Get_Device_ID_List_SizeW(ref size, filter, flags) != CfgMgr32.CR_SUCCESS
                    || size == 0)
                {
                    return levels;
                }

                var buffer = new char[size];
                if (CfgMgr32.CM_Get_Device_ID_ListW(filter, buffer, size, flags) != CfgMgr32.CR_SUCCESS)
                    return levels;

                for (int i = 0; i < buffer.Length && buffer[i] != '\0';)
                {
                    int end = i;
                    while (end < buffer.Length && buffer[end] != '\0')
                        end++;

                    string id = new(buffer, i, end - i);
                    i = end + 1;
                    if (id.Length == 0)
                        break;

                    if (CfgMgr32.CM_Locate_DevNodeW(out uint devInst, id, 0) != CfgMgr32.CR_SUCCESS)
                        continue;

                    DevPropKey batteryKey = BatteryLevelKey;
                    byte battery = 0;
                    uint propSize = 1;
                    if (CfgMgr32.CM_Get_DevNode_PropertyW(
                            devInst, ref batteryKey, out uint propType, ref battery, ref propSize, 0)
                        != CfgMgr32.CR_SUCCESS
                        || propType != CfgMgr32.DEVPROP_TYPE_BYTE
                        || propSize != 1)
                    {
                        continue;
                    }

                    DevPropKey containerKey = ContainerIdKey;
                    Guid container = Guid.Empty;
                    propSize = (uint)Marshal.SizeOf<Guid>();
                    if (CfgMgr32.CM_Get_DevNode_PropertyW(
                            devInst, ref containerKey, out propType, ref container, ref propSize, 0)
                        != CfgMgr32.CR_SUCCESS
                        || propType != CfgMgr32.DEVPROP_TYPE_GUID
                        || container == Guid.Empty)
                    {
                        continue;
                    }

                    // Prefer the first value; HFP AG and related nodes share the container.
                    levels.TryAdd(container, battery);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(filter);
            }
        }
        catch
        {
        }

        return levels;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DevPropKey
    {
        public Guid Fmtid;
        public uint Pid;

        public DevPropKey(Guid fmtid, uint pid)
        {
            Fmtid = fmtid;
            Pid = pid;
        }
    }

    private static class CfgMgr32
    {
        public const int CR_SUCCESS = 0;
        public const uint CM_GETIDLIST_FILTER_ENUMERATOR = 0x00000001;
        public const uint CM_GETIDLIST_FILTER_PRESENT = 0x00000100;
        public const uint DEVPROP_TYPE_BYTE = 0x00000003;
        public const uint DEVPROP_TYPE_GUID = 0x0000000D;

        [DllImport("cfgmgr32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CM_Get_Device_ID_List_SizeW(ref uint pulLen, IntPtr pszFilter, uint ulFlags);

        [DllImport("cfgmgr32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CM_Get_Device_ID_ListW(IntPtr pszFilter, [Out] char[] buffer, uint bufferLen, uint ulFlags);

        [DllImport("cfgmgr32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CM_Locate_DevNodeW(out uint pdnDevInst, string pDeviceID, uint ulFlags);

        [DllImport("cfgmgr32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CM_Get_DevNode_PropertyW(
            uint dnDevInst,
            ref DevPropKey PropertyKey,
            out uint PropertyType,
            ref byte PropertyBuffer,
            ref uint PropertyBufferSize,
            uint ulFlags);

        [DllImport("cfgmgr32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CM_Get_DevNode_PropertyW(
            uint dnDevInst,
            ref DevPropKey PropertyKey,
            out uint PropertyType,
            ref Guid PropertyBuffer,
            ref uint PropertyBufferSize,
            uint ulFlags);
    }
}
