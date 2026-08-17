using LageFreeze.Models;
using LageFreeze.Services;

namespace LageFreeze.ViewModels;

internal sealed class SettingsSavedEventArgs : EventArgs
{
    public SettingsSavedEventArgs(AppSettings settings) => Settings = settings;

    public AppSettings Settings { get; }
}

internal sealed class SettingsViewModel : ObservableObject
{
    private readonly Func<string, string?> _browseFolder;
    private MonitorInfo? _selectedDefaultMonitor;
    private bool _isAutostartEnabled;
    private bool _startMinimized;
    private bool _useSystemTray;
    private bool _minimizeToTray;
    private bool _enableToggleHotkey;
    private bool _enableRefreshHotkey;
    private string _toggleHotkeyText;
    private string _refreshHotkeyText;
    private DrawingMode _defaultDrawingMode;
    private string _screenshotFolder;
    private string _validationMessage = string.Empty;

    public SettingsViewModel(
        AppSettings settings,
        IReadOnlyList<MonitorInfo> monitors,
        MonitorInfo? selectedMonitor,
        Func<string, string?> browseFolder)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Monitors = monitors;
        _selectedDefaultMonitor = selectedMonitor ?? monitors.FirstOrDefault();
        _browseFolder = browseFolder;
        _isAutostartEnabled = settings.StartWithWindows;
        _startMinimized = settings.StartMinimized;
        _useSystemTray = settings.UseSystemTray;
        _minimizeToTray = settings.MinimizeToTray;
        _enableToggleHotkey = settings.ToggleFreezeHotkey.Enabled;
        _enableRefreshHotkey = settings.RefreshHotkey.Enabled;
        _toggleHotkeyText = HotkeyTextConverter.Format(settings.ToggleFreezeHotkey);
        _refreshHotkeyText = HotkeyTextConverter.Format(settings.RefreshHotkey);
        _defaultDrawingMode = settings.DefaultDrawingMode;
        _screenshotFolder = settings.ScreenshotFolder ?? settings.ResolveScreenshotFolder();

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => Canceled?.Invoke(this, EventArgs.Empty));
        ResetHotkeysCommand = new RelayCommand(ResetHotkeys);
        BrowseScreenshotFolderCommand = new RelayCommand(BrowseScreenshotFolder);
    }

    public event EventHandler<SettingsSavedEventArgs>? Saved;

    public event EventHandler? Canceled;

    public RelayCommand SaveCommand { get; }

    public RelayCommand CancelCommand { get; }

    public RelayCommand ResetHotkeysCommand { get; }

    public RelayCommand BrowseScreenshotFolderCommand { get; }

    public IReadOnlyList<MonitorInfo> Monitors { get; }

    public MonitorInfo? SelectedDefaultMonitor
    {
        get => _selectedDefaultMonitor;
        set => SetProperty(ref _selectedDefaultMonitor, value);
    }

    public bool IsAutostartEnabled
    {
        get => _isAutostartEnabled;
        set => SetProperty(ref _isAutostartEnabled, value);
    }

    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetProperty(ref _startMinimized, value);
    }

    public bool UseSystemTray
    {
        get => _useSystemTray;
        set => SetProperty(ref _useSystemTray, value);
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetProperty(ref _minimizeToTray, value);
    }

    public bool EnableToggleHotkey
    {
        get => _enableToggleHotkey;
        set => SetProperty(ref _enableToggleHotkey, value);
    }

    public bool EnableRefreshHotkey
    {
        get => _enableRefreshHotkey;
        set => SetProperty(ref _enableRefreshHotkey, value);
    }

    public string ToggleHotkeyText
    {
        get => _toggleHotkeyText;
        set => SetProperty(ref _toggleHotkeyText, value);
    }

    public string RefreshHotkeyText
    {
        get => _refreshHotkeyText;
        set => SetProperty(ref _refreshHotkeyText, value);
    }

    public DrawingMode DefaultDrawingMode
    {
        get => _defaultDrawingMode;
        set => SetProperty(ref _defaultDrawingMode, value);
    }

    public string ScreenshotFolder
    {
        get => _screenshotFolder;
        set => SetProperty(ref _screenshotFolder, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    private void Save()
    {
        ValidationMessage = string.Empty;
        if (!HotkeyTextConverter.TryParse(
                ToggleHotkeyText,
                EnableToggleHotkey,
                out var toggle,
                out var toggleError))
        {
            ValidationMessage = $"Freeze / Live: {toggleError}";
            return;
        }

        if (!HotkeyTextConverter.TryParse(
                RefreshHotkeyText,
                EnableRefreshHotkey,
                out var refresh,
                out var refreshError))
        {
            ValidationMessage = $"Aktualisieren: {refreshError}";
            return;
        }

        if (toggle.Enabled
            && refresh.Enabled
            && toggle.Key == refresh.Key
            && toggle.Modifiers == refresh.Modifiers)
        {
            ValidationMessage = "Beide Funktionen benötigen unterschiedliche Tastenkürzel.";
            return;
        }

        var settings = new AppSettings
        {
            SelectedMonitor = SelectedDefaultMonitor is null
                ? null
                : MonitorSelection.FromMonitor(SelectedDefaultMonitor),
            StartWithWindows = IsAutostartEnabled,
            StartMinimized = StartMinimized,
            UseSystemTray = UseSystemTray,
            MinimizeToTray = MinimizeToTray,
            ToggleFreezeHotkey = toggle,
            RefreshHotkey = refresh,
            DefaultDrawingMode = DefaultDrawingMode,
            ScreenshotFolder = string.IsNullOrWhiteSpace(ScreenshotFolder)
                ? null
                : ScreenshotFolder.Trim(),
        };
        Saved?.Invoke(this, new SettingsSavedEventArgs(settings));
    }

    private void ResetHotkeys()
    {
        var toggle = HotkeySetting.CreateDefaultToggle();
        var refresh = HotkeySetting.CreateDefaultRefresh();
        EnableToggleHotkey = true;
        EnableRefreshHotkey = true;
        ToggleHotkeyText = HotkeyTextConverter.Format(toggle);
        RefreshHotkeyText = HotkeyTextConverter.Format(refresh);
        ValidationMessage = string.Empty;
    }

    private void BrowseScreenshotFolder()
    {
        var selected = _browseFolder(ScreenshotFolder);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            ScreenshotFolder = selected;
        }
    }
}
