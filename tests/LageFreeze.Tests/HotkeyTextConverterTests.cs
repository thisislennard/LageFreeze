using LageFreeze.Models;
using LageFreeze.Services;

namespace LageFreeze.Tests;

[TestClass]
public sealed class HotkeyTextConverterTests
{
    [TestMethod]
    public void TryParse_GermanModifiersAndFunctionKey_ReturnsSetting()
    {
        var success = HotkeyTextConverter.TryParse(
            "Strg+Umschalt+F10",
            enabled: true,
            out var setting,
            out var error);

        Assert.IsTrue(success, error);
        Assert.AreEqual(HotkeyKey.F10, setting.Key);
        Assert.AreEqual(
            GlobalHotkeyModifiers.Control | GlobalHotkeyModifiers.Shift,
            setting.Modifiers);
    }

    [TestMethod]
    public void TryParse_UnknownKey_ReturnsUsefulError()
    {
        var success = HotkeyTextConverter.TryParse(
            "Strg+Leertaste",
            enabled: true,
            out _,
            out var error);

        Assert.IsFalse(success);
        StringAssert.Contains(error, "nicht gültig");
    }

    [TestMethod]
    public void Format_UsesStableOrder()
    {
        var setting = new HotkeySetting
        {
            Key = HotkeyKey.F9,
            Modifiers = GlobalHotkeyModifiers.Alt | GlobalHotkeyModifiers.Control,
        };

        Assert.AreEqual("Strg+Alt+F9", HotkeyTextConverter.Format(setting));
    }
}
