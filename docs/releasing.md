# Release-Prozess

Releases folgen Semantic Versioning und werden aus unveränderlichen Tags wie
`v1.0.0` erzeugt. Der GitHub-Workflow baut eine self-contained x64-Anwendung,
den Installer, das Portable-ZIP und die Prüfsummen reproduzierbar auf einem
Windows-Runner.

## Release vorbereiten

1. Gewünschte Version und Umfang festlegen.
2. `CHANGELOG.md`, README und bekannte Einschränkungen aktualisieren.
3. Build und automatisierte Tests auf `main` erfolgreich abschließen.
4. Repräsentative Fälle aus [manual-testing.md](manual-testing.md) mit dem
   Release-Kandidaten durchführen, Ergebnisse und noch nicht abgedeckte
   Hardwarekonfigurationen protokollieren.
5. Installer, Update über eine vorhandene Version und Deinstallation manuell
   prüfen.

Ein grüner CI-Build ersetzt keine Prüfung der konkreten Zielumgebung. Die
vollständigen Hardwaretests für Freeze, Refresh, DPI und Monitoränderungen sind
vor dem operativen Einsatz in jeder vorgesehenen Umgebung verpflichtend; noch
nicht dokumentierte Konfigurationen werden nicht als geprüft vorausgesetzt.

## Tag veröffentlichen

Vom geprüften Commit aus:

```powershell
git tag -a v1.0.0 -m "LageFreeze v1.0.0"
git push origin v1.0.0
```

Der Workflow akzeptiert stabile Tags und Vorabversionen wie `v1.1.0-beta.1`.
Vorabversionen werden automatisch als GitHub-Prerelease markiert. Bei Erfolg
entsteht ein Release mit:

- `LageFreeze-Setup-x64.exe`
- `LageFreeze-Portable-x64.zip`
- `SHA256SUMS.txt`

Die automatisch gegliederten Release Notes verwenden Pull-Request-Labels aus
`.github/release.yml`. Vor dem Tag sollten daher mindestens `feature` oder
`enhancement`, `bug` oder `fix`, beziehungsweise `documentation` gesetzt sein.

## Artefakte prüfen

1. Alle drei Dateien aus dem GitHub Release auf ein sauberes Windows-Testsystem
   laden.
2. Prüfsummen vergleichen, beispielsweise:

   ```powershell
   Get-FileHash .\LageFreeze-Setup-x64.exe -Algorithm SHA256
   Get-FileHash .\LageFreeze-Portable-x64.zip -Algorithm SHA256
   Get-Content .\SHA256SUMS.txt
   ```

3. Installer und portable Version jeweils starten und einen kurzen
   Freeze/Live/Refresh-Test durchführen.
4. Release Notes auf verständliche Sprache, Downloadhinweis und bekannte
   Einschränkungen prüfen.

Mangels eines bereitgestellten Zertifikats sind die Anwendung und der Installer
derzeit nicht digital signiert. Der dadurch mögliche SmartScreen-Hinweis wird
unabhängig vom Versionsstatus in README und Release Notes transparent genannt.
Code Signing wird erst ergänzt, wenn Zertifikat und sichere Secret-Verwaltung
festgelegt sind.

## Lokaler Installer-Build

Der CI-Workflow ist die maßgebliche Release-Umgebung und verwendet Inno Setup
6.7.1. Für einen lokalen Test werden das .NET 8 SDK und Inno Setup 6 benötigt:

```powershell
dotnet restore src/LageFreeze/LageFreeze.csproj --runtime win-x64
dotnet publish src/LageFreeze/LageFreeze.csproj --configuration Release --runtime win-x64 --self-contained true --output artifacts/publish/win-x64
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" "/DMyAppVersion=1.0.0" "/DMyAppNumericVersion=1.0.0.0" "/DPublishDir=$PWD\artifacts\publish\win-x64" "/DOutputDir=$PWD\artifacts" "installer\LageFreeze.iss"
```

Die Standardinstallation erfolgt ohne Administratorrechte unter
`%LOCALAPPDATA%\Programs\LageFreeze`. Einstellungen und Logs liegen getrennt
unter `%LOCALAPPDATA%\LageFreeze` und werden bei einem Update oder einer normalen
Deinstallation nicht gelöscht.
