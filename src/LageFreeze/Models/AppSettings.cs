namespace LageFreeze.Models;

public sealed class AppSettings
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public MonitorSelection? SelectedMonitor { get; set; }

    public HotkeySetting ToggleFreezeHotkey { get; set; } = HotkeySetting.CreateDefaultToggle();

    public HotkeySetting RefreshHotkey { get; set; } = HotkeySetting.CreateDefaultRefresh();

    public bool UseSystemTray { get; set; } = true;

    public bool MinimizeToTray { get; set; } = true;

    public bool StartWithWindows { get; set; }

    public bool StartMinimized { get; set; }

    public DrawingMode DefaultDrawingMode { get; set; } = DrawingMode.Original;

    /// <summary>
    /// Optional user-selected PNG directory. A null or blank value resolves to
    /// the local Pictures/LageFreeze directory.
    /// </summary>
    public string? ScreenshotFolder { get; set; }

    public string ResolveScreenshotFolder()
    {
        return string.IsNullOrWhiteSpace(ScreenshotFolder)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "LageFreeze")
            : Environment.ExpandEnvironmentVariables(ScreenshotFolder.Trim());
    }

    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }
}

public enum DrawingMode
{
    Original,
    Dimmed,
    StronglyDimmed,
}

/// <summary>
/// Persisted hints used to restore a selected physical display. StableId is the
/// strongest hint; the remaining fields deliberately provide fallbacks for
/// driver updates and display-topology changes.
/// </summary>
public sealed class MonitorSelection
{
    public string? StableId { get; set; }

    public string? DeviceName { get; set; }

    public string? DisplayName { get; set; }

    public ScreenBounds LastKnownBounds { get; set; }

    public bool WasPrimary { get; set; }

    public static MonitorSelection FromMonitor(MonitorInfo monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        return new MonitorSelection
        {
            StableId = monitor.StableId,
            DeviceName = monitor.DeviceName,
            DisplayName = monitor.DisplayName,
            LastKnownBounds = monitor.Bounds,
            WasPrimary = monitor.IsPrimary,
        };
    }
}
