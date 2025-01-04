using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

namespace ScreenShot.Services;

public class LocalizationService
{
    private static LocalizationService? instance;
    private CultureInfo currentCulture;

    public static LocalizationService Instance => instance ??= new LocalizationService();

    public event EventHandler? LanguageChanged;

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set
        {
            if (currentCulture.Name != value.Name)
            {
                currentCulture = value;
                OnLanguageChanged();
            }
        }
    }

    public IReadOnlyList<CultureInfo> SupportedLanguages { get; }

    private LocalizationService()
    {
        currentCulture = CultureInfo.CurrentUICulture;
        SupportedLanguages = new List<CultureInfo>
        {
            new("en-US"),
            new("zh-CN")
        };

        // 加载当前语言的资源
        LoadResources(currentCulture);
    }

    private void LoadResources(CultureInfo culture)
    {
        if (Application.Current == null) return;

        Application.Current.Resources.MergedDictionaries.Clear();
        var resourcePath = $"avares://ScreenShot/Resources/Strings.{culture.Name}.axaml";
        
        try
        {
            var resourceDict = AvaloniaXamlLoader.Load(new Uri(resourcePath)) as ResourceDictionary;
            if (resourceDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load resources for culture {culture.Name}: {ex.Message}");
            // 如果加载失败，使用默认语言
            if (culture.Name != "en-US")
            {
                LoadResources(new CultureInfo("en-US"));
            }
        }
    }

    public void SetLanguage(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CurrentCulture = culture;
        LoadResources(culture);
    }

    private void OnLanguageChanged()
    {
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key)
    {
        if (Application.Current?.Resources.TryGetResource(key, null, out object? value) == true)
        {
            return value?.ToString() ?? key;
        }
        return key;
    }
}
