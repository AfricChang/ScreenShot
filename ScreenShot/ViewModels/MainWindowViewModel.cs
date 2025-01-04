using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenShot.Models;
using ScreenShot.Platform;
using ScreenShot.Services;
using ScreenShot.Views;
using System;
using System.Threading.Tasks;

namespace ScreenShot.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IScreenCapture screenCapture;

    [ObservableProperty]
    private bool isCapturing;

    [ObservableProperty]
    private Bitmap? capturedImage;

    [ObservableProperty]
    private HotkeyConfig screenshotHotkey;

    public MainWindowViewModel()
    {
        screenCapture = ScreenCaptureFactory.Create();
        screenshotHotkey = new HotkeyConfig 
        { 
            Key = Key.S, 
            Modifiers = KeyModifiers.Control | KeyModifiers.Shift 
        };
        RegisterHotkey();

        // 订阅设置变更事件
        SettingsService.Instance.SettingsChanged += SettingsService_SettingsChanged;
    }

    private void SettingsService_SettingsChanged(object? sender, EventArgs e)
    {
        var settings = SettingsService.Instance.CurrentSettings;
        LocalizationService.Instance.SetLanguage(settings.Language);
    }

    private void RegisterHotkey()
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            var hotkeyManager = new GlobalHotkeyManager(mainWindow.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero);
            hotkeyManager.RegisterHotkey(ScreenshotHotkey, async () =>
            {
                await StartScreenshotAsync();
            });
        }
    }

    [RelayCommand]
    public async Task StartScreenshotAsync()
    {
        IsCapturing = true;
        try
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;

            var captureWindow = new CaptureWindow();
            if (await captureWindow.ShowDialog<bool>(mainWindow))
            {
                var bounds = captureWindow.SelectedBounds;
                var screenshot = await screenCapture.CaptureScreenAsync(
                    (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
                
                if (screenshot != null)
                {
                    CapturedImage = screenshot;
                }
            }
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        var settingsWindow = new SettingsWindow
        {
            DataContext = new SettingsViewModel(mainWindow)
        };

        await settingsWindow.ShowDialog(mainWindow);
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
