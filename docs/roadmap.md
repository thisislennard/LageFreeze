# Roadmap

## Stand von 1.0.0

Die Funktionen aus Phase 1 und Phase 2 einschließlich der kompakten Oberfläche,
dunklen Dropdown-Popups und des konfigurierbaren `EINGEFROREN`-Hinweises sind im
Funktionsumfang der ersten stabilen Version enthalten und implementiert. Build,
automatisierte Tests, Installer und Release-Automatisierung sind eingerichtet;
die automatisierten Prüfungen und die bislang beobachteten Smoke-Checks sind
erfolgreich. Die vollständige manuelle Hardwarematrix ist noch nicht für alle
aufgeführten Konfigurationen dokumentiert. Vor dem operativen Einsatz müssen die
jeweilige Zielhardware sowie Installation, Update und Deinstallation in dieser
Umgebung geprüft werden. Verifizierte Screenshots bleiben eine offene
Dokumentationsaufgabe und sind kein Ersatz für diese Abnahme.

## Phase 1 – Kernfunktionen (in 1.0.0 enthalten)

- Solution, WPF-Anwendung und fokussiertes Testprojekt
- Monitorerkennung, Auswahl, Identifikation und gespeicherte Auswahl
- Freeze, Live und Refresh mit verpflichtender Prüfung auf realer Hardware
- korrekte DPI- und Multi-Monitor-Behandlung
- globale Hotkeys
- verständliche Fehler und lokales Logging
- manuelle Testmatrix für reale Monitorhardware

## Phase 2 – Bedienkomfort (in 1.0.0 enthalten)

- System-Tray
- übersichtliche Einstellungen
- Screenshot-Export
- Zeichenmodus und Bildanpassung
- optionaler Autostart
- kompaktere Hauptansicht, Monitorauswahl und Einstellungen
- vollständig dunkle ComboBoxen einschließlich Dropdown-Popup
- optionaler `EINGEFROREN`-Hinweis mit vier Eckpositionen

## Phase 3 – Distribution (in 1.0.0 enthalten)

- Inno-Setup-Installer
- self-contained x64 Build und optionale Portable-Version
- Build- und Release-Workflows für GitHub Actions
- Checksums und anwenderfreundliche Release Notes
- Issue- und Pull-Request-Vorlagen sowie Beitrags- und Sicherheitshinweise

## Versionsschema

Semantic Versioning mit Tags wie `v1.0.0`: `1.0.0` bildet die erste stabile
Basis, Minor-Versionen bringen abwärtskompatible Funktionen und Patch-Versionen
abwärtskompatible Fehlerkorrekturen. `main` bleibt die einfache Hauptlinie;
größere Änderungen erfolgen über kurzlebige Feature-Branches und Pull Requests.

## Bewusst nicht in 1.0.0

Cloud-Dienste, Telemetrie, komplexe Navigation, automatisches Update-System,
umfangreiche Bildbearbeitung, anwendungsspezifische Integrationen und eine
allgemeine Lageführungsplattform.
