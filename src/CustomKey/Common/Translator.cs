using Avalonia.Platform;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CustomKey.Common;

public static class Translator
{
    public static string[] Languages = Array.Empty<string>();
    public static Dictionary<string, string> TradBinding = new();
    private static readonly Uri JsonTrad = new("avares://CustomKey/Assets/locales.json");

    private static Dictionary<string, Dictionary<string, string>> LoadLanguagesJson()
    {
        if (!AssetLoader.Exists(JsonTrad)) return new Dictionary<string, Dictionary<string, string>>();

        using Stream stream = AssetLoader.Open(JsonTrad);
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();

        var allTrad = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        
        return allTrad ?? new Dictionary<string, Dictionary<string, string>>();
    }

    /**
     * Loads available language names from locales.json translation file and stores them in the Languages array.
     */
    public static void LoadLanguages()
    {
        var list = new List<string>();
        foreach (var entry in LoadLanguagesJson())
        {
            list.Add(entry.Key);
        }

        Languages = list.ToArray();
        LoadSelectedLanguage();
    }
    
    /*
     * Update _tradBinding with the values of the selected language
     */
    public static void LoadSelectedLanguage()
    {
        if (LoadLanguagesJson().TryGetValue(SettingsReader.LanguageCode, out var selected)) TradBinding = new Dictionary<string, string>(selected);
    }
}