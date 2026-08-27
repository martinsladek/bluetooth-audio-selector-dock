namespace BluetoothDock;

static class AppPaths
{
    public static string DataDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Strings.ConfigFolderName);

    public static string ConfigFile => Path.Combine(DataDirectory, "config.json");

    public static string InstalledExe => Path.Combine(DataDirectory, "BluetoothDock.exe");

    public static string LegacyConfigFile =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Strings.ConfigFolderName,
            "config.json");

    public static string CurrentExe =>
        Environment.ProcessPath
        ?? throw new InvalidOperationException("The current executable path is unknown.");

    public static bool IsRunningFromInstallLocation =>
        PathsEqual(CurrentExe, InstalledExe);

    public static bool PathsEqual(string a, string b) =>
        string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
}
