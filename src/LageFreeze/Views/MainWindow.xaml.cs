using System.Windows;

namespace LageFreeze.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MinimizeButtonClick(object sender, RoutedEventArgs eventArgs)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButtonClick(object sender, RoutedEventArgs eventArgs)
    {
        Close();
    }
}
