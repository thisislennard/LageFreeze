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

    private sealed class WindowScope(System.Windows.Window value) : IDisposable
    {
        public System.Windows.Window Value { get; } = value;

        public void Dispose()
        {
            Value.Close();
        }
    }
}
