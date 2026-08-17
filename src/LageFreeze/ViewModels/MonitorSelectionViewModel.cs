using LageFreeze.Models;

namespace LageFreeze.ViewModels;

internal sealed class MonitorSelectionViewModel : ObservableObject
{
    private MonitorInfo? _selectedMonitor;

    public MonitorSelectionViewModel(
        IReadOnlyList<MonitorInfo> monitors,
        MonitorInfo? selectedMonitor,
        Func<Task> identifyMonitors)
    {
        Monitors = monitors;
        _selectedMonitor = selectedMonitor ?? monitors.FirstOrDefault();
        IdentifyMonitorsCommand = new AsyncRelayCommand(identifyMonitors, exception => Error?.Invoke(this, exception));
        ConfirmCommand = new RelayCommand(
            () => Confirmed?.Invoke(this, EventArgs.Empty),
            () => SelectedMonitor is not null);
        CancelCommand = new RelayCommand(() => Canceled?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? Confirmed;

    public event EventHandler? Canceled;

    public event EventHandler<Exception>? Error;

    public IReadOnlyList<MonitorInfo> Monitors { get; }

    public MonitorInfo? SelectedMonitor
    {
        get => _selectedMonitor;
        set
        {
            if (SetProperty(ref _selectedMonitor, value))
            {
                ConfirmCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand IdentifyMonitorsCommand { get; }

    public RelayCommand ConfirmCommand { get; }

    public RelayCommand CancelCommand { get; }
}
