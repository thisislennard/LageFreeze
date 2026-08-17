using System.Windows;

namespace LageFreeze.Views;

public partial class SettingsWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButtonClick(object sender, RoutedEventArgs eventArgs)
    {
        Close();
    }
}
