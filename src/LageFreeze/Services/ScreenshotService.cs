using System.Windows.Media.Imaging;
using LageFreeze.Models;

namespace LageFreeze.Services;

public interface IScreenshotService
{
    string DefaultDirectory { get; }

    string SavePng(
        BitmapSource image,
        MonitorInfo? monitor = null,
        string? destinationDirectory = null);

    Task<string> SavePngAsync(
        BitmapSource image,
        MonitorInfo? monitor = null,
        string? destinationDirectory = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Saves explicitly requested screenshots locally as PNG files.</summary>
public sealed class ScreenshotService : IScreenshotService
{
    private readonly ILoggingService? _logger;
    private readonly TimeProvider _timeProvider;

    public ScreenshotService(
        string? defaultDirectory = null,
        ILoggingService? logger = null,
        TimeProvider? timeProvider = null)
    {
        DefaultDirectory = defaultDirectory ?? GetDefaultDirectory();
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string DefaultDirectory { get; }

    public string SavePng(
        BitmapSource image,
        MonitorInfo? monitor = null,
        string? destinationDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        var localImage = GetFrozenImage(image);
        var directory = ResolveDirectory(destinationDirectory);
        Directory.CreateDirectory(directory);

        var fileName = BuildFileName(_timeProvider.GetLocalNow(), monitor?.DisplayNumber);
        var path = GetAvailablePath(directory, fileName);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(localImage));

            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 64 * 1024,
                       FileOptions.WriteThrough))
            {
                encoder.Save(stream);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path);
            _logger?.Information($"Screenshot lokal gespeichert: {path}");
            return path;
        }
        catch (Exception exception)
        {
            _logger?.Error("Screenshot konnte nicht gespeichert werden.", exception);
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

            throw;
        }
    }

    public Task<string> SavePngAsync(
        BitmapSource image,
        MonitorInfo? monitor = null,
        string? destinationDirectory = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        var frozenImage = GetFrozenImage(image);

        return Task.Run(
            () => SavePng(frozenImage, monitor, destinationDirectory),
            cancellationToken);
    }

    public static string GetDefaultDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "LageFreeze");
    }

    public static string BuildFileName(DateTimeOffset timestamp, int? displayNumber = null)
    {
        var displayPart = displayNumber is > 0 ? $"-Monitor-{displayNumber}" : string.Empty;
        return $"LageFreeze-{timestamp:yyyy-MM-dd-HH-mm-ss}{displayPart}.png";
    }

    private string ResolveDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return DefaultDirectory;
        }

        return Environment.ExpandEnvironmentVariables(directory.Trim());
    }

    private static BitmapSource GetFrozenImage(BitmapSource image)
    {
        if (image.IsFrozen)
        {
            return image;
        }

        var clone = image.CloneCurrentValue();
        if (!clone.CanFreeze)
        {
            throw new InvalidOperationException("The screenshot image cannot be used across threads.");
        }

        clone.Freeze();
        return clone;
    }

    private static string GetAvailablePath(string directory, string fileName)
    {
        var firstCandidate = Path.Combine(directory, fileName);
        if (!File.Exists(firstCandidate))
        {
            return firstCandidate;
        }

        var stem = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        for (var suffix = 2; suffix < int.MaxValue; suffix++)
        {
            var candidate = Path.Combine(directory, $"{stem}-{suffix}{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException("No available screenshot filename could be found.");
    }
}
