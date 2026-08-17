using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LageFreeze.Models;
using LageFreeze.Services;
using LageFreeze.ViewModels;
using LageFreeze.Views;
using Forms = System.Windows.Forms;

namespace LageFreeze;

internal sealed class ApplicationController : IDisposable
{
    private readonly ILoggingService _logger;
    private readonly SettingsService _settingsService;
    private readonly MonitorService _monitorService;
    private readonly CaptureService _captureService;
    private readonly ScreenshotService _screenshotService;
    private readonly HotkeyService _hotkeyService;
    private readonly AutostartService _autostartService;
    private readonly SystemTrayService _trayService;
    private readonly SemaphoreSlim _freezeOperation = new(1, 1);
    private readonly List<IdentificationWindow> _identificationWindows = [];
    private AppSettings _settings = AppSettings.CreateDefault();
    private MainWindow? _mainWindow;
    private System.Windows.Window? _dialogWindow;
    private MainViewModel? _mainViewModel;
    private FreezeWindow? _freezeWindow;
    private BitmapSource? _frozenImage;
    private bool _exitRequested;
    private bool _disposed;

    public ApplicationController(ILoggingService logger)
    {
        _logger = logger;
        _settingsService = new SettingsService(logger: logger);
        _monitorService = new MonitorService(logger);
        _captureService = new CaptureService(logger);
        _screenshotService = new ScreenshotService(logger: logger);
        _hotkeyService = new HotkeyService(logger);
        _autostartService = new AutostartService(logger);
        _trayService = new SystemTrayService();
    }

    public async Task StartAsync(IReadOnlyCollection<string> arguments)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.Information("LageFreeze wird gestartet.");

        _settings = await _settingsService.LoadAsync().ConfigureAwait(true);
        var monitors = _monitorService.GetMonitors();
        LogMonitors(monitors);
        var selectedMonitor = MonitorMatcher.FindBestMatch(_settings.SelectedMonitor, monitors);

        _mainViewModel = new MainViewModel(
            FreezeAsync,
            RefreshFrozenImageAsync,
            RestoreLiveAsync,
            ChangeMonitorAsync,
            IdentifyMonitorsAsync,
            OpenSettingsAsync,
            SaveScreenshotAsync,
            ApplyDrawingMode,
            HandleCommandError);
        _mainViewModel.SetSelectedMonitor(selectedMonitor);
        _mainViewModel.DrawingMode = _settings.DefaultDrawingMode;

        _mainWindow = new MainWindow { DataContext = _mainViewModel };
        _mainWindow.Closing += MainWindowClosing;
        _mainWindow.StateChanged += MainWindowStateChanged;
        System.Windows.Application.Current.MainWindow = _mainWindow;

        var handle = new WindowInteropHelper(_mainWindow).EnsureHandle();
        _monitorService.AttachWindow(handle);
        _monitorService.MonitorsChanged += MonitorsChanged;
        _hotkeyService.AttachWindow(handle);
        _hotkeyService.HotkeyPressed += HotkeyPressed;
        var hotkeyErrors = ApplyHotkeys(showErrors: false);
        ConnectTrayEvents();
        ApplyTrayVisibility();

        _ = TrySynchronizeAutostart();

        var startMinimized = arguments.Any(argument =>
                                 string.Equals(argument, "--minimized", StringComparison.OrdinalIgnoreCase))
                             || _settings.StartMinimized;
        if (selectedMonitor is null || !startMinimized)
        {
            ShowMainWindow();
        }
        else if (!_settings.UseSystemTray && _mainWindow is not null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Minimized;
        }

        if (_settings.SelectedMonitor is not null && selectedMonitor is null)
        {
            ShowWarning(
                "Der zuvor ausgewählte Monitor ist nicht verfügbar. Bitte wähle einen anderen Monitor aus.");
        }

        if (hotkeyErrors.Count > 0)
        {
            ShowWarning(string.Join(Environment.NewLine, hotkeyErrors));
        }

