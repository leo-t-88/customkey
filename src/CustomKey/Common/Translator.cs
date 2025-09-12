using Avalonia.Platform;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace CustomKey.Common;

public static class Translator
{
    public static string[] Languages = Array.Empty<string>();
    public static Dictionary<string, string> _tradBinding = new();
    private static Uri jsonTrad = new("avares://CustomKey/Assets/locales.json");

    private static Dictionary<string, Dictionary<string, string>> LoadLanguagesJSON()
    {
        if (!AssetLoader.Exists(jsonTrad)) return null;

        using Stream stream = AssetLoader.Open(jsonTrad);
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();

        var allTrad = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        if (allTrad == null) return null;
        
        return allTrad;
    }

    /**
     * Loads available language names from locales.json translation file and stores them in the Languages array.
     */
    public static void LoadLanguages()
    {
        var list = new List<string>();
        foreach (var entry in LoadLanguagesJSON())
        {
            list.Add(entry.Key);
        }

        Languages = list.ToArray();
        LoadSelectedLanguage();
    }
    
    public static void LoadSelectedLanguage()
    {
        if (LoadLanguagesJSON().TryGetValue(SettingsReader.LanguageCode, out var selected))
        {
            _tradBinding = selected.ToDictionary(
                kvp => $"Trad{kvp.Key}",  // Add Trad prefix
                kvp => kvp.Value
            );
        }
    }
}