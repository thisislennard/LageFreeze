using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;
using LageFreeze.Models;

namespace LageFreeze.Services;

public sealed class MonitorsChangedEventArgs : EventArgs
{
    public MonitorsChangedEventArgs(
        IReadOnlyList<MonitorInfo> previous,
        IReadOnlyList<MonitorInfo> current)
    {
        Previous = previous;
        Current = current;
    }

    public IReadOnlyList<MonitorInfo> Previous { get; }

    public IReadOnlyList<MonitorInfo> Current { get; }
}

/// <summary>
/// Enumerates Windows displays through Win32 so all geometry remains in physical
/// desktop pixels. The process must be made Per-Monitor-V2 aware before use.
/// </summary>
public sealed class MonitorService : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly ILoggingService? _logger;
    private IReadOnlyList<MonitorInfo> _monitors = Array.Empty<MonitorInfo>();
    private HwndSource? _messageSource;
    private bool _refreshScheduled;
    private bool _disposed;

    public MonitorService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

    public IReadOnlyList<MonitorInfo> Monitors
    {
        get
        {
            lock (_syncRoot)
            {
                return _monitors;
            }
        }
    }

    /// <summary>Enumerates and caches the currently connected desktop monitors.</summary>
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Refresh();
    }

    public IReadOnlyList<MonitorInfo> Refresh()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var current = EnumerateMonitors();
        IReadOnlyList<MonitorInfo> previous;
        var changed = false;

        lock (_syncRoot)
        {
            previous = _monitors;
            changed = !AreEquivalent(previous, current);
            _monitors = current;
        }

        if (changed)
        {
            _logger?.Information($"Monitorkonfiguration aktualisiert: {current.Count} Monitor(e).");
            MonitorsChanged?.Invoke(this, new MonitorsChangedEventArgs(previous, current));
        }

        return current;
    }

    public MonitorInfo? FindBestMatch(MonitorSelection? selection)
    {
        return MonitorMatcher.FindBestMatch(selection, Monitors);
    }

    /// <summary>
    /// Hooks WM_DISPLAYCHANGE on an existing WPF window. Call after the HWND was
    /// created; call DetachWindow before destroying that HWND.
    /// </summary>
    public void AttachWindow(nint windowHandle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));
        }

        DetachWindow();
        _messageSource = HwndSource.FromHwnd(windowHandle)
            ?? throw new InvalidOperationException("The window handle does not belong to a WPF HwndSource.");
        _messageSource.AddHook(WindowProcedure);
    }

    public void DetachWindow()
    {
        if (_messageSource is null)
        {
            return;
        }

        _messageSource.RemoveHook(WindowProcedure);
        _messageSource = null;
        _refreshScheduled = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DetachWindow();
        _disposed = true;
    }

    private static IReadOnlyList<MonitorInfo> EnumerateMonitors()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Monitor enumeration is supported only on Windows.");
        }

        var monitors = new List<MonitorInfo>();
        var fallbackNumber = 0;

        bool Callback(
            nint monitorHandle,
            nint monitorDeviceContext,
            ref NativeMethods.Rect monitorRectangle,
            nint userData)
        {
            fallbackNumber++;
            monitors.Add(CreateMonitorInfo(monitorHandle, fallbackNumber));
            return true;
        }

        NativeMethods.MonitorEnumProc callback = Callback;
        if (!NativeMethods.EnumDisplayMonitors(nint.Zero, nint.Zero, callback, nint.Zero))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Connected monitors could not be enumerated.");
        }

        return monitors
            .OrderBy(monitor => monitor.DisplayNumber)
            .ThenBy(monitor => monitor.Bounds.Left)
            .ThenBy(monitor => monitor.Bounds.Top)
            .ToArray();
    }

    private static MonitorInfo CreateMonitorInfo(nint monitorHandle, int fallbackNumber)
    {
        var nativeInfo = new NativeMethods.MonitorInfoEx
        {
            Size = Marshal.SizeOf<NativeMethods.MonitorInfoEx>(),
        };

        if (!NativeMethods.GetMonitorInfo(monitorHandle, ref nativeInfo))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Monitor details could not be read.");
        }

        var displayDevice = new NativeMethods.DisplayDevice
        {
            Size = Marshal.SizeOf<NativeMethods.DisplayDevice>(),
        };
        var hasDisplayDevice = NativeMethods.EnumDisplayDevices(
            nativeInfo.DeviceName,
            0,
            ref displayDevice,
            NativeMethods.DisplayDeviceGetDeviceInterfaceName);

        var displayNumber = ParseDisplayNumber(nativeInfo.DeviceName, fallbackNumber);
        var displayName = hasDisplayDevice && !string.IsNullOrWhiteSpace(displayDevice.DeviceString)
            ? displayDevice.DeviceString.Trim()
            : $"Monitor {displayNumber}";
        var stableId = hasDisplayDevice
            ? FirstNonEmpty(displayDevice.DeviceId, displayDevice.DeviceKey)
            : string.Empty;

        GetEffectiveDpi(monitorHandle, out var dpiX, out var dpiY);

        return new MonitorInfo
        {
            DisplayNumber = displayNumber,
            DisplayName = displayName,
            DeviceName = nativeInfo.DeviceName,
            StableId = stableId,
            Bounds = ToBounds(nativeInfo.Monitor),
            WorkArea = ToBounds(nativeInfo.WorkArea),
            IsPrimary = (nativeInfo.Flags & NativeMethods.MonitorInfoPrimary) != 0,
            DpiX = dpiX,
            DpiY = dpiY,
        };
    }

    private static void GetEffectiveDpi(nint monitorHandle, out uint dpiX, out uint dpiY)
    {
        const uint DefaultDpi = 96;

        try
        {
            if (NativeMethods.GetDpiForMonitor(
                    monitorHandle,
                    NativeMethods.MonitorDpiType.Effective,
                    out dpiX,
                    out dpiY) == 0)
            {
                return;
            }
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }

        dpiX = DefaultDpi;
        dpiY = DefaultDpi;
    }

    private static ScreenBounds ToBounds(NativeMethods.Rect rectangle)
    {
        return ScreenBounds.FromEdges(
            rectangle.Left,
            rectangle.Top,
            rectangle.Right,
            rectangle.Bottom);
    }

    private static int ParseDisplayNumber(string deviceName, int fallback)
    {
        var index = deviceName.Length - 1;
        while (index >= 0 && char.IsDigit(deviceName[index]))
        {
            index--;
        }

        var digits = deviceName[(index + 1)..];
        return int.TryParse(digits, out var number) && number > 0 ? number : fallback;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
               ?? string.Empty;
    }

    private static bool AreEquivalent(
        IReadOnlyList<MonitorInfo> first,
        IReadOnlyList<MonitorInfo> second)
    {
        return first.Count == second.Count && first.SequenceEqual(second);
    }

    private nint WindowProcedure(
        nint windowHandle,
        int message,
        nint wordParameter,
        nint longParameter,
        ref bool handled)
    {
        if (message != NativeMethods.WmDisplayChange || _refreshScheduled || _messageSource is null)
        {
            return nint.Zero;
        }

        _refreshScheduled = true;
        _messageSource.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                try
                {
                    Refresh();
                }
                catch (Exception exception)
                {
                    _logger?.Error("Monitorkonfiguration konnte nach einer Anzeigeänderung nicht aktualisiert werden.", exception);
                }
                finally
                {
                    _refreshScheduled = false;
                }
            }));

        return nint.Zero;
    }
}
