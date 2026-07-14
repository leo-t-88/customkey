using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CustomKey.Common
{
    public static class LayoutLoader
    {
        public static string? CurrentLayoutJson { get; private set; }
        private static readonly Dictionary<string, string> LayoutFileMap = new();
        
        // Dictionnaire statique contenant les données du layout
        public static Dictionary<string, (string keyChar, string shiftChar, string keyId)> KeyVal { get; private set; } = new();
        
        // Retourne une liste des noms des layouts (propriété "Name" dans les fichiers JSON)
        public static Dictionary<string, string> GetLayouts()
        {
            string keyDir = Path.Combine(AppContext.BaseDirectory, "key");
            LayoutFileMap.Clear();

            if (!Directory.Exists(keyDir)) return LayoutFileMap;

            // Browse through the JSON files in the “key” folder.
            foreach (var file in Directory.GetFiles(keyDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    using var stream = File.OpenRead(file);
                    using var doc = JsonDocument.Parse(stream);

                    if (doc.RootElement.TryGetProperty("Name", out var nameProp))
                    {
                        string layoutName = nameProp.GetString() ?? "Unknown";
                        LayoutFileMap[layoutName] = Path.GetFileName(file); // Map the name to the file
                    }
                    else
                    {
                        LayoutFileMap["Unknown"] = Path.GetFileName(file);
                    }
                }
                catch
                {
                    LayoutFileMap["Unknown"] = Path.GetFileName(file);
                }
            }

            return LayoutFileMap;
        }
        
        // Load a layout from a json file (and edit older loaded layout)
        public static void LoadLayoutFromFile(string jsonFileName)
        {
            try
            {
                string layoutPath = Path.Combine(AppContext.BaseDirectory, "key", jsonFileName);

                if (!File.Exists(layoutPath))
                {
                    Console.WriteLine($"Fichier introuvable : {layoutPath}");
                    return;
                }

                string json = File.ReadAllText(layoutPath);
                Dictionary<string, string>? rawDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                CurrentLayoutJson = jsonFileName;

                // Update KeyVal in place to avoid completely replacing the object.
                KeyVal.Clear();

                for (int i = 1; i <= 47; i++)
                {
                    string keyName = $"Key{i}";
                    string? charVal = null, shiftVal = null, idVal = null;
                    rawDict?.TryGetValue($"{keyName}_Char", out charVal);
                    rawDict?.TryGetValue($"Shift{i}_Char", out shiftVal);
                    rawDict?.TryGetValue($"{keyName}_ID", out idVal);

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
                if (!Utility.IsShift)
                {
                    var baseKey = key.Replace("Shift", "");
                    if (KeyVal.TryGetValue(baseKey, out var keyData)) return keyData.shiftChar;
                }

                return "";
            }

            if (!KeyVal.TryGetValue(key, out var keyInfo)) return "";

            if (Utility.IsShift)
            {
                if (keyInfo.shiftChar == "\r") return keyInfo.keyChar.ToUpper();
                return keyInfo.shiftChar;
            }

            return keyInfo.keyChar;
        }

        // Conversion table of Key ID beetween SharpHook (Used on Windows and macOS) and Avalonia Input (Used on Linux)
        public static String[,] SharpAvaKeyId =
        {
            { "OemQuotes", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0", "Oem4", "OemPlus", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "OemCloseBrackets", "OemSemicolon", "OemPipe", "Oem3", "OemComma", "OemPeriod", "OemQuestion", "Oem8" },
            { "VcQuote", "Vc1", "Vc2", "Vc3", "Vc4", "Vc5", "Vc6", "Vc7", "Vc8", "Vc9", "Vc0", "VcOpenBracket", "VcEquals", "VcA", "VcB", "VcC", "VcD", "VcE", "VcF", "VcG", "VcH", "VcI", "VcJ", "VcK", "VcL", "VcM", "VcN", "VcO", "VcP", "VcQ", "VcR", "VcS", "VcT", "VcU", "VcV", "VcW", "VcX", "VcY", "VcZ", "VcCloseBracket", "VcSemicolon", "VcBackslash", "VcBackQuote", "VcComma", "VcPeriod", "VcSlash", "VcMisc" }
        };
        
        public static string? ConvertToVcKey(string sharpAvaKey)
        {
            for (int i = 0; i < SharpAvaKeyId.GetLength(1); i++)
            {
                if (SharpAvaKeyId[0, i] == sharpAvaKey)
                {
                    return SharpAvaKeyId[1, i];
                }
            }
            return null;
        }
        
        public static void UpdateKey(string keyName, string keyChar, string shiftChar, string keyId)
        {
            KeyVal[keyName] = (keyChar, shiftChar, keyId);
        }
    }
}