using System.Text.Json;
using System.Text.Json.Serialization;
using LageFreeze.Models;

namespace LageFreeze.Services;

public interface ISettingsService
{
    string SettingsPath { get; }

    AppSettings Load();

    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    void Save(AppSettings settings);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}

/// <summary>Stores all user settings locally as human-readable JSON.</summary>
public sealed class SettingsService : ISettingsService, IDisposable
{
    private readonly SemaphoreSlim _accessLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILoggingService? _logger;
    private bool _disposed;

    public SettingsService(string? settingsPath = null, ILoggingService? logger = null)
    {
        SettingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LageFreeze",
            "settings.json");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public string SettingsPath { get; }

    public AppSettings Load()
    {
        return LoadAsync().GetAwaiter().GetResult();
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _accessLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!File.Exists(SettingsPath))
            {
                return AppSettings.CreateDefault();
            }

            await using var stream = new FileStream(
                SettingsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(
                    stream,
                    _jsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (settings is null)
            {
                _logger?.Warning("Die Einstellungsdatei war leer; Standardwerte werden verwendet.");
                return AppSettings.CreateDefault();
            }

            Normalize(settings);
            return settings;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException
                                          or IOException
                                          or UnauthorizedAccessException
                                          or NotSupportedException)
        {
            _logger?.Error("Einstellungen konnten nicht gelesen werden; Standardwerte werden verwendet.", exception);
            return AppSettings.CreateDefault();
        }
        finally
        {
            _accessLock.Release();
        }
    }

    public void Save(AppSettings settings)
    {
        SaveAsync(settings).GetAwaiter().GetResult();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(settings);
        Normalize(settings);

        await _accessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporaryPath = null;

        try
        {
            var directory = Path.GetDirectoryName(SettingsPath)
                ?? throw new InvalidOperationException("SettingsPath must contain a directory.");
            Directory.CreateDirectory(directory);

            temporaryPath = Path.Combine(
                directory,
                $".{Path.GetFileName(SettingsPath)}.{Guid.NewGuid():N}.tmp");
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, SettingsPath, overwrite: true);
            temporaryPath = null;
            _logger?.Debug("Einstellungen lokal gespeichert.");
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            _accessLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _accessLock.Dispose();
        _disposed = true;
    }

    private static void Normalize(AppSettings settings)
    {
        settings.SchemaVersion = AppSettings.CurrentSchemaVersion;
        settings.ToggleFreezeHotkey ??= HotkeySetting.CreateDefaultToggle();
        settings.RefreshHotkey ??= HotkeySetting.CreateDefaultRefresh();

        NormalizeHotkey(settings.ToggleFreezeHotkey, HotkeyKey.F9);
        NormalizeHotkey(settings.RefreshHotkey, HotkeyKey.F10);

        if (!Enum.IsDefined(settings.DefaultDrawingMode))
        {
            settings.DefaultDrawingMode = DrawingMode.Original;
        }

        if (!Enum.IsDefined(settings.FrozenIndicatorPosition))
        {
            settings.FrozenIndicatorPosition = FrozenIndicatorPosition.TopRight;
        }

        if (string.IsNullOrWhiteSpace(settings.ScreenshotFolder))
        {
            settings.ScreenshotFolder = null;
        }
    }

    private static void NormalizeHotkey(HotkeySetting hotkey, HotkeyKey fallbackKey)
    {
        if (!Enum.IsDefined(hotkey.Key))
        {
            hotkey.Key = fallbackKey;
        }

        const GlobalHotkeyModifiers supportedModifiers =
            GlobalHotkeyModifiers.Alt
            | GlobalHotkeyModifiers.Control
            | GlobalHotkeyModifiers.Shift
            | GlobalHotkeyModifiers.Windows;
        hotkey.Modifiers &= supportedModifiers;
    }
}
