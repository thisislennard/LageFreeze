using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LageFreeze.Views;

namespace LageFreeze.Tests;

[TestClass]
public sealed class WindowInitializationTests
{
    [STATestMethod]
    public void MainWindow_LoadsCompiledXaml()
    {
        using var window = new WindowScope(new MainWindow());

        Assert.IsNotNull(window.Value.Content);
        Assert.AreEqual("LageFreeze", window.Value.Title);
    }

    [STATestMethod]
    public void SettingsWindow_LoadsCompiledXaml()
    {
        using var window = new WindowScope(new SettingsWindow());

        Assert.IsNotNull(window.Value.Content);
        Assert.AreEqual("Einstellungen – LageFreeze", window.Value.Title);
    }

    [STATestMethod]
    public void MonitorSelectionWindow_LoadsCompiledXaml()
    {
        using var window = new WindowScope(new MonitorSelectionWindow());

        Assert.IsNotNull(window.Value.Content);
        Assert.AreEqual("Monitor auswählen – LageFreeze", window.Value.Title);
    }

    [STATestMethod]
    public void MainWindow_AdaptsHeightToLiveAndFrozenContent()
    {
        var state = new FreezeState();
        using var window = new WindowScope(new MainWindow { DataContext = state });
        window.Value.Show();
        CompleteLayout(window.Value);

        var liveHeight = window.Value.ActualHeight;
        Assert.AreEqual(SizeToContent.Height, window.Value.SizeToContent);
        Assert.IsTrue(liveHeight >= window.Value.MinHeight && liveHeight < 470);

        state.IsFrozen = true;
        CompleteLayout(window.Value);
        var frozenHeight = window.Value.ActualHeight;
        Assert.IsTrue(frozenHeight > liveHeight + 30 && frozenHeight < 530);

        state.IsFrozen = false;
        CompleteLayout(window.Value);
        Assert.AreEqual(liveHeight, window.Value.ActualHeight, 1);
    }

    [STATestMethod]
    public void MainWindow_TitleBarButtons_ExecuteWindowActions()
    {
        using var window = new WindowScope(new MainWindow());
        window.Value.Show();

        var minimizeButton = FindButton(window.Value, "MinimizeButton");
        Assert.IsTrue(minimizeButton.IsEnabled);
        minimizeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.AreEqual(WindowState.Minimized, window.Value.WindowState);

        window.Value.WindowState = WindowState.Normal;
        AssertCloseButtonCloses(window.Value);
    }

    [STATestMethod]
    public void SettingsWindow_CloseButton_ClosesWindow()
    {
        using var window = new WindowScope(new SettingsWindow());
        window.Value.Show();

        AssertCloseButtonCloses(window.Value);
    }

    [STATestMethod]
    public void MonitorSelectionWindow_CloseButton_ClosesWindow()
    {
        using var window = new WindowScope(new MonitorSelectionWindow());
        window.Value.Show();

        AssertCloseButtonCloses(window.Value);
    }

    private static void AssertCloseButtonCloses(System.Windows.Window window)
    {
        var wasClosed = false;
        window.Closed += (_, _) => wasClosed = true;
        var closeButton = FindButton(window, "CloseButton");

        Assert.IsTrue(closeButton.IsEnabled);
        closeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.IsTrue(wasClosed);
    }

    private static Button FindButton(System.Windows.Window window, string name)
    {
        return window.FindName(name) as Button
               ?? throw new AssertFailedException($"Button '{name}' wurde nicht gefunden.");
    }

    private static void CompleteLayout(System.Windows.Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        window.UpdateLayout();
    }

    private sealed class FreezeState : INotifyPropertyChanged
    {
        private bool _isFrozen;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsFrozen
        {
            get => _isFrozen;
            set
            {
                if (_isFrozen == value)
                {
                    return;
                }

                _isFrozen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFrozen)));
            }
        }
    }

    private sealed class WindowScope(System.Windows.Window value) : IDisposable
    {
        public System.Windows.Window Value { get; } = value;

        public void Dispose()
        {
            Value.Close();
        }
    }
}
