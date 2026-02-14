using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using System;
using System.IO;
using System.Text.Json;

namespace CustomKey.Common;

public static class SettingsReader
{
    public static event Action? SettingsChanged;
    
    public static void LoadSettings()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (File.Exists(jsonFilePath))
        {
            string jsonString = File.ReadAllText(jsonFilePath);
            var settings = JsonSerializer.Deserialize<Settings>(jsonString);

            if (settings != null)
            {
                ThemeValue = settings.Theme;
                CustomBgEnabled = settings.CustomBg;
                BackgroundPath = settings.BgPath;
                CustomBackgroundPath = settings.CustomBgPath;
                CustomBackgroundPath2 = settings.CustomBgPath2;
                LanguageCode = settings.Language;
            }
        }
    }
    
    public static void ApplyTheme(string theme)
    {
        // Ensure FluentAvaloniaTheme is added to Application.Current.Styles
        if (!(Application.Current?.Styles[0] is FluentAvaloniaTheme faTheme))
        {
            faTheme = new FluentAvaloniaTheme();
            Application.Current?.Styles.Insert(0, faTheme);
        }

        var newTheme = GetThemeVariant(theme);
        Application.Current?.RequestedThemeVariant = newTheme;
        faTheme.PreferSystemTheme = false;
    }
    
    private static ThemeVariant? GetThemeVariant(string value)
    {
        switch (value)
        {
            case "Light":
                return ThemeVariant.Light;
            case "Dark":
                return ThemeVariant.Dark;
            default:
                return null;
        }
    }
    
    public static void SaveSettings()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        var settings = new Settings
        {
            Theme = ThemeValue,
            CustomBg = CustomBgEnabled,
            BgPath = BackgroundPath,
            CustomBgPath = CustomBackgroundPath,
            CustomBgPath2 = CustomBackgroundPath2,
            Language = LanguageCode
        };
        string jsonString = JsonSerializer.Serialize(settings);
        File.WriteAllText(jsonFilePath, jsonString);
        SettingsChanged?.Invoke();
    }
        
    private class Settings
    {
        public string Theme { get; set; } = "";
        public bool CustomBg { get; set; }
        public string BgPath { get; set; } = "";
        public string CustomBgPath { get; set; } = "";
        public string CustomBgPath2 { get; set; } = "";
        public string Language { get; set; } = "";
    }
    
    public static string ThemeValue = "System";
    public static bool CustomBgEnabled;
    public static string BackgroundPath = "avares://CustomKey/Assets/background/gradient.jpg";
    public static string CustomBackgroundPath = "";
    public static string CustomBackgroundPath2 = "";
    public static string LanguageCode = "English";
}