using System.Globalization;

namespace BluetoothDock;

static class Strings
{
    private static readonly string Lang =
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

    public const string ProductName = "Bluetooth Audio Selector Dock";
    public const string ConfigFolderName = "BluetoothDock";

    public const string WebsiteUrl = "https://www.martinsladek.com/";
    public const string GitHubUrl = "https://github.com/martinsladek/bluetooth-audio-selector-dock";
    public const string DownloadUrl = "https://github.com/martinsladek/bluetooth-audio-selector-dock/releases/latest/download/BluetoothDock.exe";

    public static string AppName => ProductName;

    public static string BluetoothSettings => L(
        en: "Bluetooth settings",
        cs: "Nastavení Bluetooth",
        de: "Bluetooth-Einstellungen",
        fr: "Paramètres Bluetooth",
        es: "Configuración de Bluetooth",
        pl: "Ustawienia Bluetooth",
        sk: "Nastavenia Bluetooth");

    public static string StartWithWindows => L(
        en: "Start with Windows",
        cs: "Spouštět s Windows",
        de: "Mit Windows starten",
        fr: "Démarrer avec Windows",
        es: "Iniciar con Windows",
        pl: "Uruchamiaj z systemem Windows",
        sk: "Spúšťať so systémom Windows");

    public static string About => L(
        en: "About",
        cs: "O aplikaci",
        de: "Info",
        fr: "À propos",
        es: "Acerca de",
        pl: "Informacje",
        sk: "O aplikácii");

    public static string Exit => L(
        en: "Exit",
        cs: "Ukončit",
        de: "Beenden",
        fr: "Quitter",
        es: "Salir",
        pl: "Zakończ",
        sk: "Ukončiť");

    public static string Ok => "OK";

    public static string SelectDevice => L(
        en: "Select a device",
        cs: "Vyberte zařízení",
        de: "Gerät auswählen",
        fr: "Sélectionner un appareil",
        es: "Seleccionar un dispositivo",
        pl: "Wybierz urządzenie",
        sk: "Vyberte zariadenie");

    public static string BluetoothOff => L(
        en: "Bluetooth is off",
        cs: "Bluetooth je vypnutý",
        de: "Bluetooth ist ausgeschaltet",
        fr: "Bluetooth est désactivé",
        es: "Bluetooth está desactivado",
        pl: "Bluetooth jest wyłączony",
        sk: "Bluetooth je vypnutý");

    public static string NoDevices => L(
        en: "No paired Bluetooth headphones",
        cs: "Žádná spárovaná Bluetooth sluchátka",
        de: "Keine gekoppelten Bluetooth-Kopfhörer",
        fr: "Aucun casque Bluetooth associé",
        es: "No hay auriculares Bluetooth emparejados",
        pl: "Brak sparowanych słuchawek Bluetooth",
        sk: "Žiadne spárované Bluetooth slúchadlá");

    public static string ConnectFailed => L(
        en: "Could not connect",
        cs: "Připojení se nezdařilo",
        de: "Verbindung fehlgeschlagen",
        fr: "Impossible de se connecter",
        es: "No se pudo conectar",
        pl: "Nie udało się połączyć",
        sk: "Pripojenie sa nepodarilo");

    public static string DisconnectFailed => L(
        en: "Could not disconnect",
        cs: "Odpojení se nezdařilo",
        de: "Trennen fehlgeschlagen",
        fr: "Impossible de se déconnecter",
        es: "No se pudo desconectar",
        pl: "Nie udało się rozłączyć",
        sk: "Odpojenie sa nepodarilo");

    public static string AutostartFailed => L(
        en: "Could not change Start with Windows",
        cs: "Spouštění s Windows se nepodařilo nastavit",
        de: "„Mit Windows starten“ konnte nicht geändert werden",
        fr: "Impossible de modifier « Démarrer avec Windows »",
        es: "No se pudo cambiar «Iniciar con Windows»",
        pl: "Nie udało się zmienić opcji „Uruchamiaj z systemem Windows”",
        sk: "Spúšťanie so systémom Windows sa nepodarilo nastaviť");

    public static string NotAvailable => L(
        en: "unavailable",
        cs: "není dostupné",
        de: "nicht verfügbar",
        fr: "indisponible",
        es: "no disponible",
        pl: "niedostępne",
        sk: "nie je dostupné");

    public static string Connected => L(
        en: "connected",
        cs: "připojeno",
        de: "verbunden",
        fr: "connecté",
        es: "conectado",
        pl: "połączono",
        sk: "pripojené");

    public static string Disconnected => L(
        en: "disconnected",
        cs: "odpojeno",
        de: "getrennt",
        fr: "déconnecté",
        es: "desconectado",
        pl: "rozłączono",
        sk: "odpojené");

    public static string Connecting => L(
        en: "connecting…",
        cs: "připojuji…",
        de: "verbinden…",
        fr: "connexion…",
        es: "conectando…",
        pl: "łączenie…",
        sk: "pripájam…");

    public static string Disconnecting => L(
        en: "disconnecting…",
        cs: "odpojuji…",
        de: "trennen…",
        fr: "déconnexion…",
        es: "desconectando…",
        pl: "rozłączanie…",
        sk: "odpájam…");

    public static string AboutTagline => L(
        en: "A lightweight Windows desktop utility for quickly switching between Bluetooth audio devices.",
        cs: "Lehká desktopová utilita pro Windows pro rychlé přepínání mezi Bluetooth audio zařízeními.",
        de: "Ein schlankes Windows-Dienstprogramm zum schnellen Wechseln zwischen Bluetooth-Audiogeräten.",
        fr: "Un utilitaire Windows léger pour basculer rapidement entre des appareils audio Bluetooth.",
        es: "Una utilidad ligera de Windows para cambiar rápidamente entre dispositivos de audio Bluetooth.",
        pl: "Lekkie narzędzie pulpitu Windows do szybkiego przełączania między urządzeniami audio Bluetooth.",
        sk: "Ľahká desktopová utilita pre Windows na rýchle prepínanie medzi Bluetooth audio zariadeniami.");

    public static string AboutCredit => L(
        en: "Developed by Martin Sladek with the help of AI models and workflows.",
        cs: "Vytvořil Martin Sladek s pomocí AI modelů a vývojových postupů.",
        de: "Entwickelt von Martin Sladek mit Hilfe von KI-Modellen und Entwicklungsabläufen.",
        fr: "Développé par Martin Sladek avec l’aide de modèles d’IA et de flux de travail.",
        es: "Desarrollado por Martin Sladek con la ayuda de modelos de IA y flujos de trabajo.",
        pl: "Stworzone przez Martina Sladka z pomocą modeli AI i procesów deweloperskich.",
        sk: "Vytvoril Martin Sladek s pomocou AI modelov a vývojových postupov.");

    public static string Website => L(
        en: "Website",
        cs: "Web",
        de: "Webseite",
        fr: "Site web",
        es: "Sitio web",
        pl: "Witryna",
        sk: "Web");

    public static string GitHub => "GitHub";

    public static string Download => L(
        en: "Download",
        cs: "Stáhnout",
        de: "Herunterladen",
        fr: "Télécharger",
        es: "Descargar",
        pl: "Pobierz",
        sk: "Stiahnuť");

    private static string L(
        string en, string cs, string de, string fr, string es, string pl, string sk) =>
        Lang switch
        {
            "cs" => cs,
            "de" => de,
            "fr" => fr,
            "es" => es,
            "pl" => pl,
            "sk" => sk,
            _ => en
        };
}
