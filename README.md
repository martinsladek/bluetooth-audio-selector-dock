# BluetoothDock

Ikona v oznamovací oblasti Windows 10 pro rychlé připojení a odpojení Bluetooth sluchátek.

- **Levé klepnutí** — připojit / odpojit vybrané zařízení
- **Pravé klepnutí** — výběr spárovaného Bluetooth audio zařízení
- **Hover** — název a stav

## Spuštění

Přenosná binárka: `dist/BluetoothDock.exe` (self-contained, bez instalace .NET).

Sluchátka musí být už spárovaná ve Windows. Výběr zařízení se ukládá do `%AppData%\BluetoothDock\config.json`.

## Sestavení

```powershell
dotnet publish src/BluetoothDock/BluetoothDock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

Vyžaduje .NET 8 SDK. Aplikace volá Windows Core Audio (`KSPROPSETID_BtAudio`), žádné cizí Bluetooth knihovny.
