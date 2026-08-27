# Bluetooth Audio Selector Dock — specification

Hand this file to a coding agent with: **Implement this specification on Windows 10.**

This is the product contract, not a chat log. Follow the decisions below. Do not resurrect rejected ideas from the “Out of scope” section.

## Goal

A tiny Windows 10 desktop utility that lives only in the **notification area** (system tray). It connects and disconnects **already paired** Bluetooth headphones with one left click. There is no main window.

| Item | Value |
|---|---|
| Product name | Bluetooth Audio Selector Dock |
| Assembly / EXE name | `BluetoothDock.exe` |
| Config / install folder | `%LocalAppData%\BluetoothDock\` (stable, never localized) |
| Author | Martin Sladek |
| Website | https://www.martinsladek.com/ |
| Repository | https://github.com/martinsladek/bluetooth-audio-selector-dock |
| Download (immutable) | https://github.com/martinsladek/bluetooth-audio-selector-dock/releases/latest/download/BluetoothDock.exe |

Headphones must already be paired in Windows Settings. This app does **not** pair, unpair, or scan for unpaired devices.

## Behavior

### Icon — three visual states

Draw simple 32×32 tray icons at runtime (headphone glyph on a circle). Do not ship third-party icon assets.

| State | Look | Meaning |
|---|---|---|
| Disconnected | Grey filled circle, white glyph | Selected device is disconnected, or nothing is selected |
| Connected | Windows blue (`#0078D7`) filled circle, white glyph | Selected device is connected |
| Busy | Dim blue fill, blue ring, small yellow dot | Connect or disconnect in progress |

### Hover

Native tooltip only. No menu on hover. `NotifyIcon.Text` is limited to 63 characters — truncate.

Examples:

- `{device name} — connected`
- `{device name} — disconnected`
- `{device name} — connecting…`
- `{device name} — disconnecting…`
- `Bluetooth is off`
- `No paired Bluetooth headphones`
- `Select a device`
- `{device name} — unavailable`

### Left click — toggle only

| Situation | Action |
|---|---|
| Busy (connecting/disconnecting) | Ignore |
| Bluetooth radio off | Balloon: Bluetooth is off |
| No paired Bluetooth audio devices | Balloon: no devices; show the context menu |
| Nothing selected, or saved device missing from the current list | Balloon if useful; **show the same context menu as right-click** (not a special first-run wizard) |
| Selected device present | Toggle: disconnected → connect, connected → disconnect |

If exactly **one** paired Bluetooth audio device exists and nothing is saved yet, auto-select it and persist the choice. Do not auto-select when a saved device is missing.

Selecting a device in the menu only changes which device left-click controls. It does **not** connect or disconnect.

### Right click — native context menu

No flyout (custom popup above the taskbar). Use `ContextMenuStrip`.

```
✓  Sony WH-1000XM4  · connected
   JBL Flip 5
─────────────────
   Bluetooth settings
   Start with Windows  ✓
─────────────────
   About
   Exit
```

Rules:

- List **paired Bluetooth audio devices**, not “currently in range” (Windows often cannot know range until a connect attempt).
- Checkmark = the device that left-click toggles. Clicking another row switches the selection and saves it.
- Show a connected suffix on connected rows. Do **not** put a separate Connect/Disconnect command in the menu — that is left-click only.
- **Bluetooth settings** opens `ms-settings:bluetooth`.
- **Start with Windows** is a checkable setting (see Autostart). Read live registry state when the menu opens.
- **About** sits immediately above **Exit**.
- **Exit** hides the tray icon and quits.

If the list is empty, show a disabled “No paired Bluetooth headphones” row, then Settings / Start with Windows / About / Exit.

### About dialog

Standard modal WinForms dialog (`FixedDialog`, no maximize/minimize, not in the taskbar). Product name in bold, then:

**English**

> A lightweight Windows desktop utility for quickly switching between Bluetooth audio devices.
>
> Developed by Martin Sladek with the help of AI models and workflows.

**Czech** (only when Windows UI language is Czech)

> Lehká desktopová utilita pro Windows pro rychlé přepínání mezi Bluetooth audio zařízeními.
>
> Vytvořil Martin Sladek s pomocí AI modelů a vývojových postupů.

Clickable links:

| Label (en / cs) | URL |
|---|---|
| Website / Web | https://www.martinsladek.com/ |
| GitHub | https://github.com/martinsladek/bluetooth-audio-selector-dock |
| Download / Stáhnout | https://github.com/martinsladek/bluetooth-audio-selector-dock/releases/latest/download/BluetoothDock.exe |

OK button closes the dialog.

### Language

Read `CultureInfo.CurrentUICulture`. If the two-letter ISO code is `cs`, use Czech strings. Otherwise English. Product name stays English in both locales. Do not add other languages unless asked.

### Persistence

Both the optional installed EXE and settings live in one per-user, per-machine folder:

```
%LocalAppData%\BluetoothDock\BluetoothDock.exe
%LocalAppData%\BluetoothDock\config.json
```

`config.json`:

```json
{
  "containerId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "deviceName": "Sony WH-1000XM4"
}
```

Store the device **container GUID**, not only the name. Keep the folder name `BluetoothDock` even if the display name changes.

If an older build left `config.json` in `%AppData%\BluetoothDock\`, move it to LocalAppData on first load and remove the empty roaming folder.

Do not use Roaming: the EXE is large, and the selected device GUID is machine-specific.

### Autostart

Optional, off by default. No admin rights. Portable until the user opts in.

**Enable**

1. Copy the currently running EXE to `%LocalAppData%\BluetoothDock\BluetoothDock.exe` (skip if already running from that path).
2. Write `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value `BluetoothDock` = quoted path to that copy.
3. Write `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run` value `BluetoothDock` as enabled (`0x02…`), so Task Manager / Settings → Apps → Startup agree.

