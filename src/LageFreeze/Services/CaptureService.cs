using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using LageFreeze.Models;

namespace LageFreeze.Services;

public interface ICaptureService
{
    BitmapSource CaptureMonitor(MonitorInfo monitor);

    BitmapSource CaptureBounds(ScreenBounds bounds);
}

/// <summary>
/// Captures physical desktop pixels through GDI BitBlt. The pointer is not part
/// of a BitBlt capture. An existing freeze overlay must be hidden by the caller
/// before refreshing, otherwise Windows will capture the overlay itself.
/// </summary>
public sealed class CaptureService : ICaptureService
{
    private readonly ILoggingService? _logger;

    public CaptureService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    public BitmapSource CaptureMonitor(MonitorInfo monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        return CaptureBounds(monitor.Bounds);
    }

    public BitmapSource CaptureBounds(ScreenBounds bounds)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Screen capture is supported only on Windows.");
        }

        if (bounds.IsEmpty)
        {
            throw new ArgumentException("Capture bounds must have a positive width and height.", nameof(bounds));
        }

        nint screenDeviceContext = nint.Zero;
        nint memoryDeviceContext = nint.Zero;
        nint bitmapHandle = nint.Zero;
        nint previousObject = nint.Zero;

        try
        {
            screenDeviceContext = NativeMethods.GetDC(nint.Zero);
            if (screenDeviceContext == nint.Zero)
            {
                ThrowLastWin32Error("The desktop device context could not be opened.");
            }

            memoryDeviceContext = NativeMethods.CreateCompatibleDC(screenDeviceContext);
            if (memoryDeviceContext == nint.Zero)
            {
                ThrowLastWin32Error("A compatible device context could not be created.");
            }

            bitmapHandle = NativeMethods.CreateCompatibleBitmap(
                screenDeviceContext,
                bounds.Width,
                bounds.Height);
            if (bitmapHandle == nint.Zero)
            {
                ThrowLastWin32Error("A capture bitmap could not be created.");
            }

            previousObject = NativeMethods.SelectObject(memoryDeviceContext, bitmapHandle);
            if (previousObject == nint.Zero || previousObject == new nint(-1))
            {
                ThrowLastWin32Error("The capture bitmap could not be selected.");
            }

            var operation = NativeMethods.SrcCopy | NativeMethods.CaptureBlt;
            if (!NativeMethods.BitBlt(
                    memoryDeviceContext,
                    0,
                    0,
                    bounds.Width,
                    bounds.Height,
                    screenDeviceContext,
                    bounds.Left,
                    bounds.Top,
                    operation))
            {
                ThrowLastWin32Error("The monitor pixels could not be copied.");
            }

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmapHandle,
                nint.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            _logger?.Debug($"Bildschirmbereich aufgenommen: {bounds}.");
            return source;
        }
        catch (Exception exception)
        {
            _logger?.Error($"Bildschirmaufnahme fehlgeschlagen: {bounds}.", exception);
            throw;
        }
        finally
        {
            if (previousObject != nint.Zero
                && previousObject != new nint(-1)
                && memoryDeviceContext != nint.Zero)
            {
                NativeMethods.SelectObject(memoryDeviceContext, previousObject);
            }

            if (bitmapHandle != nint.Zero)
            {
                NativeMethods.DeleteObject(bitmapHandle);
            }

            if (memoryDeviceContext != nint.Zero)
            {
                NativeMethods.DeleteDC(memoryDeviceContext);
            }

            if (screenDeviceContext != nint.Zero)
            {
                NativeMethods.ReleaseDC(nint.Zero, screenDeviceContext);
            }
        }
    }

    private static void ThrowLastWin32Error(string message)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), message);
    }
}
