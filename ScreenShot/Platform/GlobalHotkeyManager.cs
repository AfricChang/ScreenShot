using Avalonia.Input;
using ScreenShot.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScreenShot.Platform;

public class GlobalHotkeyManager
{
    private const int WM_HOTKEY = 0x0312;
    private readonly IntPtr hwnd;
    private readonly Dictionary<int, Action> hotkeyActions = new();
    private int hotkeyId = 0;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public GlobalHotkeyManager(IntPtr hwnd)
    {
        this.hwnd = hwnd;
    }

    public bool RegisterHotkey(HotkeyConfig config, Action callback)
    {
        if (hwnd == IntPtr.Zero || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        uint modifiers = 0;
        if (config.Modifiers.HasFlag(KeyModifiers.Alt)) modifiers |= 0x0001;
        if (config.Modifiers.HasFlag(KeyModifiers.Control)) modifiers |= 0x0002;
        if (config.Modifiers.HasFlag(KeyModifiers.Shift)) modifiers |= 0x0004;
        if (config.Modifiers.HasFlag(KeyModifiers.Meta)) modifiers |= 0x0008;

        var vk = (uint)config.Key;
        var id = ++hotkeyId;

        if (RegisterHotKey(hwnd, id, modifiers, vk))
        {
            hotkeyActions[id] = callback;
            return true;
        }

        return false;
    }

    public void UnregisterAll()
    {
        foreach (var id in hotkeyActions.Keys)
        {
            UnregisterHotKey(hwnd, id);
        }
        hotkeyActions.Clear();
    }

    public void HandleMessage(IntPtr wParam)
    {
        var id = wParam.ToInt32();
        if (hotkeyActions.TryGetValue(id, out var callback))
        {
            callback?.Invoke();
        }
    }
}
