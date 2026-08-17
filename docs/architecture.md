# Technische Leitplanken

## Technologieentscheidung

Das MVP verwendet **C# mit .NET 8 und WPF**. .NET 8 ist für die erste stabile
Codebasis als LTS-Version festgeschrieben und wird über `global.json` auf den
aktuellen Patchstand der 8.0.4xx-SDK-Linie gebunden. WPF ist für eine kleine,
reine Windows-Desktop-Anwendung ausgereift, gut dokumentiert und benötigt weniger
zusätzliche Infrastruktur als WinUI. Windows Forms wird ausschließlich für
kleine, vorhandene Windows-Integrationen verwendet; es kommt kein zusätzliches
UI-Framework als Abhängigkeit hinzu.

Als Installer wird **Inno Setup 6** verwendet: Für ein einzelnes x64-Desktopprodukt
ist es einfacher zu warten als eine umfangreiche MSI/WiX-Struktur. Eine
self-contained `win-x64`-Veröffentlichung vermeidet eine separate
Runtime-Installation. Der Installer arbeitet standardmäßig ohne erhöhte Rechte
und installiert für den aktuellen Benutzer unter
`%LOCALAPPDATA%\Programs\LageFreeze`.

Diese Festlegungen dürfen geändert werden, wenn ein reproduzierbarer Prototyp
einen konkreten technischen Nachteil zeigt. Gründe werden hier dokumentiert.

## Vorgesehene Struktur

```text
src/LageFreeze/
  Views/
  ViewModels/
  Models/
  Services/
  Resources/
tests/LageFreeze.Tests/
installer/
docs/
.github/workflows/
```

Nur tatsächlich benötigte Verzeichnisse und Abstraktionen werden angelegt.
Vorgesehene Verantwortlichkeiten sind Monitorerkennung, Bildschirmaufnahme,
Freeze-Overlay, globale Hotkeys, Einstellungen, Logging und optionaler
Screenshot-Export.

## Kritische technische Regeln

- Prozessweit Per-Monitor-V2-DPI-Awareness verwenden.
- Monitorgrenzen in physischen Pixeln und inklusive negativer Koordinaten
  behandeln.
- Das Freeze-Overlay ist ein normales, randloses Topmost-Fenster auf den exakten
  Monitorgrenzen; keine Manipulation von Treibern, DWM oder Anzeigeeinstellungen.
- Beim Schließen und bei unbehandelten Fehlern alle Identifikations- und
  Freeze-Fenster schließen.
- Displayänderungen über Windows-Ereignisse erkennen und Zustände neu validieren.
- Globale Hotkeys über die dafür vorgesehenen Win32-APIs registrieren und beim
  Beenden zuverlässig freigeben.
- Windows-Interop klein halten und hinter klar benannten Services bündeln.

## Capture- und Refresh-Entscheidung

Die Aufnahme verwendet die stabilen GDI-APIs `GetDC`,
`CreateCompatibleBitmap` und `BitBlt` mit `SRCCOPY | CAPTUREBLT`. Aufgenommen
werden die exakten physischen Grenzen des Zielmonitors; der Mauszeiger wird
nicht in die Aufnahme komponiert. Die Interop-Ressourcen werden unmittelbar
nach jeder Aufnahme freigegeben.

Ein sichtbares Topmost-Overlay würde beim Aktualisieren selbst erneut
aufgenommen. Der Refresh-Koordinator blendet das Overlay deshalb kontrolliert
kurz aus, lässt Windows die Änderung darstellen, nimmt den tatsächlichen Inhalt
dahinter auf, tauscht das Bild und zeigt das Overlay wieder. Dieser einfache
Ablauf vermeidet zusätzliche Capture-Frameworks. Sichtbares Flackern und
unterschiedliches Verhalten von GPU-beschleunigten oder geschützten Inhalten
bleiben hardwareabhängig und sind Teil der manuellen Testmatrix.

## Lokale Persistenz und Diagnose

Einstellungen liegen als lesbares UTF-8-JSON unter
`%LOCALAPPDATA%\LageFreeze\settings.json`. Sie werden zunächst in eine temporäre
Datei geschrieben und anschließend atomar ersetzt, damit ein Abbruch keine halb
geschriebene Konfiguration hinterlässt.

Für das Wiedererkennen eines Monitors hat die Windows Display-Interface-ID
Vorrang. Falls sie sich geändert hat oder nicht verfügbar ist, bewertet ein
gewichteter Fallback Gerätename, Anzeigename, Auflösung und Position. Zu schwache
oder mehrdeutige Treffer führen bewusst zu einer erneuten Auswahl statt zu einem
Freeze auf dem falschen Monitor.

Das Logging ist eine kleine, dependency-freie Implementierung. Pro lokalem Tag
entsteht `%LOCALAPPDATA%\LageFreeze\Logs\LageFreeze-YYYY-MM-DD.log`; Fehler beim
Logging dürfen die Anwendung nicht beenden. Dateien, die älter als 30 Tage sind,
werden bestmöglich bereinigt.

Explizit gespeicherte Standbilder werden als PNG standardmäßig unter
`Bilder\LageFreeze` abgelegt. Der Zielordner ist konfigurierbar; es findet kein
automatischer Export statt.

## Optionale Windows-Integration

Das System-Tray verwendet das im .NET-Windows-Desktop-Framework enthaltene
`NotifyIcon`; dafür wird keine zusätzliche UI-Bibliothek eingebunden. Autostart
ist standardmäßig deaktiviert und wird nur auf ausdrücklichen Benutzerwunsch im
`Run`-Schlüssel des aktuellen Benutzers eingetragen. Es werden weder erhöhte
Rechte noch maschinenweite Registry-Einträge benötigt.

## Noch nicht entschieden

- konkrete Open-Source-Lizenz (MIT, Apache-2.0 oder GPLv3)
