using System.Windows;
using System.Windows.Threading;
using LageFreeze.Services;

namespace LageFreeze;

public partial class App : System.Windows.Application
{
    private readonly LoggingService _logger = new();
    private ApplicationController? _controller;
    private Mutex? _singleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RegisterErrorHandlers();

        try
        {
            _singleInstanceMutex = new Mutex(
                initiallyOwned: true,
                name: "Local\\LageFreeze.SingleInstance",
                createdNew: out var isFirstInstance);
            if (!isFirstInstance)
            {
                System.Windows.MessageBox.Show(
                    "LageFreeze läuft bereits.",
                    "LageFreeze",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown();
                return;
            }

            if (!DpiAwarenessService.TryEnablePerMonitorV2(out var dpiError) && dpiError is not null)
            {
                _logger.Warning(dpiError);
            }

            _controller = new ApplicationController(_logger);
            await _controller.StartAsync(e.Args).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            _logger.Error("LageFreeze konnte nicht gestartet werden.", exception);
            _controller?.EmergencyRestore();
            System.Windows.MessageBox.Show(
                "LageFreeze konnte nicht gestartet werden. Weitere Details stehen im lokalen Log.",
                "LageFreeze – Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _controller?.EmergencyRestore();
        _controller?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private void RegisterErrorHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        _logger.Error("Unbehandelter Fehler auf dem UI-Thread.", eventArgs.Exception);
        _controller?.EmergencyRestore();
        eventArgs.Handled = true;
        System.Windows.MessageBox.Show(
            "LageFreeze wurde nach einem unerwarteten Fehler sicher beendet. Details stehen im lokalen Log.",
            "LageFreeze – Fehler",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Shutdown(-1);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs)
    {
        _logger.Error("Unbeobachteter Hintergrundfehler.", eventArgs.Exception);
        eventArgs.SetObserved();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        _logger.Error(
            "Unbehandelter Prozessfehler.",
            eventArgs.ExceptionObject as Exception);
        _controller?.EmergencyRestore();
    }
}
