using LageFreeze.Models;

namespace LageFreeze.Services;

public static class HotkeyTextConverter
{
    public static string Format(HotkeySetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        var parts = new List<string>();
        AddModifier(parts, setting.Modifiers, GlobalHotkeyModifiers.Control, "Strg");
        AddModifier(parts, setting.Modifiers, GlobalHotkeyModifiers.Alt, "Alt");
        AddModifier(parts, setting.Modifiers, GlobalHotkeyModifiers.Shift, "Umschalt");
        AddModifier(parts, setting.Modifiers, GlobalHotkeyModifiers.Windows, "Windows");
        parts.Add(setting.Key.ToString());
        return string.Join('+', parts);
    }

    public static bool TryParse(
        string? text,
        bool enabled,
        out HotkeySetting setting,
        out string error)
    {
        setting = new HotkeySetting { Enabled = enabled };
        error = string.Empty;

        if (!enabled)
        {
            setting.Key = HotkeyKey.F9;
            return true;
        }

        var tokens = (text ?? string.Empty)
            .Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            error = "Bitte ein Tastenkürzel wie F9 oder Strg+F9 eingeben.";
            return false;
        }

        var modifiers = GlobalHotkeyModifiers.None;
        HotkeyKey? key = null;

        foreach (var token in tokens)
        {
            if (TryParseModifier(token, out var modifier))
            {
                if ((modifiers & modifier) != 0)
                {
                    error = $"Die Taste „{token}“ wurde doppelt angegeben.";
                    return false;
                }

                modifiers |= modifier;
                continue;
            }

            if (Enum.TryParse<HotkeyKey>(token, ignoreCase: true, out var parsedKey))
            {
                if (key is not null)
                {
                    error = "Ein Tastenkürzel darf nur eine Funktionstaste enthalten.";
                    return false;
                }

                key = parsedKey;
                continue;
            }

            error = $"„{token}“ ist nicht gültig. Erlaubt sind F1 bis F24 und optionale Zusatztasten.";
            return false;
        }

        if (key is null)
        {
            error = "Das Tastenkürzel benötigt eine Funktionstaste zwischen F1 und F24.";
            return false;
        }

        setting = new HotkeySetting
        {
            Enabled = true,
            Key = key.Value,
            Modifiers = modifiers,
        };
        return true;
    }

    private static void AddModifier(
        ICollection<string> parts,
        GlobalHotkeyModifiers value,
        GlobalHotkeyModifiers modifier,
        string text)
    {
        if ((value & modifier) != 0)
        {
            parts.Add(text);
        }
    }

    private static bool TryParseModifier(string token, out GlobalHotkeyModifiers modifier)
    {
        modifier = token.ToUpperInvariant() switch
        {
            "STRG" or "CTRL" or "CONTROL" => GlobalHotkeyModifiers.Control,
            "ALT" => GlobalHotkeyModifiers.Alt,
            "UMSCHALT" or "SHIFT" => GlobalHotkeyModifiers.Shift,
            "WIN" or "WINDOWS" => GlobalHotkeyModifiers.Windows,
            _ => GlobalHotkeyModifiers.None,
        };
        return modifier != GlobalHotkeyModifiers.None;
    }
}
