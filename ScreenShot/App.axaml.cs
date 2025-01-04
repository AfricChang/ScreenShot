using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ScreenShot.Platform;
using ScreenShot.ViewModels;
using ScreenShot.Views;
using System;
using System.Runtime.Versioning;

namespace ScreenShot;

[SupportedOSPlatform("windows6.1")]
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var screenshotService = new WindowsScreenshotService();
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(screenshotService)
                };

                desktop.MainWindow = mainWindow;
                desktop.MainWindow.Show();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"应用程序初始化时出错: {ex}");
            throw;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
