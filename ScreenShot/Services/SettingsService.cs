using System;
using System.IO;
using System.Text.Json;

namespace ScreenShot.Services;

public class SettingsService
{
    private static SettingsService? instance;
    private readonly string settingsPath;
    private Settings currentSettings;

    public static SettingsService Instance => instance ??= new SettingsService();

    public event EventHandler? SettingsChanged;

    public Settings CurrentSettings
    {
        get => currentSettings;
        private set
        {
            currentSettings = value;
            OnSettingsChanged();
        }
    }

    private SettingsService()
    {
        settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ScreenShot",
            "settings.json");

        currentSettings = LoadSettings();
    }

    private Settings LoadSettings()
    {
        try
        {
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch (Exception)
        {
            // 如果加载失败，使用默认设置
        }

        return new Settings();
    }

    public void SaveSettings(Settings settings)
    {
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsPath, json);
        CurrentSettings = settings;
    }

    private void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class Settings
{
    public string Language { get; set; } = "en-US";
    public string Theme { get; set; } = "Light";
}
