# Bluetooth Audio Selector Dock

A Windows 10 system tray utility for quickly connecting and disconnecting Bluetooth headphones.

- **Left click** — connect or disconnect the selected device
- **Right click** — choose which paired Bluetooth audio device the left click controls
- **Hover** — device name and status

The interface follows the Windows display language (`en`, `cs`, `de`, `fr`, `es`, `pl`, `sk`). Other languages fall back to English.

### Notes

- Headphones must already be paired in Windows.
- The selected device is stored in `%LocalAppData%\BluetoothDock\config.json`.

### Download

**[Download here](https://github.com/martinsladek/bluetooth-audio-selector-dock/releases/latest/download/BluetoothDock.exe)** — portable Windows 10 x64 executable (self-contained, no .NET install required).

### Building

```powershell
dotnet publish src/BluetoothDock/BluetoothDock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

Requires the .NET 8 SDK. The app talks to Windows Core Audio (`KSPROPSETID_BtAudio`) and does not use third-party Bluetooth libraries.

Pushing a version tag (`v0.2.1`, `v1.0.0`, …) runs GitHub Actions: it publishes `BluetoothDock.exe` and creates a GitHub Release. The download link above always follows the latest non-prerelease Release.

### Recreate from the idea

[SPEC.md](SPEC.md) is the full product contract. Give that file to a coding agent and ask it to implement the specification.
