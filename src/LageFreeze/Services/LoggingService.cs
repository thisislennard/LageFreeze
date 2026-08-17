using System.Globalization;
using System.Text;

namespace LageFreeze.Services;

public enum AppLogLevel
{
    Debug,
    Information,
    Warning,
    Error,
}

public interface ILoggingService
{
    string LogDirectory { get; }

    void Log(AppLogLevel level, string message, Exception? exception = null);

    void Debug(string message);

    void Information(string message);

    void Warning(string message);

    void Error(string message, Exception? exception = null);
}

/// <summary>
/// Small, dependency-free, local file logger. Logging failures are deliberately
/// swallowed so that a locked or unavailable log folder cannot break freezing.
/// </summary>
public sealed class LoggingService : ILoggingService
{
    private const string LogFilePrefix = "LageFreeze-";
    private readonly object _syncRoot = new();
    private readonly TimeProvider _timeProvider;
    private DateOnly? _lastCleanupDate;

    public LoggingService(
        string? logDirectory = null,
        int retentionDays = 30,
        TimeProvider? timeProvider = null)
    {
        if (retentionDays < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionDays));
        }

        LogDirectory = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LageFreeze",
            "Logs");
        RetentionDays = retentionDays;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string LogDirectory { get; }

    public int RetentionDays { get; }

    public void Debug(string message) => Log(AppLogLevel.Debug, message);

    public void Information(string message) => Log(AppLogLevel.Information, message);

    public void Warning(string message) => Log(AppLogLevel.Warning, message);

    public void Error(string message, Exception? exception = null)
        => Log(AppLogLevel.Error, message, exception);

    public void Log(AppLogLevel level, string message, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        try
        {
            lock (_syncRoot)
            {
                var now = _timeProvider.GetLocalNow();
                Directory.CreateDirectory(LogDirectory);
                CleanupIfNeeded(now);

                var fileName = $"{LogFilePrefix}{now:yyyy-MM-dd}.log";
                var path = Path.Combine(LogDirectory, fileName);
                var entry = FormatEntry(now, level, message, exception);
                File.AppendAllText(path, entry, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or NotSupportedException
                                   or System.Security.SecurityException)
        {
            // Logging is best effort and must never become an application failure.
        }
    }

    public void CleanupOldLogs()
    {
        try
        {
            lock (_syncRoot)
            {
                Directory.CreateDirectory(LogDirectory);
                CleanupCore(_timeProvider.GetLocalNow());
            }
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or NotSupportedException
                                   or System.Security.SecurityException)
        {
            // See Log: retention is useful, but never critical to core behavior.
        }
    }

    private static string FormatEntry(
        DateTimeOffset timestamp,
        AppLogLevel level,
        string message,
        Exception? exception)
    {
        var builder = new StringBuilder();
        builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
        builder.Append(" [");
        builder.Append(level.ToString().ToUpperInvariant());
        builder.Append("] ");
        builder.AppendLine(message.Replace("\0", string.Empty, StringComparison.Ordinal));

        if (exception is not null)
        {
            builder.AppendLine(exception.ToString());
        }

        return builder.ToString();
    }

    private void CleanupIfNeeded(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.Date);
        if (_lastCleanupDate == today)
        {
            return;
        }

        CleanupCore(now);
        _lastCleanupDate = today;
    }

    private void CleanupCore(DateTimeOffset now)
    {
        var cutoff = now.LocalDateTime.Date.AddDays(-RetentionDays);

        foreach (var path in Directory.EnumerateFiles(LogDirectory, $"{LogFilePrefix}*.log"))
        {
            if (File.GetLastWriteTime(path) < cutoff)
            {
                File.Delete(path);
            }
        }
    }
}
