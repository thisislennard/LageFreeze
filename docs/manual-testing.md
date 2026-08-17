# Manuelle Testmatrix

Automatisierte Tests decken reine Programmlogik, Einstellungspersistenz und die
strukturelle Initialisierung zentraler WPF-Fenster ab. Bildschirmaufnahme,
visuelle Darstellung, Vollbildfenster, DPI, Hotkeys und Änderungen der
Monitorhardware müssen vor dem operativen Einsatz zusätzlich auf der konkreten
Windows-Zielumgebung geprüft werden.

Ein nicht ausgeführter Hardwaretest gilt nicht als bestanden. Ergebnisse werden
für jeden geprüften Release-Kandidaten beziehungsweise jede Zielumgebung in
einer Kopie dieser Matrix oder im zugehörigen Release-Issue dokumentiert.

## Testprotokoll

| Angabe | Wert |
| --- | --- |
| Version / Commit | |
| Datum und Tester | |
| Windows-Version und Build | |
| Grafikadapter und Treiberversion | |
| Monitoranschlüsse | |
| Ergebnis | ☐ bestanden ☐ fehlgeschlagen |

## Erforderliche Konfigurationen

Die Gesamtheit der Testläufe muss mindestens folgende Varianten abdecken:

- Windows 10 x64 und Windows 11 x64
- zwei oder mehr Monitore
- 1920 × 1080 und 3840 × 2160
- Quer- und Hochformat
- Zielmonitor links, rechts, oberhalb und unterhalb des Hauptmonitors
- negative Desktop-Koordinaten
- gleiche und gemischte DPI-Skalierungen mit 100 %, 125 % und 150 %

Nicht alle Kombinationen müssen auf demselben Gerät vorhanden sein. Im
Testprotokoll muss nachvollziehbar bleiben, welche Konfiguration welchen Fall
abgedeckt hat.

## Funktionale Prüfung

| ID | Prüfung | Erwartetes Ergebnis | Ergebnis / Notiz |
| --- | --- | --- | --- |
| START-01 | Anwendung erstmals starten | Hauptfenster öffnet ohne technischen Fehlerdialog; Monitore werden gelistet. | |
| UI-01 | Hauptansicht, Monitorauswahl und Einstellungen bei 100 %, 125 % und 150 % Skalierung öffnen | Inhalte wirken kompakt und ruhig, bleiben vollständig lesbar und bedienbar und werden weder abgeschnitten noch überlagert. | |
| DROP-01 | Jede ComboBox in Hauptansicht, Monitorauswahl und Einstellungen öffnen und Einträge mit Maus sowie Tastatur wechseln | Geschlossenes Feld, Popup, Einträge, Auswahl-, Hover- und Fokuszustände sowie eine gegebenenfalls sichtbare Scrollbar bleiben vollständig dunkel und gut lesbar; es erscheint keine helle Systemfläche. | |
| MON-01 | Jeden Monitor anhand Nummer, Name und Auflösung mit Windows vergleichen | Liste ist vollständig und eindeutig genug für die Auswahl. | |
| MON-02 | Zielmonitor wählen, Anwendung schließen und erneut starten | Derselbe physische Monitor wird robust wiedererkannt. | |
| MON-03 | Gespeicherten Zielmonitor vor dem Start entfernen | Anwendung bleibt bedienbar und fordert verständlich zu einer neuen Auswahl auf. | |
| ID-01 | **Monitore identifizieren** auslösen | Auf jedem Monitor erscheint kurz die korrekte, große Nummer; alle Fenster verschwinden anschließend. | |
| FRZ-01 | Bewegten Inhalt auf dem Zielmonitor einfrieren | Das Standbild liegt exakt auf dem Zielmonitor; kein Rand, keine Taskleiste, kein Cursor und außer dem konfigurierten Statushinweis keine Bedienelemente sind sichtbar. | |
| FRZ-02 | Während Freeze auf **LIVE-BILD WIEDERHERSTELLEN** drücken | Overlay schließt sofort und der unveränderte Live-Inhalt ist sichtbar. | |
| IND-01 | Mit Standardeinstellungen einfrieren | Genau ein gut lesbarer, nicht interaktiver `EINGEFROREN`-Hinweis erscheint oben rechts und verdeckt nur seinen kleinen Randbereich. | |
| IND-02 | Den Hinweis nacheinander oben links, oben rechts, unten links und unten rechts positionieren | Der Hinweis sitzt nach dem Speichern sofort in der gewählten Ecke, bleibt vollständig auf dem Zielmonitor und die Position bleibt nach einem Neustart erhalten. | |
| IND-03 | Den `EINGEFROREN`-Hinweis deaktivieren und erneut einfrieren | Auf dem Zielmonitor erscheint kein Hinweis; die deaktivierte Einstellung bleibt nach einem Neustart erhalten. | |
| REF-01 | Inhalt hinter dem Overlay verändern und **STANDBILD AKTUALISIEREN** drücken | Das neue Hintergrundbild wird gezeigt, nicht das vorherige Standbild; Flackern bleibt minimal. | |
| REF-02 | Bei sichtbarem `EINGEFROREN`-Hinweis aktualisieren und den Hinweis danach in den Einstellungen ausblenden | Der Hintergrund wird neu aufgenommen, es erscheint nie ein doppelter Hinweis und nach dem Ausblenden bleibt kein in das Standbild eingebrannter Hinweis zurück. | |
| HOT-01 | `F9` bei inaktivem Hauptfenster verwenden | Freeze und Live wechseln zuverlässig. | |
| HOT-02 | `F10` bei aktivem Freeze und inaktivem Hauptfenster verwenden | Standbild wird aktualisiert. | |
| HOT-03 | Hotkey durch eine andere Anwendung belegen und LageFreeze starten | Konflikt wird verständlich gemeldet; Bedienung über das Hauptfenster bleibt möglich. | |
| DRAW-01 | Während Freeze zwischen Original, leicht und stark abgedunkelt wechseln | Nur die Darstellung des Standbilds wird in klar unterscheidbaren Stufen abgedunkelt; Position und Schärfe bleiben unverändert. | |
| PNG-01 | Aktives Standbild im Standardordner speichern | Gültige PNG-Datei entsteht unter `Bilder\LageFreeze`; Dateiname enthält Datum und Uhrzeit und überschreibt keine vorhandene Datei. | |
| PNG-02 | Benutzerdefinierten Screenshot-Ordner wählen und erneut speichern | PNG wird ausschließlich im gewählten lokalen Ordner gespeichert. | |
| PNG-03 | Bei sichtbarem `EINGEFROREN`-Hinweis ein Standbild speichern | Die PNG-Datei enthält nur den aufgenommenen Monitorinhalt, nicht den Hinweis. | |
| SET-01 | Hotkeys ändern, deaktivieren und auf Standard zurücksetzen | Änderungen werden nach Speichern aktiv, bleiben nach Neustart erhalten und lassen sich auf `F9`/`F10` zurücksetzen. | |
| TRAY-01 | Hauptfenster bei aktiviertem Tray schließen oder minimieren | Hauptfenster verschwindet entsprechend der Einstellung; Tray-Menü bleibt erreichbar und zeigt nur zum Zustand passende Aktionen aktiv. | |
| TRAY-02 | Freeze, Refresh, Live, Einstellungen und Beenden über das Tray auslösen | Jede Aktion entspricht der Hauptansicht; **Beenden** entfernt Icon und alle Overlays. | |
| AUTO-01 | Autostart aktivieren, optional minimierten Start wählen und Windows-Anmeldung simulieren | Eintrag gilt nur für den aktuellen Benutzer; LageFreeze startet wie gewählt und benötigt keine erhöhten Rechte. | |
| AUTO-02 | Autostart wieder deaktivieren | LageFreeze startet bei der nächsten Anmeldung nicht automatisch. | |
| CLOSE-01 | Anwendung während eines Freeze normal schließen | Alle LageFreeze-Fenster schließen und der Live-Desktop bleibt sichtbar. | |

