# Agent notes

## Releases

Do **not** attach a locally built EXE to GitHub Releases and do not commit `dist/`.

GitHub Actions publishes `BluetoothDock.exe` when a tag matching `v*` is pushed (see `.github/workflows/release.yml`). The stable URL is `/releases/latest/download/BluetoothDock.exe`.

To ship a build: commit to `main`, then `git tag vX.Y.Z` and `git push origin vX.Y.Z`.

Local `dotnet publish` is fine for trying the tray app on this machine.

## Build

.NET 8 SDK (`global.json`). Target `net8.0-windows10.0.19041.0`.
