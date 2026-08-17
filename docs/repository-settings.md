# Empfohlene GitHub-Einstellungen

Beschreibung, Topics und Schutzregeln sind GitHub-Metadaten und können nicht
durch Dateien im Repository gesetzt werden. Für das öffentliche Repository sind
folgende Werte vorgesehen.

## Beschreibung

> Simple Windows tool to freeze a selected monitor for tactical situation
> displays and physical whiteboard overlays.

## Topics

```text
windows
csharp
dotnet
wpf
multi-monitor
screenshot
fullscreen
fire-department
emergency-services
situation-display
```

## Repository und Actions

- Issues aktivieren, damit die vorbereiteten Formulare nutzbar sind.
- Private Vulnerability Reporting aktivieren, damit Meldungen aus `SECURITY.md`
  vertraulich eingehen können.
- Für `main` einen Pull Request und den erfolgreichen Check
  **Build / Build and test (.NET 8)** verlangen.
- GitHub Actions erlauben, mit dem automatisch bereitgestellten `GITHUB_TOKEN`
  Releases zu schreiben. Der Release-Workflow beschränkt sich selbst auf
  `contents: write`.
- Tags nach dem Schema `vMAJOR.MINOR.PATCH` schützen und nur nach abgeschlossener
  manueller Testmatrix anlegen.
- Optional die GitHub-Funktion für unveränderliche Releases aktivieren, sobald
  der Release-Ablauf einmal vollständig erprobt wurde.

GitHub soll die gewählte MIT-Lizenz aus der Datei [`LICENSE`](../LICENSE)
erkennen. Ein zusätzliches License-Topic ist nicht erforderlich.
