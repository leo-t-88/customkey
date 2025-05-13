using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace CustomKey.Common
{
    public static class Utility
    {
        // Événement déclenché lorsque l'état de Shift change
        public static event Action? IsShiftChanged;
        public static bool _isShift;
        public static bool _inputOn = true;
        
        public static string GetAssemblyVersion()
        {
            String version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            
            if (version == null) version = "non disponible";
            
            if (version == "0.0.1.0") version = "Dev Build";

            string[] versionParts = version.Split('.');
            return string.Join(".", versionParts.Take(3));
        }

        public static void OpenURL(String url)
        {
            try
            {
                // Should work on Windows and Linux, for MacOS I'm not sure
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" });
            }
            catch
            {
                Console.WriteLine("Unable to open " + url + " on this platform.");
            }
        }
        
        public static bool IsShift
        {
            get => _isShift;
            set
            {
                if (_isShift != value)
                {
                    _isShift = value;
                    IsShiftChanged?.Invoke(); // Notifie les abonnés que l'état a changé
                }
            }
        }
    }
}