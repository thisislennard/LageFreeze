using System.ComponentModel;

namespace LageFreeze.Services;

/// <summary>
/// Configures process DPI awareness. Call this before WPF creates any HWND.
/// The application manifest should also declare PerMonitorV2 as a fallback.
/// </summary>
public static class DpiAwarenessService
{
    public static bool IsPerMonitorV2
    {
        get
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 15063))
            {
                return false;
            }

            var current = NativeMethods.GetThreadDpiAwarenessContext();
            return NativeMethods.AreDpiAwarenessContextsEqual(
                current,
                NativeMethods.DpiAwarenessContextPerMonitorAwareV2);
        }
    }

    public static bool TryEnablePerMonitorV2(out string? error)
    {
        error = null;

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 15063))
        {
            error = "Per-Monitor-V2-DPI-Unterstützung ist auf dieser Windows-Version nicht verfügbar.";
            return false;
        }

        if (IsPerMonitorV2)
        {
            return true;
        }

        if (NativeMethods.SetProcessDpiAwarenessContext(
                NativeMethods.DpiAwarenessContextPerMonitorAwareV2))
        {
            return true;
        }

        var exception = new Win32Exception();
        error = $"Die DPI-Unterstützung konnte nicht aktiviert werden ({exception.NativeErrorCode}).";
        return false;
    }
}
