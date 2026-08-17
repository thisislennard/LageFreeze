using System.Windows;

namespace LageFreeze.Views;

public partial class MonitorSelectionWindow
{
    public MonitorSelectionWindow()
    {
        InitializeComponent();
    }

    private void CloseButtonClick(object sender, RoutedEventArgs eventArgs)
    {
        Close();
    }
}
