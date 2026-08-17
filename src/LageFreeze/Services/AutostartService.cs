using Microsoft.Win32;

namespace LageFreeze.Services;

/// <summary>
/// Manages the current user's optional Windows startup entry. It never writes
/// machine-wide keys and does not enable itself unless explicitly requested.
/// </summary>
public sealed class AutostartService
{
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "LageFreeze";
    private readonly ILoggingService? _logger;

    public AutostartService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    public bool IsEnabled()
    {
        EnsureWindows();
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        return key?.GetValue(ValueName) is string command && !string.IsNullOrWhiteSpace(command);
    }

    public string? GetRegisteredCommand()
    {
        EnsureWindows();
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        return key?.GetValue(ValueName) as string;
    }

    public void SetEnabled(bool enabled, string? executablePath = null, bool startMinimized = false)
    {
        EnsureWindows();

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true)
                ?? throw new InvalidOperationException("The current-user startup key could not be opened.");

            if (!enabled)
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
                _logger?.Information("Autostart deaktiviert.");
                return;
            }

            var path = executablePath ?? Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            {
                throw new ArgumentException("A fully qualified executable path is required.", nameof(executablePath));
            }

            var command = QuoteCommandLineArgument(path);
            if (startMinimized)
            {
                command += " --minimized";
            }

            key.SetValue(ValueName, command, RegistryValueKind.String);
            _logger?.Information("Autostart aktiviert.");
        }
        catch (Exception exception)
        {
            _logger?.Error("Autostart-Einstellung konnte nicht geändert werden.", exception);
            throw;
        }
    }

    private static string QuoteCommandLineArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows autostart is supported only on Windows.");
        }
    }
}
