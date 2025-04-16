using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

                KeyVal[keyName] = (charVal ?? "", shiftVal ?? "", idVal ?? "");
            }

            Console.WriteLine($"Layout chargé : {jsonFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement du layout : {ex.Message}");
        }
    }

    public static string GetChar(string keyName, bool isShift)
    {
        if (KeyVal.TryGetValue(keyName, out var val))
            return isShift ? val.shiftChar : val.keyChar;
        return string.Empty;
    }
}