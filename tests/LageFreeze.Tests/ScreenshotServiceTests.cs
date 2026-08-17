using LageFreeze.Services;

namespace LageFreeze.Tests;

[TestClass]
public sealed class ScreenshotServiceTests
{
    [TestMethod]
    public void BuildFileName_UsesReadableLocalTimestampAndMonitorNumber()
    {
        var timestamp = new DateTimeOffset(2026, 8, 17, 8, 42, 15, TimeSpan.FromHours(2));

        var result = ScreenshotService.BuildFileName(timestamp, displayNumber: 2);

        Assert.AreEqual("LageFreeze-2026-08-17-08-42-15-Monitor-2.png", result);
        Assert.IsFalse(result.Any(character => Path.GetInvalidFileNameChars().Contains(character)));
    }
}
