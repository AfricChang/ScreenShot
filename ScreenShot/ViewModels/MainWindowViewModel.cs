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

[SupportedOSPlatform("windows6.1")]
public partial class MainWindowViewModel : ViewModelBase
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    private const uint GMEM_MOVEABLE = 0x0002;
    private const uint CF_BITMAP = 2;

    private readonly IScreenCapture screenCapture;
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
                // 释放旧的图片
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

    public MainWindowViewModel()
    {
        screenCapture = new WindowsScreenCapture();
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
            // 暂时最小化主窗口
            mainWindow.WindowState = WindowState.Minimized;
            await Task.Delay(200); // 等待窗口最小化动画完成
            
            try
            {
                var captureWindow = new CaptureWindow();
                if (await captureWindow.ShowDialog<bool>(mainWindow))
                {
                    var bounds = captureWindow.SelectedBounds;
                    var screenshot = await screenCapture.CaptureScreenAsync(
                        bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    
                    if (screenshot != null)
                    {
                        // 在UI线程上设置图片
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
                // 恢复主窗口状态
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
                await using var stream = await file.OpenWriteAsync();
                image.Save(stream);
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
        if (image == null) return;

        try
        {
            // 将 Avalonia 位图转换为 GDI+ 位图
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream);
            memoryStream.Position = 0;

            using var gdiBitmap = new SysDraw.Bitmap(memoryStream);
            IntPtr hBitmap = gdiBitmap.GetHbitmap();

            try
            {
                // 打开剪贴板并设置数据
                if (!OpenClipboard(IntPtr.Zero))
                    throw new Exception("无法打开剪贴板");

                try
                {
                    EmptyClipboard();
                    if (SetClipboardData(CF_BITMAP, hBitmap) == IntPtr.Zero)
                        throw new Exception("无法设置剪贴板数据");
                }
                finally
                {
                    CloseClipboard();
                }

                // 成功设置剪贴板数据后，Windows 会接管位图，不要删除
                hBitmap = IntPtr.Zero;
            }
            finally
            {
                // 如果出错，删除位图
                if (hBitmap != IntPtr.Zero)
                    SysDraw.Bitmap.FromHbitmap(hBitmap).Dispose();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"复制到剪贴板时出错: {ex}");
        }
    }
}
