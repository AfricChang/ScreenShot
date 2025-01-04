using System;
using Avalonia.Input;

namespace ScreenShot.Models;

public class HotkeyConfig
{
    public Key Key { get; set; }
    public KeyModifiers Modifiers { get; set; }
    public string Action { get; set; } = string.Empty;

    public override string ToString()
    {
        var modifierString = Modifiers != KeyModifiers.None ? $"{Modifiers}+" : "";
        return $"{modifierString}{Key}";
    }

    public static HotkeyConfig Parse(string hotkeyString)
    {
        var parts = hotkeyString.Split('+');
        var config = new HotkeyConfig();
        
        if (parts.Length > 1)
        {
            // Parse modifiers
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (Enum.TryParse<KeyModifiers>(parts[i].Trim(), true, out var modifier))
                {
                    config.Modifiers |= modifier;
                }
            }
            
            // Parse key
            if (Enum.TryParse<Key>(parts[^1].Trim(), true, out var key))
            {
                config.Key = key;
            }
        }
        else if (parts.Length == 1)
        {
            if (Enum.TryParse<Key>(parts[0].Trim(), true, out var key))
            {
                config.Key = key;
            }
        }

        return config;
    }
}
