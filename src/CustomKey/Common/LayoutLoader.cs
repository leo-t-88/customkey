using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CustomKey.Common
{
    public static class LayoutLoader
    {
        private static Dictionary<string, string> _layoutNameToFileMap = new();

        // Retourne une liste des noms des layouts (propriété "Name" dans les fichiers JSON)
        public static string[] GetLayoutNames()
        {
            var layoutNames = new List<string>();
            string keyDir = Path.Combine(AppContext.BaseDirectory, "Key");

            if (!Directory.Exists(keyDir))
                return layoutNames.ToArray();

            // Parcourt les fichiers JSON dans le dossier "Key"
            foreach (var file in Directory.GetFiles(keyDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    using var stream = File.OpenRead(file);
                    using var doc = JsonDocument.Parse(stream);

                    if (doc.RootElement.TryGetProperty("Name", out var nameProp))
                    {
                        string layoutName = nameProp.GetString() ?? "Unknown";
                        layoutNames.Add(layoutName);
                        _layoutNameToFileMap[layoutName] = Path.GetFileName(file); // Mappe le nom au fichier
                    }
                    else
                    {
                        layoutNames.Add("Unknown");
                        _layoutNameToFileMap["Unknown"] = Path.GetFileName(file);
                    }
                }
                catch
                {
                    layoutNames.Add("Unknown");
                    _layoutNameToFileMap["Unknown"] = Path.GetFileName(file);
                }
            }

            return layoutNames.ToArray();
        }

        // Retourne le nom du fichier JSON correspondant à un layout donné
        public static string GetJsonFileName(string layoutName)
        {
            return _layoutNameToFileMap.TryGetValue(layoutName, out var fileName) ? fileName : null;
        }
    }
}