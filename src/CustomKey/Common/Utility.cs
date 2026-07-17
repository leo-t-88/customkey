using System;
using System.Reflection;
using System.Linq;

namespace CustomKey.Common
{
    public static class Utility
    {
        public static event Action? GlobalRefresh, EditModeChanged;
        public static bool IsInputEnabled = true;
        
        public static string GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown version";
            
            if (version.StartsWith("0.0.0")) return "Dev Build";

            string[] versionParts = version.Split('.');
            return string.Join(".", versionParts.Take(3));
        }
        
        private static bool _isShiftPending;
        public static bool IsShift
        {
            get => _isShiftPending;
            set
            {
                if (_isShiftPending != value)
                {
                    _isShiftPending = value;
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