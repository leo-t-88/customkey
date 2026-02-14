using System;
using System.Reflection;
using System.Linq;

namespace CustomKey.Common
{
    public static class Utility
    {
        // Event triggered when the Shift state changes
        public static event Action? GlobalRefresh;
        public static bool IsShiftPending;
        public static bool IsInputEnabled = true;
        
        public static string GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown version";
            if (version == "0.0.1.0") version = "Dev Build";

            string[] versionParts = version.Split('.');
            return string.Join(".", versionParts.Take(3));
        }
        
        public static bool IsShift
        {
            get => IsShiftPending;
            set
            {
                if (IsShiftPending != value)
                {
                    IsShiftPending = value;
                    GlobalRefresh?.Invoke(); // Notifies that the status has changed
                }
            }
        }
        
        public static void RaiseGlobalRefresh()
        {
            GlobalRefresh?.Invoke();
        }
    }
}