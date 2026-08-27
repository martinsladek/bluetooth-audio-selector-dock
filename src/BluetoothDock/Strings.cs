using System.Globalization;

namespace BluetoothDock;

static class Strings
{
    private static bool Czech =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("cs", StringComparison.OrdinalIgnoreCase);

    public const string ProductName = "Bluetooth Audio Selector Dock";
    public const string ConfigFolderName = "BluetoothDock";

    public const string WebsiteUrl = "https://www.martinsladek.com/";
    public const string GitHubUrl = "https://github.com/martinsladek/bluetooth-audio-selector-dock";
    public const string DownloadUrl = "https://github.com/martinsladek/bluetooth-audio-selector-dock/releases/latest/download/BluetoothDock.exe";

    public static string AppName => ProductName;

    public static string BluetoothSettings => Czech ? "Nastavení Bluetooth" : "Bluetooth settings";
    public static string StartWithWindows => Czech ? "Spouštět s Windows" : "Start with Windows";
    public static string About => Czech ? "O aplikaci" : "About";
    public static string Exit => Czech ? "Ukončit" : "Exit";
    public static string Ok => "OK";
    public static string SelectDevice => Czech ? "Vyberte zařízení" : "Select a device";
    public static string BluetoothOff => Czech ? "Bluetooth je vypnutý" : "Bluetooth is off";
    public static string NoDevices => Czech ? "Žádná spárovaná Bluetooth sluchátka" : "No paired Bluetooth headphones";
    public static string ConnectFailed => Czech ? "Připojení se nezdařilo" : "Could not connect";
    public static string DisconnectFailed => Czech ? "Odpojení se nezdařilo" : "Could not disconnect";
    public static string AutostartFailed => Czech ? "Spouštění s Windows se nepodařilo nastavit" : "Could not change Start with Windows";
    public static string NotAvailable => Czech ? "není dostupné" : "unavailable";
    public static string Connected => Czech ? "připojeno" : "connected";
    public static string Disconnected => Czech ? "odpojeno" : "disconnected";
    public static string Connecting => Czech ? "připojuji…" : "connecting…";
    public static string Disconnecting => Czech ? "odpojuji…" : "disconnecting…";

    public static string AboutTagline => Czech
        ? "Lehká desktopová utilita pro Windows pro rychlé přepínání mezi Bluetooth audio zařízeními."
        : "A lightweight Windows desktop utility for quickly switching between Bluetooth audio devices.";

    public static string AboutCredit => Czech
        ? "Vytvořil Martin Sladek s pomocí AI modelů a vývojových postupů."
        : "Developed by Martin Sladek with the help of AI models and workflows.";

    public static string Website => Czech ? "Web" : "Website";
    public static string GitHub => "GitHub";
    public static string Download => Czech ? "Stáhnout" : "Download";
}
