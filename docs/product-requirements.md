# Produktanforderungen

## Zweck

Ein ausgewählter Monitor zeigt beispielsweise eine Karte, ein Lagebild, einen
Browser oder einen Videostream. LageFreeze nimmt dessen aktuellen Inhalt auf und
legt das Bild als randloses, pixelgenaues Vollbild auf genau diesen Monitor. Der
Desktop und alle Anwendungen darunter laufen weiter. Durch Schließen des
Vollbildfensters ist sofort wieder das Live-Bild sichtbar.

Der typische Anwendungsfall ist ein Außenmonitor mit transparenter
Plexiglasscheibe, auf der taktische Markierungen mit Whiteboard-Markern angebracht
werden. Das Produkt bleibt unabhängig von Fireboard oder jeder anderen
Quellanwendung.

## MVP-Funktionen

1. Alle angeschlossenen Monitore erkennen und verständlich mit Nummer,
   Bezeichnung und Auflösung auflisten.
2. Einen Zielmonitor wählen, dauerhaft speichern und beim nächsten Start robust
   wiedererkennen.
3. Zur Identifikation einige Sekunden lang eine große Nummer auf jedem Monitor
   anzeigen.
4. Nur den gewählten Monitor aufnehmen und ohne Rand, Taskleiste, Mauszeiger oder
   Bedienelemente pixelgenau über ihm anzeigen.
5. Freeze beenden und das unveränderte Live-Bild unmittelbar sichtbar machen.
6. Ein aktives Standbild aus dem tatsächlichen Inhalt hinter dem Overlay erneuern,
   möglichst ohne sichtbares Flackern.
7. Globale Standard-Hotkeys: `F9` für Freeze/Live und `F10` für Aktualisieren.
8. Auswahl und Hotkeys lokal speichern.
9. Benutzerfreundliche Fehlerbehandlung und lokales Logging.

## Bedienoberfläche

Die dunkle, ruhige und touchfreundliche Hauptansicht zeigt jederzeit:

- ausgewählten Monitor und Auflösung,
- eindeutig `LIVE` oder `EINGEFROREN`,
- im Live-Modus die Hauptaktion `BILD EINFRIEREN`,
- im Freeze-Modus `STANDBILD AKTUALISIEREN` und
  `LIVE-BILD WIEDERHERSTELLEN`,
- Aktionen zum Monitorwechsel und Identifizieren.

Große Schaltflächen, klare Abstände und wenig Text haben Vorrang vor Animationen
oder komplexer Navigation.

## Display-Anforderungen

Unterstützt werden Windows 10/11 x64, Full HD bis 4K, Hochformat, Monitore links,
rechts, oberhalb oder unterhalb des Hauptmonitors, negative Koordinaten und
gemischte Skalierungen von mindestens 100, 125 und 150 Prozent. Fensterposition
und Aufnahme müssen physische Pixel respektieren; die Anwendung ist per-monitor
DPI-aware.

Änderungen der Display-Konfiguration dürfen keinen Absturz verursachen. Wird der
aktive Zielmonitor entfernt, beendet die Anwendung den Freeze, aktualisiert die
Liste und informiert verständlich. Ein Anwendungsende oder Fehler darf niemals
ein dauerhaft eingefrorenes oder schwarzes Bild hinterlassen.

## Lokale Daten und Datenschutz

- Keine Cloud, Telemetrie, Analytics oder Benutzerverfolgung.
- Kein Internet für den Betrieb erforderlich.
- Einstellungen unter einem geeigneten lokalen Benutzerpfad.
- Logs unter `%LOCALAPPDATA%\LageFreeze\Logs\`, nach Tagen getrennt und
  optional nach 30 Tagen bereinigt.
- Logs enthalten Lebenszyklus, Monitorerkennung/-änderungen, Auswahl,
  Freeze/Live/Aktualisieren sowie Fehler; technische Details gehören ins Log,
  nicht in Benutzerdialoge.
- Optional gespeicherte Screenshots bleiben lokal.

## Spätere Funktionen

- System-Tray und konfigurierbares Verhalten beim Schließen
- Autostart, standardmäßig deaktiviert
- konfigurierbare/deaktivierbare Hotkeys
- PNG-Export nach `Bilder\LageFreeze`
- Zeichenmodus: Original, leicht oder stark abgedunkelt
- weitere Helligkeits-, Kontrast- und Sättigungsoptionen
- optionale Update-Prüfung, aber kein automatisches Update im MVP

## Distribution

Langfristiges Ziel ist `LageFreeze-Setup-x64.exe`, optional ergänzt durch
`LageFreeze-Portable-x64.zip` und `SHA256SUMS.txt`. Installation, Startmenü,
optionale Desktop-Verknüpfung, Update und Deinstallation sollen Windows-typisch
funktionieren. Für Endanwender wird eine self-contained x64-Veröffentlichung
bevorzugt.

## Qualität und Abnahme

Automatisierte Tests konzentrieren sich auf Einstellungen, Monitor-Matching,
Hotkey-Konfiguration, Dateinamen und Speicherpfade. Manuell zu prüfen sind echte
Multi-Monitor-Konfigurationen, gemischte DPI-Werte, Fullscreen, Freeze/Refresh,
Monitorentfernung, 4K und Hochformat.

Ein Release setzt einen erfolgreichen Build, App-Start, Monitorerkennung,
Freeze/Live/Refresh, sauberes Beenden, funktionierende Installation und
Deinstallation, aktuelle Dokumentation und ein reproduzierbares GitHub Release
voraus.
