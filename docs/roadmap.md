# Roadmap

## Stand der 0.1.0-Vorabversion

Die Funktionen aus Phase 1 und Phase 2 einschließlich der kompakten Oberfläche,
dunklen Dropdown-Popups und des konfigurierbaren `EINGEFROREN`-Hinweises sind im
aktuellen Entwicklungsstand implementiert. Build, automatisierte Tests,
Installer und Release-Automatisierung sind vorbereitet. Vor einer stabilen
Freigabe fehlen noch die vollständige manuelle Hardwarematrix, geprüfte
Screenshots und die Abnahme von Installation, Update und Deinstallation auf
einem sauberen Windows-System.

## Phase 1 – stabiles MVP

- Solution, WPF-Anwendung und fokussiertes Testprojekt
- Monitorerkennung, Auswahl, Identifikation und gespeicherte Auswahl
- Freeze, Live und Refresh mit verpflichtender Prüfung auf realer Hardware
- korrekte DPI- und Multi-Monitor-Behandlung
- globale Hotkeys
- verständliche Fehler und lokales Logging
- manuelle Testmatrix für reale Monitorhardware

## Phase 2 – Bedienkomfort (in 0.1.0 integriert)

- System-Tray
- übersichtliche Einstellungen
- Screenshot-Export
- Zeichenmodus und Bildanpassung
- optionaler Autostart
- kompaktere Hauptansicht, Monitorauswahl und Einstellungen
- vollständig dunkle ComboBoxen einschließlich Dropdown-Popup
- optionaler `EINGEFROREN`-Hinweis mit vier Eckpositionen

## Phase 3 – Distribution (technisch vorbereitet)

- Inno-Setup-Installer
- self-contained x64 Build und optionale Portable-Version
- Build- und Release-Workflows für GitHub Actions
- Checksums und anwenderfreundliche Release Notes
- Issue- und Pull-Request-Vorlagen sowie Beitrags- und Sicherheitshinweise

## Versionsschema

Semantic Versioning mit Tags wie `v0.1.0`: Minor-Versionen bringen Funktionen,
Patch-Versionen Fehlerkorrekturen und `1.0.0` bezeichnet die erste stabile
Version. `main` bleibt die einfache Hauptlinie; größere Änderungen erfolgen über
kurzlebige Feature-Branches und Pull Requests.

## Bewusst nicht im MVP

Cloud-Dienste, Telemetrie, komplexe Navigation, automatisches Update-System,
umfangreiche Bildbearbeitung, anwendungsspezifische Integrationen und eine
allgemeine Lageführungsplattform.
