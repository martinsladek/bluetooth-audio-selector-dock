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

    private static string FilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Strings.ConfigFolderName,
            "config.json");

    public Guid? ParsedContainerId =>
        Guid.TryParse(ContainerId, out Guid id) ? id : null;

    public static AppConfig Load()
    {
        try
        {
            string path = FilePath;
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
        string path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }
}
