using System;
using System.Windows;
using System.Windows.Threading;

namespace LageFreeze.Views;

/// <summary>
/// Short-lived overlay used to map detected monitors to physical screens.
/// </summary>
public partial class IdentificationWindow : PhysicalPixelWindow
{
    public IdentificationWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Configures the displayed number, friendly name and physical monitor bounds.
    /// </summary>
    public void Configure(
        int displayNumber,
        string? displayName,
        Int32Rect physicalBounds)
    {
        if (displayNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayNumber),
                "Die Monitornummer muss größer als null sein.");
        }

        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(
                () => Configure(displayNumber, displayName, physicalBounds),
                DispatcherPriority.Send);
            return;
        }

        MonitorNumberText.Text = displayNumber.ToString(System.Globalization.CultureInfo.CurrentCulture);
        MonitorNameText.Text = string.IsNullOrWhiteSpace(displayName)
            ? $"Monitor {displayNumber}"
            : displayName;
        SetPhysicalBounds(physicalBounds);
    }

    /// <summary>
    /// Convenience overload for services that keep monitor bounds as scalar values.
    /// </summary>
    public void Configure(
        int displayNumber,
        string? displayName,
        int left,
        int top,
        int width,
        int height)
    {
        Configure(
            displayNumber,
            displayName,
            new Int32Rect(left, top, width, height));
    }
}
