using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenShot.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ScreenShot.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly Window window;
    private readonly Settings originalSettings;

    [ObservableProperty]
    private CultureInfo selectedLanguage;

    public ObservableCollection<CultureInfo> Languages { get; }

    public SettingsViewModel(Window window)
    {
        this.window = window;
        originalSettings = SettingsService.Instance.CurrentSettings;

        Languages = new ObservableCollection<CultureInfo>(
            LocalizationService.Instance.SupportedLanguages);

        selectedLanguage = Languages.First(l => l.Name == originalSettings.Language);
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new Settings
        {
            Language = SelectedLanguage.Name,
            Theme = originalSettings.Theme
        };

        SettingsService.Instance.SaveSettings(settings);

        if (settings.Language != originalSettings.Language)
        {
            LocalizationService.Instance.SetLanguage(settings.Language);
        }

        window.Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        window.Close(false);
    }
}
