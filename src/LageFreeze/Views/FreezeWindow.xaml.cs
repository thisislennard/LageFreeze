using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LageFreeze.Models;

namespace LageFreeze.Views;

/// <summary>
/// Borderless overlay that presents one captured monitor frame.
/// </summary>
public partial class FreezeWindow : PhysicalPixelWindow
{
    public FreezeWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Configures the overlay using physical virtual-desktop pixels and its initial image.
    /// Call this before Show whenever possible.
    /// </summary>
    public void Configure(Int32Rect physicalBounds, BitmapSource frozenImage)
    {
        ArgumentNullException.ThrowIfNull(frozenImage);

        SetPhysicalBounds(physicalBounds);
        UpdateImage(frozenImage);
    }

    /// <summary>
    /// Convenience overload for services that keep monitor bounds as scalar values.
    /// </summary>
    public void Configure(
        int left,
        int top,
        int width,
        int height,
        BitmapSource frozenImage)
    {
        Configure(new Int32Rect(left, top, width, height), frozenImage);
    }

    /// <summary>
    /// Atomically replaces the displayed frame without clearing the previous frame first.
    /// </summary>
    public void UpdateImage(BitmapSource frozenImage)
    {
        ArgumentNullException.ThrowIfNull(frozenImage);

        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(
                () => UpdateImage(frozenImage),
                DispatcherPriority.Render);
            return;
        }

        FrozenImage.Source = frozenImage;
    }

    public void SetDrawingMode(DrawingMode mode)
    {
        DimmingOverlay.Opacity = mode switch
        {
            DrawingMode.Original => 0,
            DrawingMode.Dimmed => 0.28,
            DrawingMode.StronglyDimmed => 0.52,
            _ => 0,
        };
    }

    /// <summary>
    /// Shows or hides the non-interactive freeze marker and anchors it to the
    /// requested monitor corner. The marker is part of the overlay, so hiding
    /// the overlay for a refresh also excludes it from the new capture.
    /// </summary>
    public void SetFrozenIndicator(bool isVisible, FrozenIndicatorPosition position)
    {
        if (!Enum.IsDefined(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        FrozenIndicator.HorizontalAlignment = position is FrozenIndicatorPosition.TopLeft
            or FrozenIndicatorPosition.BottomLeft
            ? System.Windows.HorizontalAlignment.Left
            : System.Windows.HorizontalAlignment.Right;
        FrozenIndicator.VerticalAlignment = position is FrozenIndicatorPosition.TopLeft
            or FrozenIndicatorPosition.TopRight
            ? System.Windows.VerticalAlignment.Top
            : System.Windows.VerticalAlignment.Bottom;
        FrozenIndicator.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void OnClosed(EventArgs e)
    {
        FrozenImage.Source = null;
        base.OnClosed(e);
    }
}
