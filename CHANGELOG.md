# Changelog

Alle nennenswerten Änderungen an LageFreeze werden in dieser Datei dokumentiert.
Das Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.1.0/),
die Versionierung folgt [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Added

- Noch keine neuen Funktionen.

### Changed

- Noch keine Änderungen.

### Fixed

- Noch keine Fehlerbehebungen.

## [1.0.0] - 2026-08-17

### Changed

- Erster stabil versionierter Release des in der Beta-Reihe aufgebauten
  Funktionsumfangs.
- Fußzeile des Hauptfensters auf die beiden relevanten Tastenkürzel `F9` und
  `F10` reduziert; der redundante Hinweis zum lokalen Betrieb bleibt in der
  Datenschutzdokumentation erhalten.

## [0.1.0-beta.3] - 2026-08-17

### Added

- Konfigurierbarer, standardmäßig oben rechts sichtbarer
  `EINGEFROREN`-Hinweis auf dem Zielmonitor; er lässt sich deaktivieren oder in
  einer der vier Bildschirmecken positionieren.
- Automatisierte Tests für Standardwerte, Persistenz, Validierung, alle vier
  Hinweispositionen und das vollständige Ausblenden des Hinweises.

### Changed

- Hauptansicht, Monitorauswahl und Einstellungen kompakter, ruhiger und
  übersichtlicher gestaltet.
- ComboBoxen einschließlich Dropdown-Popup, Auswahlzuständen und Scrollbar
  vollständig an das dunkle Oberflächendesign angepasst.
- Programmsymbol zu einer klareren, auch in kleinen Windows-Größen gut
  erkennbaren Form vereinfacht.

### Fixed

- Beim Aktualisieren wird der sichtbare `EINGEFROREN`-Hinweis zusammen mit dem
  Overlay ausgeblendet und dadurch nicht in das neue Standbild aufgenommen.

## [0.1.0-beta.2] - 2026-08-17

### Added

- Eigenes, mehrstufiges LageFreeze-App-Symbol für Anwendung, Fenster,
  System-Tray, Portable-Paket und Installer.
- STA-Smoke-Tests, die Hauptfenster, Monitorauswahl und Einstellungen samt
  kompiliertem XAML initialisieren.

### Changed

- Lizenzhinweise in README und Architekturdokumentation konsistent auf die
  gewählte MIT-Lizenz aktualisiert.

### Fixed

- Fehlende Code-behind-Initialisierung der drei WPF-Fenster ergänzt, sodass
  deren kompiliertes XAML beim Erzeugen zuverlässig geladen wird.
- Fenstergebundene Hotkey- und Monitor-Hooks werden vor dem Zerstören des
  Hauptfensters entfernt; dadurch tritt beim Beenden kein Win32-Fehler 1400
  mehr auf.
- Icon-Ressourcen werden in den self-contained Publish eingebettet und vom
  Programm, Tray sowie Inno-Setup konsistent verwendet.

## [0.1.0-beta.1] - 2026-08-17

### Added

- Erste .NET-8-WPF-Anwendung für Windows 10 und 11 auf x64.
- Auswahl, Identifikation und lokales Wiedererkennen angeschlossener Monitore.
- Freeze-, Live- und Refresh-Ablauf über ein randloses Topmost-Fenster.
- Globale Standard-Hotkeys `F9` und `F10`.
- Lokale Einstellungen, tägliche Logs und benutzerfreundliche Fehlerbehandlung.
- Konfigurierbare Hotkeys, System-Tray sowie optionaler Autostart und minimierter Start.
- PNG-Export und drei Darstellungsmodi für Whiteboard-Markierungen.
- Automatisierte Build-, Test- und Tag-Release-Workflows.
- Self-contained Portable-ZIP, Inno-Setup-Installer und SHA-256-Prüfsummen.
- Dokumentation, manuelle Testmatrix sowie Issue- und Pull-Request-Vorlagen.

### Changed

- Noch keine Änderungen.

### Fixed

- Noch keine Fehlerbehebungen.

[Unreleased]: https://github.com/thisislennard/LageFreeze/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/thisislennard/LageFreeze/compare/v0.1.0-beta.3...v1.0.0
[0.1.0-beta.3]: https://github.com/thisislennard/LageFreeze/compare/v0.1.0-beta.2...v0.1.0-beta.3
[0.1.0-beta.2]: https://github.com/thisislennard/LageFreeze/compare/v0.1.0-beta.1...v0.1.0-beta.2
[0.1.0-beta.1]: https://github.com/thisislennard/LageFreeze/releases/tag/v0.1.0-beta.1
