using LageFreeze.Models;
using LageFreeze.Services;

namespace LageFreeze.Tests;

[TestClass]
public sealed class MonitorMatcherTests
{
    [TestMethod]
    public void FindBestMatch_PrefersStableIdentityAfterDisplayNumberChanged()
    {
        var target = CreateMonitor(3, "MONITOR-A", "\\\\.\\DISPLAY3", -1920, 0);
        var selection = MonitorSelection.FromMonitor(
            CreateMonitor(2, "MONITOR-A", "\\\\.\\DISPLAY2", 1920, 0));

        var result = MonitorMatcher.FindBestMatch(
            selection,
            [CreateMonitor(1, "MONITOR-B", "\\\\.\\DISPLAY1", 0, 0), target]);

        Assert.AreSame(target, result);
    }

    [TestMethod]
    public void FindBestMatch_DisconnectedMonitor_DoesNotSelectUnrelatedDisplay()
    {
        var selection = MonitorSelection.FromMonitor(
            CreateMonitor(2, "MONITOR-A", "\\\\.\\DISPLAY2", 1920, 0));

        var result = MonitorMatcher.FindBestMatch(
            selection,
            [CreateMonitor(1, "MONITOR-B", "\\\\.\\DISPLAY1", 0, 0)]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void FindBestMatch_UsesDeviceAndGeometryFallback()
    {
        var expected = CreateMonitor(2, "Updated name", "\\\\.\\DISPLAY2", -1080, 0);
        var selection = new MonitorSelection
        {
            StableId = "old-stable-id",
            DeviceName = "\\\\.\\DISPLAY2",
            DisplayName = "Old name",
            LastKnownBounds = new ScreenBounds(-1080, 0, 1080, 1920),
        };

        Assert.AreSame(expected, MonitorMatcher.FindBestMatch(selection, [expected]));
    }

    private static MonitorInfo CreateMonitor(
        int number,
        string stableId,
        string deviceName,
        int left,
        int top)
    {
        var bounds = new ScreenBounds(left, top, 1920, 1080);
        return new MonitorInfo
        {
            DisplayNumber = number,
            DisplayName = $"Monitor {number}",
            DeviceName = deviceName,
            StableId = stableId,
            Bounds = bounds,
            WorkArea = bounds,
            IsPrimary = number == 1,
            DpiX = 96,
            DpiY = 96,
        };
    }
}