## DPI-, Layout- und Hardwareprüfung

| ID | Prüfung | Erwartetes Ergebnis | Ergebnis / Notiz |
| --- | --- | --- | --- |
| DPI-01 | Zielmonitor nacheinander mit 100 %, 125 % und 150 % testen | Aufnahme und Overlay bleiben pixelgenau ohne Rand oder Versatz. | |
| DPI-02 | Monitore mit unterschiedlichen Skalierungen verwenden | Hauptfenster und Overlay wechseln ohne Größen- oder Positionsfehler zwischen den Monitoren. | |
| LAYOUT-01 | Zielmonitor links bzw. oberhalb des Hauptmonitors anordnen | Negative Koordinaten werden korrekt erfasst und positioniert. | |
| LAYOUT-02 | 4K- und Hochformatmonitor einfrieren und aktualisieren | Das vollständige Bild ist scharf, korrekt ausgerichtet und nicht beschnitten. | |
| CHANGE-01 | Auflösung, Skalierung oder Monitoranordnung im Live-Modus ändern | Liste und gespeicherte Auswahl werden ohne Absturz aktualisiert. | |
| CHANGE-02 | Aktiven Freeze-Monitor physisch trennen | Freeze wird beendet, verbleibende Overlays schließen und ein verständlicher Hinweis erscheint. | |
| CHANGE-03 | Monitor wieder verbinden | Monitorliste wird aktualisiert und eine erneute Auswahl ist möglich. | |

## Installation, Daten und Betrieb

| ID | Prüfung | Erwartetes Ergebnis | Ergebnis / Notiz |
| --- | --- | --- | --- |
| INST-01 | Installer ohne Administratorrechte ausführen | Installation für den aktuellen Benutzer funktioniert; Startmenüeintrag und optionaler Desktop-Link sind korrekt. | |
| INST-02 | Neuere Version über eine vorhandene installieren | Anwendung wird aktualisiert; Einstellungen bleiben erhalten. | |
| INST-03 | Über **Installierte Apps** deinstallieren | Programmdateien und Verknüpfungen werden entfernt; lokale Einstellungen und Logs bleiben bewusst erhalten. | |
| PORT-01 | Portable-ZIP in einen leeren Ordner entpacken und starten | Anwendung startet ohne separat installierte .NET-Runtime. | |
| LOG-01 | Start, Auswahl, Freeze, Refresh, Live und Ende ausführen | Tageslog unter `%LOCALAPPDATA%\LageFreeze\Logs\` enthält die Ereignisse, aber keine unnötigen Bilddaten. | |
| ERROR-01 | Nicht verfügbaren Monitor oder provozierten Aufnahmefehler behandeln | Meldung ist benutzerverständlich; technische Details stehen im Log; kein Overlay bleibt zurück. | |
| PRIV-01 | Betrieb im Ressourcenmonitor bzw. mit geeigneter Netzwerküberwachung beobachten | LageFreeze baut keine Netzwerkverbindung auf. | |

## Abschluss

Vor dem operativen Einsatz müssen alle für die konkrete Zielumgebung relevanten
Zeilen ein Ergebnis besitzen. Abweichungen werden als Issue dokumentiert und vor
der Inbetriebnahme behoben oder organisatorisch als bekannte Einschränkung
bewertet. Freeze, Live, Refresh, sauberes Beenden, Installation und
Deinstallation dürfen für die operative Freigabe nicht offen oder fehlgeschlagen
sein.
