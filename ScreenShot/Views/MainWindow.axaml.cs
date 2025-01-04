using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using ScreenShot.Platform;
using ScreenShot.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace ScreenShot.Views;

public partial class MainWindow : Window
{
    private const int WM_HOTKEY = 0x0312;
    private readonly GlobalHotkeyManager hotkeyManager;

    public MainWindow()
    {
        InitializeComponent();
        hotkeyManager = new GlobalHotkeyManager(TryGetPlatformHandle()?.Handle ?? IntPtr.Zero);
        DataContextChanged += MainWindow_DataContextChanged;
    }

    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        hotkeyManager.UnregisterAll();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.ScreenshotHotkey))
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                hotkeyManager.UnregisterAll();
                hotkeyManager.RegisterHotkey(viewModel.ScreenshotHotkey, async () =>
                {
                    await viewModel.StartScreenshotAsync();
                });
            }
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.CapturedImage))
        {
            if (DataContext is MainWindowViewModel viewModel && viewModel.CapturedImage != null)
            {
                // 当有新的截图时，显示编辑窗口
                ShowEditWindow();
            }
        }
    }

    private void ShowEditWindow()
    {
        // TODO: 实现编辑窗口
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
}