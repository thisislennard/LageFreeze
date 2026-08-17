using LageFreeze.Models;

namespace LageFreeze.Tests;

[TestClass]
public sealed class ScreenBoundsTests
{
    [TestMethod]
    public void FromEdges_PreservesNegativeVirtualDesktopCoordinates()
    {
        var bounds = ScreenBounds.FromEdges(-3840, -2160, 0, 0);

        Assert.AreEqual(-3840, bounds.Left);
        Assert.AreEqual(-2160, bounds.Top);
        Assert.AreEqual(3840, bounds.Width);
        Assert.AreEqual(2160, bounds.Height);
    }

    [TestMethod]
    public void Contains_UsesExclusiveRightAndBottomEdges()
    {
        var bounds = new ScreenBounds(-100, -100, 200, 200);

        Assert.IsTrue(bounds.Contains(-100, -100));
        Assert.IsTrue(bounds.Contains(99, 99));
        Assert.IsFalse(bounds.Contains(100, 99));
        Assert.IsFalse(bounds.Contains(99, 100));
    }
}