**Disable**

1. Delete the Run value and the StartupApproved value.
2. Delete the installed EXE if this process is **not** running from that path (the running file cannot be deleted). Keep `config.json`.

**Checkbox state**

Checked only if the Run value exists **and** StartupApproved does not mark it disabled (`0x03` / `0x07`). Task Manager “Disable” leaves Run in place; the menu must not show checked in that case. Checking the box again re-enables Approved.

**Updates**

On launch, if Run is registered and this process is a different file than the installed copy (size or last-write), overwrite the LocalAppData EXE so the next logon is not an old download.

### Process

Single instance via mutex `Local\BluetoothDock.SingleInstance`. A second launch exits silently.

## Technical stack (required)

| Layer | Choice |
|---|---|
| Language | C# |
| UI | WinForms, `ApplicationContext` + `NotifyIcon` (no main form) |
| Target | `net8.0-windows10.0.19041.0` |
| Output | `WinExe`, self-contained single-file `win-x64` |
| Bluetooth connect | Windows Core Audio + Kernel Streaming, **not** WinRT `AudioPlaybackConnection` |
| Device names | WinRT `DeviceInformation` with `DeviceInformationKind.DeviceContainer` when possible |
| Third-party NuGet for Bluetooth | **None** |

Publish:

```powershell
dotnet publish src/BluetoothDock/BluetoothDock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
```

The published EXE must run on a PC that has no .NET SDK and no extra runtimes installed.

Do not use trimming (`PublishTrimmed=false`) — COM interop will break.

No installer in v1. Distribution of the binary is **GitHub Releases**, never git.

## Connecting and disconnecting (the hard part)

Windows has no simple public `BluetoothConnect()` for A2DP headphones. The Settings app talks to the **audio driver**.

Documented property set (`ksmedia.h` / MSDN):

- `KSPROPSETID_BtAudio` = `{7FA06C40-B8F6-4C7E-8556-E8C33A12E54D}`
- `KSPROPERTY_ONESHOT_RECONNECT` = `0`
- `KSPROPERTY_ONESHOT_DISCONNECT` = `1`
- Flags: `KSPROPERTY_TYPE_GET`

User-mode path (same idea as [ToothTray](https://github.com/m2jean/ToothTray), implemented ourselves — do **not** depend on that repo as a library):

1. `IMMDeviceEnumerator.EnumAudioEndpoints(eAll, DEVICE_STATEMASK_ALL)`. Use `eAll`, not `eRender` only (HFP endpoints are otherwise missed on some Windows versions).
2. For each endpoint, walk `IDeviceTopology` → connector → `GetConnectedTo` → `IPart` → other topology `GetDeviceId`.
3. Keep filters whose device id contains `\?\bth` (covers `bthenum` and `bthhfenum`).
4. Activate `IKsControl` on that filter. Confirm Bluetooth audio with `KSPROPERTY_TYPE_BASICSUPPORT` on `KSPROPSETID_BtAudio`.
5. Group endpoints by `PKEY_Device_ContainerId`. Display name: device-container name from WinRT, else strip the Windows role prefix (`Headphones (X)` → `X`).
6. Connected = any endpoint in the group is `DEVICE_STATE_ACTIVE`.
7. Connect/disconnect: send the oneshot property to **all** KS controls in the container (A2DP and HFP).
8. After the request, poll for up to ~8 seconds until state matches. Busy icon meanwhile. On timeout, balloon “Could not connect” / “Could not disconnect”.

COM interfaces are declared in-process (P/Invoke / `ComImport`). No NAudio, no 32feet, no random GitHub Bluetooth helpers.

Subscribe to `IMMNotificationClient` so the icon updates when Windows or the headset changes state. Debounce ~250 ms. Do not poll forever on a timer except during the busy wait.

Bluetooth radio on/off: `Windows.Devices.Radios`. Call WinRT async APIs via `Task.Run` so the WinForms STA thread cannot deadlock.

## Suggested layout

```
src/BluetoothDock/
  BluetoothDock.csproj
  app.manifest
  Program.cs
  Strings.cs
  AppConfig.cs
  AppPaths.cs
  Autostart.cs
  TrayApplicationContext.cs
  TrayIcons.cs
  AboutForm.cs
  BluetoothAudioService.cs
  AudioCom.cs
  AudioEndpointWatcher.cs
```

`.gitignore`: `bin/`, `obj/`, `dist/`, `.vs/`, `*.user`.

README is English only. Human download link is the `/releases/latest/download/BluetoothDock.exe` URL above.

## Out of scope (do not implement)

These were considered and rejected:

- Custom tray **flyout** instead of a native context menu
- Separate Connect/Disconnect item in the right-click menu
- A unique “first left-click wizard” different from the normal menu
- Listing devices that are “in range” rather than paired audio endpoints
- Pairing / unpairing / discovering unpaired devices
- `AudioPlaybackConnection` (that makes the PC an A2DP **sink**)
- `BluetoothSetServiceState` / PnP disable-enable (unreliable, often needs admin)
- Java, Python, Node, C++ toolchains
- Putting the 70+ MB self-contained EXE into git
- Installer, multiple primary devices at once

No administrator rights are required.

## Definition of done

- Tray icon with three states, tooltip, left-click toggle, right-click menu as specified
- About dialog with the three links
- Czech UI only when Windows display language is Czech
- Optional Start with Windows via HKCU Run + StartupApproved, EXE copy only when enabled
- Self-contained `dist/BluetoothDock.exe` builds and runs without a local SDK
- Binary published as a GitHub Release asset named `BluetoothDock.exe` so the latest-download URL stays valid
- Source in git; `dist/` not in git
