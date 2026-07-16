using System;
using System.Reflection;
using System.Linq;

namespace CustomKey.Common
{
    public static class Utility
    {
        public static event Action? GlobalRefresh, EditModeChanged;
        public static bool IsShiftPending;
        public static bool IsInputEnabled = true;
        
        public static string GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown version";
            
            if (version.StartsWith("0.0.0")) return "Dev Build";

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
                    GlobalRefresh?.Invoke();
                }
            }
        }
        
        public static void RaiseGlobalRefresh()
        {
            GlobalRefresh?.Invoke();
        }

        public static void RaiseEditModeChanged()
        {
            EditModeChanged?.Invoke();
        }
    }
}