        if (selectedMonitor is null)
        {
            await ChangeMonitorAsync().ConfigureAwait(true);
        }
    }

    public void EmergencyRestore()
    {
        void Restore()
        {
            foreach (var window in _identificationWindows.ToArray())
            {
                window.Close();
            }

            _identificationWindows.Clear();
            _freezeWindow?.Close();
            _freezeWindow = null;
            _frozenImage = null;
            _mainViewModel?.SetFrozen(false);
        }

        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(Restore);
        }
        else
        {
            Restore();
        }
    }

    public async Task ExitAsync()
    {
        if (_exitRequested)
        {
            return;
        }

        _exitRequested = true;
        EmergencyRestore();

        try
        {
            await _settingsService.SaveAsync(_settings).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            _logger.Error("Einstellungen konnten beim Beenden nicht gespeichert werden.", exception);
        }

        _logger.Information("LageFreeze wird beendet.");
        _trayService.Hide();

        // Release all hooks and registrations while the owning HWND is still
        // valid. Waiting until Application.OnExit would attempt UnregisterHotKey
        // after MainWindow.Close destroyed the handle (Win32 error 1400).
        _hotkeyService.DetachWindow();
        _monitorService.DetachWindow();
        _mainWindow?.Close();
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        EmergencyRestore();
        _hotkeyService.HotkeyPressed -= HotkeyPressed;
        _monitorService.MonitorsChanged -= MonitorsChanged;
        _hotkeyService.Dispose();
        _monitorService.Dispose();
        _trayService.Dispose();
        _settingsService.Dispose();
        _freezeOperation.Dispose();
        _disposed = true;
    }

    private async Task FreezeAsync()
    {
        await _freezeOperation.WaitAsync().ConfigureAwait(true);
        FreezeWindow? newOverlay = null;
        try
        {
            if (_freezeWindow is not null)
            {
                return;
            }

            var target = ResolveSelectedMonitor();
            if (target is null)
            {
                _mainViewModel?.SetSelectedMonitor(null);
                ShowWarning("Der ausgewählte Monitor ist nicht mehr verfügbar. Bitte wähle ihn erneut aus.");
                return;
            }

            var image = await Task.Run(() => _captureService.CaptureMonitor(target)).ConfigureAwait(true);
            newOverlay = new FreezeWindow();
            newOverlay.Configure(
                target.Bounds.Left,
                target.Bounds.Top,
                target.Bounds.Width,
                target.Bounds.Height,
                image);
            newOverlay.SetDrawingMode(_mainViewModel?.DrawingMode ?? DrawingMode.Original);
            newOverlay.SetFrozenIndicator(
                _settings.ShowFrozenIndicator,
                _settings.FrozenIndicatorPosition);
            newOverlay.Closed += FreezeWindowClosed;
            newOverlay.Show();
            newOverlay.ReapplyPhysicalBounds();

            _freezeWindow = newOverlay;
            _frozenImage = image;
            _mainViewModel?.SetSelectedMonitor(target);
            SetFrozenState(true);
            _logger.Information($"Freeze gestartet: {target.DisplayText}.");
        }
        catch
        {
            if (newOverlay is not null)
            {
                newOverlay.Closed -= FreezeWindowClosed;
                newOverlay.Close();
            }

            if (ReferenceEquals(_freezeWindow, newOverlay))
            {
                _freezeWindow = null;
                _frozenImage = null;
                SetFrozenState(false);
            }

            throw;
        }
        finally
        {
            _freezeOperation.Release();
        }
    }

    private async Task RefreshFrozenImageAsync()
    {
        await _freezeOperation.WaitAsync().ConfigureAwait(true);
        FreezeWindow? overlay = null;
        try
        {
            overlay = _freezeWindow;
            var target = ResolveSelectedMonitor();
            if (overlay is null || target is null)
            {
                if (overlay is not null)
                {
                    RestoreLiveCore();
                }

                ShowWarning("Das Standbild kann nicht aktualisiert werden, weil der Monitor nicht verfügbar ist.");
                return;
            }

            overlay.Hide();
            await overlay.Dispatcher.InvokeAsync(
                static () => { },
                DispatcherPriority.ApplicationIdle);
            FlushDesktopComposition();

            var image = await Task.Run(() => _captureService.CaptureMonitor(target)).ConfigureAwait(true);
            if (_freezeWindow != overlay)
            {
                return;
            }

            overlay.UpdateImage(image);
            _frozenImage = image;
            _logger.Information($"Standbild aktualisiert: {target.DisplayText}.");
        }
        finally
        {
            if (overlay is not null && _freezeWindow == overlay && !overlay.IsVisible)
            {
                overlay.Show();
                overlay.ReapplyPhysicalBounds();
            }

            _freezeOperation.Release();
        }
    }

    private async Task RestoreLiveAsync()
    {
        await _freezeOperation.WaitAsync().ConfigureAwait(true);
        try
        {
            RestoreLiveCore();
        }
        finally
        {
            _freezeOperation.Release();
        }
    }

    private void RestoreLiveCore()
    {
        var overlay = _freezeWindow;
        _freezeWindow = null;
        _frozenImage = null;
        if (overlay is not null)
        {
            overlay.Closed -= FreezeWindowClosed;
            overlay.Close();
            _logger.Information("Freeze beendet; Live-Bild wiederhergestellt.");
        }

        SetFrozenState(false);
    }

    private Task ChangeMonitorAsync()
    {
        if (_dialogWindow is not null)
        {
            _dialogWindow.Activate();
            return Task.CompletedTask;
        }

        var monitors = _monitorService.GetMonitors();
        if (monitors.Count == 0)
        {
            ShowWarning("Windows meldet aktuell keinen verfügbaren Monitor.");
            return Task.CompletedTask;
        }

        var current = ResolveSelectedMonitor();
        var viewModel = new MonitorSelectionViewModel(monitors, current, IdentifyMonitorsAsync);
        var window = new MonitorSelectionWindow
        {
            DataContext = viewModel,
            Owner = _mainWindow?.IsVisible == true ? _mainWindow : null,
        };
        viewModel.Confirmed += (_, _) => window.DialogResult = true;
        viewModel.Canceled += (_, _) => window.DialogResult = false;
        viewModel.Error += (_, exception) => HandleCommandError(exception);

        _dialogWindow = window;
        bool? result;
        try
        {
            result = window.ShowDialog();
        }
        finally
        {
            _dialogWindow = null;
        }

        if (result == true && viewModel.SelectedMonitor is { } selected)
        {
            if (_freezeWindow is not null)
            {
                RestoreLiveCore();
            }

            _settings.SelectedMonitor = MonitorSelection.FromMonitor(selected);
            _mainViewModel?.SetSelectedMonitor(selected);
            _settingsService.Save(_settings);
            _logger.Information($"Monitor ausgewählt: {selected.DisplayText}.");
        }

        return Task.CompletedTask;
    }

    private async Task IdentifyMonitorsAsync()
    {
        if (_identificationWindows.Count > 0)
        {
            return;
        }

        var monitors = _monitorService.GetMonitors();
        try
        {
            foreach (var monitor in monitors)
            {
                var window = new IdentificationWindow();
                window.Configure(
                    monitor.DisplayNumber,
                    monitor.DisplayName,
                    monitor.Bounds.Left,
                    monitor.Bounds.Top,
                    monitor.Bounds.Width,
                    monitor.Bounds.Height);
                _identificationWindows.Add(window);
                window.Show();
                window.ReapplyPhysicalBounds();
            }

            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(true);
        }
        finally
        {
            foreach (var window in _identificationWindows.ToArray())
            {
                window.Close();
            }

            _identificationWindows.Clear();
        }
    }

    private Task OpenSettingsAsync()
    {
        if (_dialogWindow is not null)
        {
            _dialogWindow.Activate();
            return Task.CompletedTask;
        }

        var monitors = _monitorService.GetMonitors();
        var selected = ResolveSelectedMonitor();
        var viewModel = new SettingsViewModel(_settings, monitors, selected, BrowseFolder);
        var window = new SettingsWindow
        {
            DataContext = viewModel,
            Owner = _mainWindow?.IsVisible == true ? _mainWindow : null,
        };

        viewModel.Canceled += (_, _) => window.DialogResult = false;
        viewModel.Saved += async (_, eventArgs) =>
        {
            try
            {
                await ApplySettingsAsync(eventArgs.Settings).ConfigureAwait(true);
                window.DialogResult = true;
            }
            catch (Exception exception)
            {
                _logger.Error("Einstellungen konnten nicht übernommen werden.", exception);
                ShowError("Die Einstellungen konnten nicht gespeichert werden.");
            }
        };
        _dialogWindow = window;
        try
        {
            window.ShowDialog();
        }
        finally
        {
            _dialogWindow = null;
        }

        return Task.CompletedTask;
    }

    private async Task ApplySettingsAsync(AppSettings settings)
    {
        var previousSelection = _settings.SelectedMonitor;
        var targetChanged = !SameMonitorSelection(previousSelection, settings.SelectedMonitor);
        if (targetChanged && _freezeWindow is not null)
        {
            RestoreLiveCore();
        }

        var previousSettings = _settings;
        _settings = settings;
        try
        {
            if (!TrySynchronizeAutostart())
            {
                throw new InvalidOperationException("The Windows autostart setting could not be applied.");
            }

            await _settingsService.SaveAsync(_settings).ConfigureAwait(true);
        }
        catch
        {
            _settings = previousSettings;
            _ = TrySynchronizeAutostart();
            throw;
        }

        var selected = MonitorMatcher.FindBestMatch(_settings.SelectedMonitor, _monitorService.Monitors);
        _mainViewModel?.SetSelectedMonitor(selected);
        if (_mainViewModel is not null)
        {
            _mainViewModel.DrawingMode = _settings.DefaultDrawingMode;
        }

        ApplyFrozenIndicatorSettings();

        ApplyHotkeys(showErrors: true);
        ApplyTrayVisibility();
    }

    private async Task SaveScreenshotAsync()
    {
        var image = _frozenImage;
        var monitor = ResolveSelectedMonitor();
        if (image is null)
        {
            ShowWarning("Es ist kein Standbild vorhanden, das gespeichert werden kann.");
            return;
        }

        var path = await _screenshotService.SavePngAsync(
                image,
                monitor,
                _settings.ResolveScreenshotFolder())
            .ConfigureAwait(true);
        ShowInformation($"Das Standbild wurde gespeichert:\n{path}");
    }

    private void ApplyDrawingMode(DrawingMode mode)
    {
        _freezeWindow?.SetDrawingMode(mode);
    }

    private void ApplyFrozenIndicatorSettings()
    {
        _freezeWindow?.SetFrozenIndicator(
            _settings.ShowFrozenIndicator,
            _settings.FrozenIndicatorPosition);
    }

    private IReadOnlyList<string> ApplyHotkeys(bool showErrors)
    {
        var errors = _hotkeyService.ApplySettings(_settings);
        if (showErrors && errors.Count > 0)
        {
            ShowWarning(string.Join(Environment.NewLine, errors));
        }

        return errors;
    }

    private bool TrySynchronizeAutostart()
    {
        try
        {
            if (!_settings.StartWithWindows && !_autostartService.IsEnabled())
            {
                return true;
            }

            _autostartService.SetEnabled(
                _settings.StartWithWindows,
                Environment.ProcessPath,
                _settings.StartMinimized);
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error("Autostart konnte nicht synchronisiert werden.", exception);
            return false;
        }
    }

    private MonitorInfo? ResolveSelectedMonitor()
    {
        var monitors = _monitorService.Refresh();
        var match = MonitorMatcher.FindBestMatch(_settings.SelectedMonitor, monitors);
        if (match is not null)
        {
            _mainViewModel?.SetSelectedMonitor(match);
        }

        return match;
    }

    private void MonitorsChanged(object? sender, MonitorsChangedEventArgs eventArgs)
    {
        LogMonitors(eventArgs.Current);
        CloseIdentificationWindows();
        if (_dialogWindow is not null)
        {
            _dialogWindow.Close();
            _dialogWindow = null;
            ShowWarning("Die Monitoranordnung wurde geändert. Bitte öffne die Auswahl erneut.");
        }

        _ = HandleMonitorChangeAsync(eventArgs.Current);
    }

    private async Task HandleMonitorChangeAsync(IReadOnlyList<MonitorInfo> monitors)
    {
        var previous = _mainViewModel?.SelectedMonitor;
        var current = MonitorMatcher.FindBestMatch(_settings.SelectedMonitor, monitors);
        var targetGeometryChanged = previous is not null
                                    && current is not null
                                    && previous.Bounds != current.Bounds;

        if (_freezeWindow is not null && (current is null || targetGeometryChanged))
        {
            await RestoreLiveAsync().ConfigureAwait(true);
            ShowWarning(current is null
                ? "Der eingefrorene Monitor wurde getrennt. Das Live-Bild wurde sicher wiederhergestellt."
                : "Die Anzeigeeinstellungen des eingefrorenen Monitors wurden geändert. Das Live-Bild wurde sicher wiederhergestellt.");
        }

        _mainViewModel?.SetSelectedMonitor(current);
    }

    private void HotkeyPressed(object? sender, GlobalHotkeyPressedEventArgs eventArgs)
    {
        _ = RunUiActionAsync(async () =>
        {
            if (eventArgs.Action == HotkeyAction.ToggleFreeze)
            {
                if (_freezeWindow is null)
                {
                    await FreezeAsync().ConfigureAwait(true);
                }
                else
                {
                    await RestoreLiveAsync().ConfigureAwait(true);
                }
            }
            else if (eventArgs.Action == HotkeyAction.Refresh && _freezeWindow is not null)
            {
                await RefreshFrozenImageAsync().ConfigureAwait(true);
            }
        });
    }

    private void ConnectTrayEvents()
    {
        _trayService.OpenRequested += (_, _) => Dispatch(ShowMainWindow);
        _trayService.FreezeRequested += (_, _) => DispatchAsync(FreezeAsync);
        _trayService.RefreshRequested += (_, _) => DispatchAsync(RefreshFrozenImageAsync);
        _trayService.LiveRequested += (_, _) => DispatchAsync(RestoreLiveAsync);
        _trayService.SettingsRequested += (_, _) => DispatchAsync(OpenSettingsAsync);
        _trayService.ExitRequested += (_, _) => DispatchAsync(ExitAsync);
    }

    private void ApplyTrayVisibility()
    {
        if (_settings.UseSystemTray)
        {
            _trayService.Show();
        }
        else
        {
            _trayService.Hide();
            if (_mainWindow?.IsVisible == false)
            {
                ShowMainWindow();
            }
        }
    }

    private void SetFrozenState(bool isFrozen)
    {
        _mainViewModel?.SetFrozen(isFrozen);
        _trayService.UpdateFrozenState(isFrozen);
    }

    private void FreezeWindowClosed(object? sender, EventArgs eventArgs)
    {
        if (ReferenceEquals(sender, _freezeWindow))
        {
            _freezeWindow = null;
            _frozenImage = null;
            SetFrozenState(false);
        }
    }

    private void MainWindowClosing(object? sender, CancelEventArgs eventArgs)
    {
        if (_exitRequested)
        {
            return;
        }

        eventArgs.Cancel = true;
        if (_settings.UseSystemTray && _settings.MinimizeToTray)
        {
            _mainWindow?.Hide();
            _trayService.Show();
            return;
        }

        _ = RunUiActionAsync(ExitAsync);
    }

    private void MainWindowStateChanged(object? sender, EventArgs eventArgs)
    {
        if (_mainWindow?.WindowState == WindowState.Minimized && _settings.UseSystemTray)
        {
            _mainWindow.Hide();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private string? BrowseFolder(string currentFolder)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Ordner für gespeicherte Standbilder auswählen",
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(currentFolder) ? currentFolder : string.Empty,
        };
        return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }

    private void Dispatch(Action action)
    {
        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(action);
    }

    private void DispatchAsync(Func<Task> action)
    {
        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(() => RunUiActionAsync(action));
    }

    private async Task RunUiActionAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            HandleCommandError(exception);
        }
    }

    private void HandleCommandError(Exception exception)
    {
        _logger.Error("Vorgang fehlgeschlagen.", exception);
        ShowError("Der Vorgang konnte nicht abgeschlossen werden. Weitere Details stehen im lokalen Log.");
    }

    private void ShowInformation(string message) => ShowMessage(message, "LageFreeze", MessageBoxImage.Information);

    private void ShowWarning(string message) => ShowMessage(message, "LageFreeze", MessageBoxImage.Warning);

    private void ShowError(string message) => ShowMessage(message, "LageFreeze – Fehler", MessageBoxImage.Error);

    private void ShowMessage(string message, string title, MessageBoxImage icon)
    {
        if (_mainWindow?.IsVisible == true)
        {
            System.Windows.MessageBox.Show(_mainWindow, message, title, MessageBoxButton.OK, icon);
        }
        else
        {
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }

    private void CloseIdentificationWindows()
    {
        foreach (var window in _identificationWindows.ToArray())
        {
            window.Close();
        }

        _identificationWindows.Clear();
    }

    private void LogMonitors(IReadOnlyList<MonitorInfo> monitors)
    {
        foreach (var monitor in monitors)
        {
            _logger.Information(
                $"Monitor erkannt: {monitor.DisplayText}, Position {monitor.Bounds.Left},{monitor.Bounds.Top}, DPI {monitor.DpiX}x{monitor.DpiY}.");
        }
    }

    private static bool SameMonitorSelection(MonitorSelection? first, MonitorSelection? second)
    {
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return string.Equals(first.StableId, second.StableId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(first.DeviceName, second.DeviceName, StringComparison.OrdinalIgnoreCase);
    }

    private static void FlushDesktopComposition()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6))
        {
            _ = DwmFlush();
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();
}
