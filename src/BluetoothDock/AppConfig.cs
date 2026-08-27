using System.Text.Json;

namespace BluetoothDock;

sealed class AppConfig
{
    public string? ContainerId { get; set; }
    public string? DeviceName { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public Guid? ParsedContainerId =>
        Guid.TryParse(ContainerId, out Guid id) ? id : null;

    public static AppConfig Load()
    {
        try
        {
            MigrateLegacyConfig();
            string path = AppPaths.ConfigFile;
            if (!File.Exists(path))
                return new AppConfig();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save()
    {
        string path = AppPaths.ConfigFile;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }

    private static void MigrateLegacyConfig()
    {
        string current = AppPaths.ConfigFile;
        string legacy = AppPaths.LegacyConfigFile;
        if (File.Exists(current) || !File.Exists(legacy))
            return;

        Directory.CreateDirectory(AppPaths.DataDirectory);
        File.Move(legacy, current, overwrite: false);

        try
        {
            string? legacyDir = Path.GetDirectoryName(legacy);
            if (legacyDir is not null && Directory.Exists(legacyDir) && !Directory.EnumerateFileSystemEntries(legacyDir).Any())
                Directory.Delete(legacyDir);
        }
        catch
        {
        }
    }
}
