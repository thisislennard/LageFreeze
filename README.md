# LageFreeze

<p align="center">
  <img src="src/LageFreeze/Assets/LageFreeze.png" alt="LageFreeze-Programmsymbol" width="144">
</p>

[![Build](https://github.com/thisislennard/LageFreeze/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/thisislennard/LageFreeze/actions/workflows/build.yml)
[![Aktuelle Version](https://img.shields.io/github/v/release/thisislennard/LageFreeze?display_name=tag&sort=semver)](https://github.com/thisislennard/LageFreeze/releases/latest)
![Plattform](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4)
![Lizenz](https://img.shields.io/github/license/thisislennard/LageFreeze)

LageFreeze friert den sichtbaren Inhalt eines ausgewählten Windows-Monitors als
pixelgenaues, randloses Standbild ein. Desktop und Anwendungen laufen darunter
weiter und sind mit einem Tastendruck sofort wieder live sichtbar.

> **Projektstatus:** Der Funktionsumfang der geplanten `0.1.0`-Vorabversion ist
> implementiert. Die verpflichtende Validierung auf realer Multi-Monitor- und
> DPI-Hardware ist noch offen; deshalb gibt es noch keine stabile Freigabe.

## Download

Die aktuelle veröffentlichte Version steht unter
**[GitHub Releases](https://github.com/thisislennard/LageFreeze/releases/latest)**
bereit:

- `LageFreeze-Setup-x64.exe` – empfohlene Installation mit Startmenüeintrag
- `LageFreeze-Portable-x64.zip` – portable Variante ohne Installation
- `SHA256SUMS.txt` – SHA-256-Prüfsummen beider Downloads

Für den normalen Einsatz wird die Installer-Version empfohlen. Falls noch kein
Release angezeigt wird, gibt es derzeit ausschließlich den Entwicklungsstand im
Repository.

## Was ist LageFreeze?

Auf einem externen Monitor kann beispielsweise eine Karte oder ein Lagebild
angezeigt werden. Vor dem Bildschirm lässt sich eine transparente
Plexiglasscheibe anbringen, auf der taktische Informationen mit
Whiteboard-Markern eingezeichnet werden. Damit die Markierungen nicht durch eine
Änderung des digitalen Bildes ihre Bedeutung verlieren, legt LageFreeze ein
Standbild über genau diesen Monitor.

LageFreeze verändert weder den Grafiktreiber noch die Windows-Anzeigeeinstellungen.
Der Freeze besteht nur aus einem normalen Vollbildfenster, das beim Wechsel zu
Live oder beim Beenden wieder geschlossen wird.

## Funktionsweise

1. Außenmonitor auswählen und bei Bedarf über die großen eingeblendeten Nummern
   identifizieren.
2. Lagebild in der gewünschten Anwendung öffnen.
3. **BILD EINFRIEREN** drücken.
4. Auf der Plexiglasscheibe markieren.
5. Bei Bedarf **STANDBILD AKTUALISIEREN** verwenden; der optionale
   `EINGEFROREN`-Hinweis wird dabei nicht in das neue Standbild aufgenommen.
6. Mit **LIVE-BILD WIEDERHERSTELLEN** zum aktuellen Bild zurückkehren.

## Funktionen der 0.1.0-Vorabversion

- Auswahl und lokales Wiedererkennen eines angeschlossenen Monitors
- Monitoridentifikation auf allen Displays
- pixelgenaues Standbild ohne Taskleiste, Mauszeiger oder interaktive
  Bedienelemente; auf Wunsch erscheint nur der kleine Statushinweis
- Live/Freeze-Wechsel und Aktualisierung des tatsächlichen Hintergrundinhalts
- konfigurierbare und deaktivierbare globale Hotkeys mit `F9` und `F10` als Standard
- Unterstützung für unterschiedliche Auflösungen, Anordnungen und DPI-Skalierungen
- verständliche Fehlermeldungen und lokale, täglich getrennte Logs
- kompakte, ruhige und touchscreen-taugliche Oberfläche mit vollständig dunklen
  Auswahlfeldern und Dropdown-Popups
- System-Tray mit den wichtigsten Freeze-, Live- und Refresh-Aktionen
- optionaler Autostart und minimierter Start, standardmäßig deaktiviert
- lokaler PNG-Export nach `Bilder\LageFreeze` oder in einen gewählten Ordner
- Zeichenmodus in Original, leicht abgedunkelt oder stark abgedunkelt
- optionaler `EINGEFROREN`-Hinweis in einer der vier Bildschirmecken
- übersichtliche Einstellungen für Monitor, Hotkeys, Tray, Anzeige und Screenshots
- vollständig lokaler Betrieb ohne Cloud, Telemetrie oder Tracking

Der Stand und die noch offenen Freigabeschritte stehen in der
[Roadmap](docs/roadmap.md) und der [manuellen Testmatrix](docs/manual-testing.md).

## Screenshots

### Hauptansicht

> Platzhalter – `docs/images/main-window.png` wird mit dem ersten visuellen
> Release ergänzt.

### Freeze-Modus

> Platzhalter – `docs/images/freeze-mode.png` wird nach dem Hardwaretest ergänzt.

## Installation

1. `LageFreeze-Setup-x64.exe` aus dem neuesten Release herunterladen.
2. Installer starten und optional die Desktop-Verknüpfung auswählen.
3. LageFreeze über das Startmenü öffnen.
4. Den gewünschten Außenmonitor auswählen.

Der Installer installiert LageFreeze für den aktuellen Benutzer. Eine separate
.NET-Runtime ist nicht erforderlich. Updates können über eine neuere
Installer-Version eingespielt werden; lokale Einstellungen und Logs bleiben bei
der Deinstallation bewusst erhalten.

Die Vorabversion ist noch nicht mit einem vertrauenswürdigen Code-Signing-Zertifikat
signiert. Windows kann deshalb beim ersten Start einen SmartScreen-Hinweis
anzeigen. Downloads sollten ausschließlich vom offiziellen Release stammen und
bei Bedarf mit `SHA256SUMS.txt` geprüft werden.

Für die portable Variante das ZIP-Archiv in einen eigenen Ordner entpacken und
`LageFreeze.exe` starten.

## Verwendung

Das Hauptfenster bleibt auf dem Bedienmonitor. Der ausgewählte Außenmonitor zeigt
im Freeze-Modus das Standbild und, falls aktiviert, nur den kleinen
`EINGEFROREN`-Hinweis darüber. Der Status **LIVE** beziehungsweise
**EINGEFROREN** und die jeweils verfügbaren großen Aktionen sind im Hauptfenster
jederzeit sichtbar.

- **STANDBILD AKTUALISIEREN** blendet das gesamte Overlay einschließlich des
  `EINGEFROREN`-Hinweises kurz aus und nimmt den tatsächlichen Inhalt dahinter
  neu auf. Der Hinweis wird dadurch nicht dauerhaft in das Standbild eingebrannt.
- Der **Zeichenmodus** dunkelt nur die Anzeige ab, damit Markierungen auf einer
  Plexiglasscheibe besser sichtbar sein können.
- **STANDBILD SPEICHERN** exportiert das aktuell erfasste Bild lokal als PNG.
- Bei aktiviertem System-Tray sind Freeze, Refresh und Live auch über das
  Kontextmenü des Tray-Icons erreichbar.

Wird der aktive Zielmonitor getrennt, beendet LageFreeze den Freeze und fordert
zu einer neuen Auswahl auf. Vor einem Einsatz sollte dieses Verhalten mit der
tatsächlich verwendeten Hardware geprüft werden.

## Tastenkürzel

| Funktion | Standard |
| --- | --- |
| Freeze / Live umschalten | `F9` |
| Standbild aktualisieren | `F10` |

Kann ein Tastenkürzel nicht registriert werden, ist es wahrscheinlich bereits
von einer anderen Anwendung belegt. LageFreeze zeigt dann einen Hinweis und
bleibt über die Oberfläche bedienbar.

## Einstellungen

Unter **EINSTELLUNGEN** lassen sich Standardmonitor, Autostart, minimierter
Start, System-Tray, Tastenkürzel, Zeichenmodus und Screenshot-Ordner festlegen.
Alle Auswahlfelder verwenden auch im geöffneten Dropdown ein durchgehend dunkles
Design.

Der `EINGEFROREN`-Hinweis ist standardmäßig aktiviert und erscheint oben rechts
auf dem eingefrorenen Zielmonitor. Er kann vollständig ausgeschaltet oder oben
links, oben rechts, unten links beziehungsweise unten rechts positioniert
werden. Änderungen gelten nach **SPEICHERN** sofort auch für ein bereits
sichtbares Standbild und bleiben nach einem Neustart erhalten. Der Hinweis ist
nur eine Anzeigeebene: Er gehört weder zum gespeicherten PNG noch zum beim
Aktualisieren neu aufgenommenen Bild.

## Systemanforderungen

- Windows 10 oder Windows 11
- x64-Prozessor
- ein zusätzlicher Monitor für den vorgesehenen Anwendungsfall
- keine separate .NET-Installation bei Verwendung eines Release-Downloads

## Datenschutz und lokales Logging

Bildschirminhalte, Einstellungen und Logs bleiben auf dem Gerät. LageFreeze
benötigt für den Betrieb keine Internetverbindung und enthält keine Cloud-
Anbindung, Analytics, Telemetrie oder Benutzerverfolgung. Logs liegen unter
`%LOCALAPPDATA%\LageFreeze\Logs\` und können bei einer Supportanfrage vor dem
Teilen geprüft und geschwärzt werden.

## Entwicklung

Voraussetzungen sind Windows 10/11 und das [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/thisislennard/LageFreeze.git
cd LageFreeze
dotnet restore LageFreeze.sln
dotnet build LageFreeze.sln --configuration Release --no-restore
dotnet test LageFreeze.sln --configuration Release --no-build
dotnet run --project src/LageFreeze/LageFreeze.csproj
```

### Projektstruktur

```text
src/LageFreeze/          WPF-Anwendung, Modelle und Windows-Dienste
tests/LageFreeze.Tests/  fokussierte automatisierte Tests
installer/               Inno-Setup-Skript
docs/                    Anforderungen, Architektur und Prüfanleitungen
.github/                 CI, Releases und Beitragsvorlagen
```

Wichtige Dokumente:

- [Produktanforderungen](docs/product-requirements.md)
- [Technische Leitplanken](docs/architecture.md)
- [Manuelle Testmatrix](docs/manual-testing.md)
- [Release-Prozess](docs/releasing.md)
- [Empfohlene Repository-Einstellungen](docs/repository-settings.md)
- [Beitragen](CONTRIBUTING.md)
- [Sicherheitsmeldungen](SECURITY.md)

Builds und Tests laufen bei Pushes und Pull Requests gegen `main` automatisch.
Ein SemVer-Tag wie `v0.1.0` erzeugt die drei einheitlich benannten
Release-Artefakte. Details beschreibt der [Release-Prozess](docs/releasing.md).

## Releases

Versionen folgen [Semantic Versioning](https://semver.org/lang/de/) und werden
unter [GitHub Releases](https://github.com/thisislennard/LageFreeze/releases)
veröffentlicht. Das [Changelog](CHANGELOG.md) dokumentiert anwenderrelevante
Änderungen; ein Release wird erst nach der manuellen Hardwarematrix freigegeben.

## Haftungshinweis

LageFreeze ist ein Hilfsmittel zur Darstellung von Bildschirminhalten und kein
sicherheitskritisches Einsatzführungssystem. Anwender sind selbst dafür
verantwortlich, Aktualität und Richtigkeit der angezeigten Informationen
sicherzustellen.

## Lizenz

LageFreeze ist unter der [MIT-Lizenz](LICENSE) veröffentlicht.
Copyright © 2026 ThisisLennard.
