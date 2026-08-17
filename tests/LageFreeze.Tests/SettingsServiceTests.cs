using LageFreeze.Models;
using LageFreeze.Services;

namespace LageFreeze.Tests;

[TestClass]
public sealed class SettingsServiceTests
{
    private string _testDirectory = null!;

    [TestInitialize]
    public void Initialize()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "LageFreeze.Tests",
            Guid.NewGuid().ToString("N"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task SaveAndLoadAsync_RoundTripsUserConfiguration()
    {
        var path = Path.Combine(_testDirectory, "settings.json");
        using var service = new SettingsService(path);
        var expected = new AppSettings
        {
            StartWithWindows = true,
            StartMinimized = true,
            UseSystemTray = false,
            DefaultDrawingMode = DrawingMode.StronglyDimmed,
            ShowFrozenIndicator = false,
            FrozenIndicatorPosition = FrozenIndicatorPosition.BottomLeft,
            ScreenshotFolder = "C:\\Screenshots",
            ToggleFreezeHotkey = new HotkeySetting
            {
                Key = HotkeyKey.F11,
                Modifiers = GlobalHotkeyModifiers.Control,
            },
        };

        await service.SaveAsync(expected);
        var actual = await service.LoadAsync();

        Assert.IsTrue(actual.StartWithWindows);
        Assert.IsTrue(actual.StartMinimized);
        Assert.IsFalse(actual.UseSystemTray);
        Assert.AreEqual(DrawingMode.StronglyDimmed, actual.DefaultDrawingMode);
        Assert.IsFalse(actual.ShowFrozenIndicator);
        Assert.AreEqual(FrozenIndicatorPosition.BottomLeft, actual.FrozenIndicatorPosition);
        Assert.AreEqual("C:\\Screenshots", actual.ScreenshotFolder);
        Assert.AreEqual(HotkeyKey.F11, actual.ToggleFreezeHotkey.Key);
        Assert.AreEqual(GlobalHotkeyModifiers.Control, actual.ToggleFreezeHotkey.Modifiers);
    }

    [TestMethod]
    public async Task LoadAsync_MalformedJson_ReturnsDefaults()
    {
        Directory.CreateDirectory(_testDirectory);
        var path = Path.Combine(_testDirectory, "settings.json");
        await File.WriteAllTextAsync(path, "{ invalid json");
        using var service = new SettingsService(path);

        var settings = await service.LoadAsync();

        Assert.AreEqual(HotkeyKey.F9, settings.ToggleFreezeHotkey.Key);
        Assert.AreEqual(HotkeyKey.F10, settings.RefreshHotkey.Key);
        Assert.IsTrue(settings.ShowFrozenIndicator);
        Assert.AreEqual(FrozenIndicatorPosition.TopRight, settings.FrozenIndicatorPosition);
    }

    [TestMethod]
    public async Task LoadAsync_SettingsWithoutIndicatorFields_UsesNewDefaults()
    {
        Directory.CreateDirectory(_testDirectory);
        var path = Path.Combine(_testDirectory, "settings.json");
        await File.WriteAllTextAsync(path, "{ \"schemaVersion\": 1 }");
        using var service = new SettingsService(path);

        var settings = await service.LoadAsync();

        Assert.IsTrue(settings.ShowFrozenIndicator);
        Assert.AreEqual(FrozenIndicatorPosition.TopRight, settings.FrozenIndicatorPosition);
    }

    [TestMethod]
    public async Task SaveAndLoadAsync_InvalidIndicatorPosition_NormalizesToTopRight()
    {
        var path = Path.Combine(_testDirectory, "settings.json");
        using var service = new SettingsService(path);
        var settings = new AppSettings
        {
            FrozenIndicatorPosition = (FrozenIndicatorPosition)999,
        };

        await service.SaveAsync(settings);
        var actual = await service.LoadAsync();

        Assert.AreEqual(FrozenIndicatorPosition.TopRight, actual.FrozenIndicatorPosition);
    }
}
