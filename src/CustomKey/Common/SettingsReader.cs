using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using System;
using System.IO;
using System.Text.Json;

namespace CustomKey.Common;

public static class SettingsReader
{
    public static event Action SettingsChanged;
    
    public static void LoadSettings()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (File.Exists(jsonFilePath))
        {
            string jsonString = File.ReadAllText(jsonFilePath);
            var settings = JsonSerializer.Deserialize<Settings>(jsonString);

            if (settings != null)
            {
                _currentAppTheme = settings.theme;
                _customBg = settings.custombg;
                _Bgpath = settings.bgpath;
                _customBgpath = settings.custombgpath;
                _customBgpath2 = settings.custombgpath2;
                _language = settings.language;
                _autoUpdate = settings.autoupdate;
            }
        }
    }
    
    public static void ApplyTheme(string theme)
    {
        // Ensure FluentAvaloniaTheme is added to Application.Current.Styles
        if (!(Application.Current.Styles[0] is FluentAvaloniaTheme faTheme))
        {
            faTheme = new FluentAvaloniaTheme();
            Application.Current.Styles.Insert(0, faTheme);
        }

        var newTheme = GetThemeVariant(theme);
        if (newTheme != null)
        {
            Application.Current.RequestedThemeVariant = newTheme;
            faTheme.PreferSystemTheme = false;
        }
        else
        {
            faTheme.PreferSystemTheme = true;
        }
    }
    
    private static ThemeVariant GetThemeVariant(string value)
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
            theme = _currentAppTheme,
            custombg = _customBg,
            bgpath = _Bgpath,
            custombgpath = _customBgpath,
            custombgpath2 = _customBgpath2,
            language = _language,
            autoupdate = _autoUpdate
        };
        string jsonString = JsonSerializer.Serialize(settings);
        File.WriteAllText(jsonFilePath, jsonString);
        SettingsChanged?.Invoke();
    }
        
    private class Settings
    {
        public string theme { get; set; }     
        public bool custombg { get; set; }
        public string bgpath { get; set; }
        public string custombgpath { get; set; }
        public string custombgpath2 { get; set; }
        public string language { get; set; }
        public bool autoupdate { get; set; }
    }
    
    public static string _currentAppTheme;
    public static bool _customBg;
    public static string _Bgpath;
    public static string _customBgpath;
    public static string _customBgpath2;
    public static string _language;
    public static bool _autoUpdate;
}