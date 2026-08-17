using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using LageFreeze.Models;

namespace LageFreeze.Services;

public sealed class GlobalHotkeyPressedEventArgs : EventArgs
{
    public GlobalHotkeyPressedEventArgs(HotkeyAction action)
    {
        Action = action;
    }

    public HotkeyAction Action { get; }
}

/// <summary>Owns application-wide Win32 hotkey registrations for one WPF HWND.</summary>
public sealed class HotkeyService : IDisposable
{
    private const int RegistrationIdBase = 0x4C00;
    private readonly HashSet<HotkeyAction> _registeredActions = new();
    private readonly ILoggingService? _logger;
    private HwndSource? _messageSource;
    private nint _windowHandle;
    private bool _disposed;

    public HotkeyService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    public event EventHandler<GlobalHotkeyPressedEventArgs>? HotkeyPressed;

    public bool IsAttached => _windowHandle != nint.Zero;

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
        _windowHandle = windowHandle;
        _messageSource.AddHook(WindowProcedure);
    }

    public bool TryRegister(
        HotkeyAction action,
        HotkeySetting setting,
        out string? userMessage)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(setting);

        userMessage = null;
        if (_windowHandle == nint.Zero)
        {
            throw new InvalidOperationException("AttachWindow must be called before registering hotkeys.");
        }

        Unregister(action);
        if (!setting.Enabled)
        {
            return true;
        }

        var registrationId = ToRegistrationId(action);
        var modifiers = (uint)setting.Modifiers | NativeMethods.ModNoRepeat;
        if (NativeMethods.RegisterHotKey(_windowHandle, registrationId, modifiers, (uint)setting.Key))
        {
            _registeredActions.Add(action);
            _logger?.Information($"Globaler Hotkey registriert: {action} ({FormatGesture(setting)}).");
            return true;
        }

        var exception = new Win32Exception(Marshal.GetLastWin32Error());
        userMessage = $"Der Hotkey {FormatGesture(setting)} wird bereits von einer anderen Anwendung verwendet.";
        _logger?.Error($"Globaler Hotkey konnte nicht registriert werden: {action}.", exception);
        return false;
    }

    public IReadOnlyList<string> ApplySettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        UnregisterAll();

        var errors = new List<string>();
        if (!TryRegister(HotkeyAction.ToggleFreeze, settings.ToggleFreezeHotkey, out var toggleError)
            && toggleError is not null)
        {
            errors.Add(toggleError);
        }

        if (!TryRegister(HotkeyAction.Refresh, settings.RefreshHotkey, out var refreshError)
            && refreshError is not null)
        {
            errors.Add(refreshError);
        }

        return errors;
    }

    public void Unregister(HotkeyAction action)
    {
        if (!_registeredActions.Remove(action) || _windowHandle == nint.Zero)
        {
            return;
        }

        if (!NativeMethods.UnregisterHotKey(_windowHandle, ToRegistrationId(action)))
        {
            var exception = new Win32Exception(Marshal.GetLastWin32Error());
            _logger?.Error($"Globaler Hotkey konnte nicht freigegeben werden: {action}.", exception);
        }
    }

    public void UnregisterAll()
    {
        foreach (var action in _registeredActions.ToArray())
        {
            Unregister(action);
        }
    }

    public void DetachWindow()
    {
        UnregisterAll();

        if (_messageSource is not null)
        {
            _messageSource.RemoveHook(WindowProcedure);
            _messageSource = null;
        }

        _windowHandle = nint.Zero;
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

    public static string FormatGesture(HotkeySetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        var parts = new List<string>();
        if (setting.Modifiers.HasFlag(GlobalHotkeyModifiers.Control))
        {
            parts.Add("Strg");
        }

        if (setting.Modifiers.HasFlag(GlobalHotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (setting.Modifiers.HasFlag(GlobalHotkeyModifiers.Shift))
        {
            parts.Add("Umschalt");
        }

        if (setting.Modifiers.HasFlag(GlobalHotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(setting.Key.ToString());
        return string.Join("+", parts);
    }

    private static int ToRegistrationId(HotkeyAction action)
    {
        return checked(RegistrationIdBase + (int)action);
    }

    private nint WindowProcedure(
        nint windowHandle,
        int message,
        nint wordParameter,
        nint longParameter,
        ref bool handled)
    {
        if (message != NativeMethods.WmHotkey)
        {
            return nint.Zero;
        }

        var registrationId = unchecked((int)wordParameter.ToInt64());
        var actionValue = registrationId - RegistrationIdBase;
        if (Enum.IsDefined(typeof(HotkeyAction), actionValue))
        {
            var action = (HotkeyAction)actionValue;
            if (_registeredActions.Contains(action))
            {
                handled = true;
                _logger?.Debug($"Globaler Hotkey ausgelöst: {action}.");
                HotkeyPressed?.Invoke(this, new GlobalHotkeyPressedEventArgs(action));
            }
        }

        return nint.Zero;
    }
}
