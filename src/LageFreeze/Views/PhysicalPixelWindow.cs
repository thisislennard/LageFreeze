using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace LageFreeze.Views;

/// <summary>
/// Base window for monitor overlays whose bounds are expressed in physical pixels.
/// WPF normally exposes device-independent units; SetWindowPos keeps placement exact
/// across mixed DPI values and negative virtual-desktop coordinates.
/// </summary>
public class PhysicalPixelWindow : Window
{
    private static readonly nint HwndTopmost = new(-1);

    private Int32Rect? _physicalBounds;

    /// <summary>
    /// Gets the most recently requested bounds in physical virtual-desktop pixels.
    /// </summary>
    public Int32Rect? PhysicalBounds => _physicalBounds;

    /// <summary>
    /// Gets whether the most recent native placement operation succeeded.
    /// </summary>
    public bool LastPlacementSucceeded { get; private set; }

    /// <summary>
    /// Stores and, when possible, immediately applies physical monitor bounds.
    /// </summary>
    protected void SetPhysicalBounds(Int32Rect bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bounds),
                "Die Monitorgrenzen müssen eine positive Breite und Höhe besitzen.");
        }

        _physicalBounds = bounds;
        WindowStartupLocation = WindowStartupLocation.Manual;
        ApplyPhysicalBounds();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyPhysicalBounds();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        ApplyPhysicalBounds();
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        Dispatcher.BeginInvoke(DispatcherPriority.Render, ApplyPhysicalBounds);
    }

    /// <summary>
    /// Reapplies the configured physical bounds, for example after a DPI change.
    /// </summary>
    public void ReapplyPhysicalBounds()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(ReapplyPhysicalBounds, DispatcherPriority.Send);
            return;
        }

        ApplyPhysicalBounds();
    }

    private void ApplyPhysicalBounds()
    {
        if (_physicalBounds is not { } bounds)
        {
            return;
        }

        nint handle = new WindowInteropHelper(this).Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        LastPlacementSucceeded = SetWindowPos(
            handle,
            HwndTopmost,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            SetWindowPositionFlags.NoActivate |
            SetWindowPositionFlags.NoOwnerZOrder |
            SetWindowPositionFlags.ShowWindow);

        if (!LastPlacementSucceeded)
        {
            Trace.TraceError(
                "LageFreeze could not place overlay at {0},{1} ({2}x{3}). Win32 error: {4}",
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                Marshal.GetLastWin32Error());
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        nint windowHandle,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        SetWindowPositionFlags flags);

    [Flags]
    private enum SetWindowPositionFlags : uint
    {
        NoActivate = 0x0010,
        NoOwnerZOrder = 0x0200,
        ShowWindow = 0x0040,
    }
}
