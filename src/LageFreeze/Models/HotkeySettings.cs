namespace LageFreeze.Models;

[Flags]
public enum GlobalHotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
}

/// <summary>Function keys represented by their Win32 virtual-key values.</summary>
public enum HotkeyKey : uint
{
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,
    F13 = 0x7C,
    F14 = 0x7D,
    F15 = 0x7E,
    F16 = 0x7F,
    F17 = 0x80,
    F18 = 0x81,
    F19 = 0x82,
    F20 = 0x83,
    F21 = 0x84,
    F22 = 0x85,
    F23 = 0x86,
    F24 = 0x87,
}

public enum HotkeyAction
{
    ToggleFreeze = 1,
    Refresh = 2,
}

public sealed class HotkeySetting
{
    public bool Enabled { get; set; } = true;

    public HotkeyKey Key { get; set; }

    public GlobalHotkeyModifiers Modifiers { get; set; }

    public static HotkeySetting CreateDefaultToggle()
    {
        return new HotkeySetting { Key = HotkeyKey.F9 };
    }

    public static HotkeySetting CreateDefaultRefresh()
    {
        return new HotkeySetting { Key = HotkeyKey.F10 };
    }
}
