using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Input;

namespace CustomKey.Common;

public static class LayoutInit
{
    // Dictionnaire statique contenant les données du layout
    public static Dictionary<string, (string keyChar, string shiftChar, string keyId)> KeyVal { get; private set; } = new();

    public static void LoadLayoutFromFile(string jsonFileName)
    {
        try
        {
            string exePath = AppContext.BaseDirectory;
            string layoutPath = Path.Combine(exePath, "Key", jsonFileName);

            if (!File.Exists(layoutPath))
            {
                Console.WriteLine($"Fichier introuvable : {layoutPath}");
                return;
            }

            string json = File.ReadAllText(layoutPath);
            var rawDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            // Mettez à jour KeyVal en place pour éviter de remplacer complètement l'objet
            KeyVal.Clear();

            for (int i = 1; i <= 47; i++)
            {
                string keyName = $"Key{i}";
                rawDict.TryGetValue($"{keyName}_Char", out string charVal);
                rawDict.TryGetValue($"Shift{i}_Char", out string shiftVal);
                rawDict.TryGetValue($"{keyName}_ID", out string idVal);

                // If no value : set to ASCII code "\r", which result to do a toUpperCase of the charVal when shift/caps is on
                // For exemple if you set Key1_Char to "m" and don't set Shift1_Char in the json when shift is enable it will show "M" and not "\r" or ""
                // In order to don't do a ToUpperCase you should set a value to "" for Shift_Char in json layout
                KeyVal[keyName] = (charVal ?? "", shiftVal ?? "\r", idVal ?? "");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when loading layout : {e.Message}");
        }
    }
    
    public static string GetChar(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return "";

        if (key.EndsWith("Shift"))
        {
            if (!Keyboard.IsShift)
            {
                var baseKey = key.Replace("Shift", "");
                if (KeyVal.TryGetValue(baseKey, out var keyData)) return keyData.shiftChar;
            }
            return "";
        }

        if (!KeyVal.TryGetValue(key, out var keyInfo)) return "";

        if (Keyboard.IsShift)
        {
            if (keyInfo.shiftChar == "\r") return keyInfo.keyChar.ToUpper();
            return keyInfo.shiftChar;
        }

        return keyInfo.keyChar;
    }
}