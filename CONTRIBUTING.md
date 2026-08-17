# Zu LageFreeze beitragen

Danke für dein Interesse an LageFreeze. Das Projekt soll klein, robust und auch
nach längerer Pause schnell verständlich bleiben.

## Vor einer Änderung

- Für Fehler und größere Funktionen zuerst ein passendes Issue anlegen.
- Den MVP-Umfang in [docs/roadmap.md](docs/roadmap.md) und die technischen
  Leitplanken in [AGENTS.md](AGENTS.md) beachten.
- Neue Abhängigkeiten nur vorschlagen, wenn ihr Nutzen den zusätzlichen
  Wartungsaufwand klar rechtfertigt.

## Lokaler Ablauf

1. Repository forken oder einen kurzlebigen Branch von `main` erstellen.
2. Änderung in kleinen, nachvollziehbaren Schritten umsetzen.
3. Build und automatisierte Tests ausführen:

   ```powershell
   dotnet restore LageFreeze.sln
   dotnet build LageFreeze.sln --configuration Release --no-restore
   dotnet test LageFreeze.sln --configuration Release --no-build
   ```

4. Hardwareabhängige Änderungen anhand von
   [docs/manual-testing.md](docs/manual-testing.md) prüfen.
5. Einen Pull Request mit Zweck, Testnachweisen und bei UI-Änderungen passenden
   Screenshots eröffnen.

## Konventionen

- Code, Bezeichner und Dateinamen sind Englisch; sichtbare Oberflächentexte sind
  Deutsch.
- Einfache, explizite Implementierungen haben Vorrang vor zusätzlichen
  Abstraktionsschichten.
- Keine Cloud-Dienste, Telemetrie, Analytics oder erforderlichen Netzwerkzugriffe
  hinzufügen.
- Änderungen an Verhalten oder Architektur zusammen mit den zugehörigen Tests
  und Dokumenten einreichen.
- Keine Logs oder Screenshots veröffentlichen, bevor mögliche vertrauliche
  Inhalte und lokale Pfade entfernt wurden.

Der Pull Request sollte den Build nicht beschädigen und alle für die Änderung
relevanten automatisierten und manuellen Prüfungen benennen. Maintainer können
um eine Aufteilung sehr großer Änderungen bitten.

Mit dem Einreichen eines Beitrags erklärst du dich damit einverstanden, dass
dieser Beitrag unter der [MIT-Lizenz](LICENSE) des Projekts veröffentlicht wird.
