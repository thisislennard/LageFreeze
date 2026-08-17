using System.Windows;
using System.Windows.Controls;
using LageFreeze.Models;
using LageFreeze.ViewModels;
using LageFreeze.Views;

namespace LageFreeze.Tests;

[TestClass]
public sealed class FrozenIndicatorTests
{
    [TestMethod]
    public void AppSettings_DefaultsToVisibleTopRightIndicator()
    {
        var settings = AppSettings.CreateDefault();

        Assert.IsTrue(settings.ShowFrozenIndicator);
        Assert.AreEqual(FrozenIndicatorPosition.TopRight, settings.FrozenIndicatorPosition);
    }

    [TestMethod]
    public void SettingsViewModel_ExposesAllPositionsAndSavesSelection()
    {
        var viewModel = CreateViewModel();
        AppSettings? savedSettings = null;
        viewModel.Saved += (_, eventArgs) => savedSettings = eventArgs.Settings;

        viewModel.ShowFrozenIndicator = false;
        viewModel.FrozenIndicatorPosition = FrozenIndicatorPosition.BottomLeft;
        viewModel.SaveCommand.Execute(null);

        Assert.AreEqual(4, viewModel.FrozenIndicatorPositions.Count);
        CollectionAssert.AreEquivalent(
            Enum.GetValues<FrozenIndicatorPosition>(),
            viewModel.FrozenIndicatorPositions.Select(option => option.Value).ToArray());
        Assert.IsNotNull(savedSettings);
        Assert.IsFalse(savedSettings.ShowFrozenIndicator);
        Assert.AreEqual(FrozenIndicatorPosition.BottomLeft, savedSettings.FrozenIndicatorPosition);
    }

    [TestMethod]
    public void SettingsViewModel_InvalidPositionDoesNotSave()
    {
        var viewModel = CreateViewModel();
        var saved = false;
        viewModel.Saved += (_, _) => saved = true;
        viewModel.FrozenIndicatorPosition = (FrozenIndicatorPosition)999;

        viewModel.SaveCommand.Execute(null);

        Assert.IsFalse(saved);
        StringAssert.Contains(viewModel.ValidationMessage, "Position");
    }

    [STATestMethod]
    [DataRow(FrozenIndicatorPosition.TopLeft, HorizontalAlignment.Left, VerticalAlignment.Top)]
    [DataRow(FrozenIndicatorPosition.TopRight, HorizontalAlignment.Right, VerticalAlignment.Top)]
    [DataRow(FrozenIndicatorPosition.BottomLeft, HorizontalAlignment.Left, VerticalAlignment.Bottom)]
    [DataRow(FrozenIndicatorPosition.BottomRight, HorizontalAlignment.Right, VerticalAlignment.Bottom)]
    public void FreezeWindow_SetFrozenIndicator_AlignsRequestedCorner(
        FrozenIndicatorPosition position,
        HorizontalAlignment expectedHorizontal,
        VerticalAlignment expectedVertical)
    {
        using var scope = new WindowScope(new FreezeWindow());
        var indicator = (Border)scope.Value.FindName("FrozenIndicator");

        scope.Value.SetFrozenIndicator(isVisible: true, position);

        Assert.AreEqual(Visibility.Visible, indicator.Visibility);
        Assert.AreEqual(expectedHorizontal, indicator.HorizontalAlignment);
        Assert.AreEqual(expectedVertical, indicator.VerticalAlignment);
        Assert.IsFalse(indicator.IsHitTestVisible);
    }

    [STATestMethod]
    public void FreezeWindow_SetFrozenIndicator_HidesMarker()
    {
        using var scope = new WindowScope(new FreezeWindow());
        var indicator = (Border)scope.Value.FindName("FrozenIndicator");

        scope.Value.SetFrozenIndicator(false, FrozenIndicatorPosition.TopRight);

        Assert.AreEqual(Visibility.Collapsed, indicator.Visibility);
    }

    private static SettingsViewModel CreateViewModel()
    {
        return new SettingsViewModel(
            AppSettings.CreateDefault(),
            Array.Empty<MonitorInfo>(),
            selectedMonitor: null,
            browseFolder: _ => null);
    }

    private sealed class WindowScope(FreezeWindow value) : IDisposable
    {
        public FreezeWindow Value { get; } = value;

        public void Dispose()
        {
            Value.Close();
        }
    }
}
