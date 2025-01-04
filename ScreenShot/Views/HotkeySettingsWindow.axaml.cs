using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ScreenShot.Models;
using System;

namespace ScreenShot.Views;

public partial class HotkeySettingsWindow : Window
{
    private HotkeyConfig hotkeyConfig = new();

    public HotkeyConfig HotkeyConfig => hotkeyConfig;

    public HotkeySettingsWindow()
    {
        InitializeComponent();

        var hotkeyTextBox = this.FindControl<TextBox>("HotkeyTextBox");
        if (hotkeyTextBox != null)
        {
            hotkeyTextBox.KeyDown += HotkeyTextBox_KeyDown;
            hotkeyTextBox.KeyUp += HotkeyTextBox_KeyUp;
        }

        var okButton = this.FindControl<Button>("OkButton");
        if (okButton != null)
        {
            okButton.Click += OkButton_Click;
        }

        var cancelButton = this.FindControl<Button>("CancelButton");
        if (cancelButton != null)
        {
            cancelButton.Click += CancelButton_Click;
        }
    }

    public HotkeySettingsWindow(HotkeyConfig existingHotkey)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        hotkeyConfig.Key = existingHotkey.Key;
        hotkeyConfig.Modifiers = existingHotkey.Modifiers;

        var currentHotkeyTextBlock = this.FindControl<TextBlock>("CurrentHotkeyText");
        if (currentHotkeyTextBlock != null)
        {
            currentHotkeyTextBlock.Text = existingHotkey.ToString();
        }

        var hotkeyTextBox = this.FindControl<TextBox>("HotkeyTextBox");
        if (hotkeyTextBox != null)
        {
            hotkeyTextBox.KeyDown += HotkeyTextBox_KeyDown;
            hotkeyTextBox.KeyUp += HotkeyTextBox_KeyUp;
        }

        var okButton = this.FindControl<Button>("OkButton");
        if (okButton != null)
        {
            okButton.Click += OkButton_Click;
        }

        var cancelButton = this.FindControl<Button>("CancelButton");
        if (cancelButton != null)
        {
            cancelButton.Click += CancelButton_Click;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var modifiers = KeyModifiers.None;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) modifiers |= KeyModifiers.Control;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) modifiers |= KeyModifiers.Alt;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) modifiers |= KeyModifiers.Shift;

        hotkeyConfig.Key = e.Key;
        hotkeyConfig.Modifiers = modifiers;

        var hotkeyText = GetHotkeyText(hotkeyConfig);
        var hotkeyTextBlock = this.FindControl<TextBlock>("HotkeyTextBlock");
        if (hotkeyTextBlock != null)
        {
            hotkeyTextBlock.Text = hotkeyText;
        }
    }

    private string GetHotkeyText(HotkeyConfig hotkey)
    {
        var text = "";

        if (hotkey.Modifiers.HasFlag(KeyModifiers.Control)) text += "Ctrl + ";
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Alt)) text += "Alt + ";
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Shift)) text += "Shift + ";

        text += hotkey.Key.ToString();

        return text;
    }

    private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;

        var modifiers = e.KeyModifiers;
        var key = e.Key;

        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        hotkeyConfig.Key = key;
        hotkeyConfig.Modifiers = modifiers;

        var hotkeyTextBox = sender as TextBox;
        if (hotkeyTextBox != null)
        {
            hotkeyTextBox.Text = $"{modifiers} + {key}";
        }
    }

    private void HotkeyTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
