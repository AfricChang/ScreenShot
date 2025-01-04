using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenShot.Models;
using ScreenShot.Platform;
using ScreenShot.Services;
using ScreenShot.Views;
using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using SysDraw = System.Drawing;

namespace ScreenShot.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IScreenshotService screenshotService;
    private Bitmap? _capturedImage;
    
    [ObservableProperty]
    private bool isCapturing;

    public Bitmap? CapturedImage
    {
        get => _capturedImage;
        set
        {
            if (_capturedImage != value)
            {
                if (_capturedImage != null)
                {
                    var oldBitmap = _capturedImage;
                    _capturedImage = null;
                    oldBitmap.Dispose();
                }

                _capturedImage = value;
                OnPropertyChanged(nameof(CapturedImage));
            }
        }
    }

    public MainWindowViewModel(IScreenshotService screenshotService)
    {
        this.screenshotService = screenshotService;
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    [RelayCommand]
    public async Task StartScreenshotAsync()
    {
        IsCapturing = true;
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        try
        {
            mainWindow.WindowState = WindowState.Minimized;
            await Task.Delay(200);
            
            try
            {
                var captureWindow = new CaptureWindow();
                if (await captureWindow.ShowDialog<bool>(mainWindow))
                {
                    var bounds = captureWindow.SelectedBounds;
                    var screenshot = await screenshotService.CaptureScreenAsync(
                        (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
                    
                    if (screenshot != null)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            try
                            {
                                CapturedImage = screenshot;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"设置截图时出错: {ex}");
                                CapturedImage = null;
                            }
                        });
                    }
                }
            }
            finally
            {
                mainWindow.WindowState = WindowState.Normal;
            }
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    public async Task SaveImageAsync()
    {
        var image = CapturedImage;
        if (image == null || GetMainWindow() == null) return;

        try
        {
            var options = new FilePickerSaveOptions
            {
                Title = "保存截图",
                DefaultExtension = ".png",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("PNG 图片") { Patterns = new[] { "*.png" } }
                }
            };

            var file = await GetMainWindow()!.StorageProvider.SaveFilePickerAsync(options);
            if (file != null)
            {
                await screenshotService.SaveImageAsync(image, file.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存图片时出错: {ex}");
        }
    }

    [RelayCommand]
    public async Task CopyImageAsync()
    {
        var image = CapturedImage;
        if (image == null || GetMainWindow() == null) return;

        try
        {
            await screenshotService.CopyImageToClipboardAsync(image);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"复制到剪贴板时出错: {ex}");
        }
    }
}
