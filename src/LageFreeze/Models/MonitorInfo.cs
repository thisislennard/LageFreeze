namespace LageFreeze.Models;

/// <summary>
/// A snapshot of a connected display. Bounds and work area are always expressed
/// in physical pixels and may use negative desktop coordinates.
/// </summary>
public sealed record MonitorInfo
{
    public required int DisplayNumber { get; init; }

    public required string DisplayName { get; init; }

    /// <summary>The GDI display name, for example <c>\\.\DISPLAY1</c>.</summary>
    public required string DeviceName { get; init; }

    /// <summary>
    /// Best available Windows display-interface identity. It is preferred over
    /// DeviceName when restoring a selection because DISPLAY numbers can change.
    /// </summary>
    public required string StableId { get; init; }

    public required ScreenBounds Bounds { get; init; }

    public required ScreenBounds WorkArea { get; init; }

    public required bool IsPrimary { get; init; }

    public required uint DpiX { get; init; }

    public required uint DpiY { get; init; }

    public string ResolutionText => $"{Bounds.Width} × {Bounds.Height}";

    public string DisplayText => $"{DisplayNumber} – {DisplayName} ({ResolutionText})";
}
