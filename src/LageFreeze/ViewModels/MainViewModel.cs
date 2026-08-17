using LageFreeze.Models;

namespace LageFreeze.ViewModels;

internal sealed class MainViewModel : ObservableObject
{
    private MonitorInfo? _selectedMonitor;
    private bool _isFrozen;
    private DrawingMode _drawingMode;

    public MainViewModel(
        Func<Task> freeze,
        Func<Task> refresh,
        Func<Task> restoreLive,
        Func<Task> changeMonitor,
        Func<Task> identifyMonitors,
        Func<Task> openSettings,
        Func<Task> saveScreenshot,
        Action<DrawingMode> drawingModeChanged,
        Action<Exception> handleError)
    {
        FreezeCommand = new AsyncRelayCommand(freeze, handleError, () => SelectedMonitor is not null && !IsFrozen);
        RefreshCommand = new AsyncRelayCommand(refresh, handleError, () => IsFrozen);
        RestoreLiveCommand = new AsyncRelayCommand(restoreLive, handleError, () => IsFrozen);
        ChangeMonitorCommand = new AsyncRelayCommand(changeMonitor, handleError);
        IdentifyMonitorsCommand = new AsyncRelayCommand(identifyMonitors, handleError);
        OpenSettingsCommand = new AsyncRelayCommand(openSettings, handleError);
        SaveScreenshotCommand = new AsyncRelayCommand(saveScreenshot, handleError, () => IsFrozen);
        DrawingModeChanged = drawingModeChanged;
    }

    public AsyncRelayCommand FreezeCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand RestoreLiveCommand { get; }

    public AsyncRelayCommand ChangeMonitorCommand { get; }

    public AsyncRelayCommand IdentifyMonitorsCommand { get; }

    public AsyncRelayCommand OpenSettingsCommand { get; }

    public AsyncRelayCommand SaveScreenshotCommand { get; }

    public Action<DrawingMode> DrawingModeChanged { get; }

    public IReadOnlyList<DrawingModeOption> DrawingModes { get; } =
    [
        new(DrawingMode.Original, "Original"),
        new(DrawingMode.Dimmed, "Leicht abgedunkelt"),
        new(DrawingMode.StronglyDimmed, "Stark abgedunkelt"),
    ];

    public MonitorInfo? SelectedMonitor
    {
        get => _selectedMonitor;
        private set
        {
            if (SetProperty(ref _selectedMonitor, value))
            {
                OnPropertyChanged(nameof(HasSelectedMonitor));
                FreezeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasSelectedMonitor => SelectedMonitor is not null;

    public bool IsFrozen
    {
        get => _isFrozen;
        private set
        {
            if (!SetProperty(ref _isFrozen, value))
            {
                return;
            }

            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusDescription));
            FreezeCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            RestoreLiveCommand.NotifyCanExecuteChanged();
            SaveScreenshotCommand.NotifyCanExecuteChanged();
        }
    }

    public string StatusText => IsFrozen ? "EINGEFROREN" : "LIVE";

    public string StatusDescription => IsFrozen
        ? "Der ausgewählte Monitor zeigt ein Standbild."
        : "Der ausgewählte Monitor zeigt den aktuellen Inhalt.";

    public DrawingMode DrawingMode
    {
        get => _drawingMode;
        set
        {
            if (SetProperty(ref _drawingMode, value))
            {
                DrawingModeChanged(value);
            }
        }
    }

    public void SetSelectedMonitor(MonitorInfo? monitor) => SelectedMonitor = monitor;

    public void SetFrozen(bool frozen) => IsFrozen = frozen;
}

internal sealed record DrawingModeOption(DrawingMode Value, string DisplayName);
