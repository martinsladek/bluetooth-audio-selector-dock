# Agent notes

## Releases

Do **not** attach a locally built EXE to GitHub Releases and do not commit `dist/`.

GitHub Actions publishes `BluetoothDock.exe` when a tag matching `v*` is pushed (see `.github/workflows/release.yml`). The stable URL is `/releases/latest/download/BluetoothDock.exe`.

To ship a build: commit to `main`, then `git tag vX.Y.Z` and `git push origin vX.Y.Z`.

After changes that affect the running app, `dotnet publish` to `dist/` and restart the tray process (`BluetoothDock.exe`) so the user can try it locally. That local EXE is only for this machine — never attach it to a GitHub Release.

## Build

.NET 8 SDK (`global.json`). Target `net8.0-windows10.0.19041.0`.
