using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CustomKey.ViewModels;
using CustomKey.Views;
using SharpHook;
using SharpHook.Data;
using System;
using System.IO;
using System.Text.Json;
using CustomKey.Common;

namespace CustomKey
{
    public partial class App : Application
    {
        // Windows and macOS - Not use on Linux
        private IEventSimulator _outputInjector = new EventSimulator();
        private SimpleGlobalHook _hook = new();
        // =================================
        private MainWindow? _mainWindow;
        public static bool IsCtrlAltPressed;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Verify if the settings json file existe, else it will create it with defaults settings
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (!File.Exists(jsonFilePath))
                {
                    var settings = new { Theme = "System", CustomBg = false, BgPath = "avares://CustomKey/Assets/background/gradient.jpg", CustomBgPath = "", CustomBgPath2 = "", Language = "English"};
                    string jsonString = JsonSerializer.Serialize(settings);
                    File.WriteAllText(jsonFilePath, jsonString);
                }
                
                // Load settings and apply theme
                SettingsReader.LoadSettings();
                Translator.LoadLanguages();
                SettingsReader.ApplyTheme(SettingsReader.ThemeValue);
                
                // Show the MainWindow
                _mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
                desktop.MainWindow = _mainWindow;
                _mainWindow.Show();

                // Global Hook and Text Simulation are very limited and low on Linux
                // => Linux will use Avalonia Input instead but will only work if the MainWindow is focus
                if (!OperatingSystem.IsLinux())
                {
                    _hook.KeyPressed += OnKeyPressed;
                    _hook.KeyReleased += OnKeyReleased;
                    _hook.RunAsync();
                    _mainWindow.Closing += (_, _) => _hook.Dispose(); //Stop the Hook when MainWindow is closed else the app will be not stopped
                }
            }
            base.OnFrameworkInitializationCompleted();
        }

        // Keyboard Key Manager for Windows and macOS
        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcLeftShift || e.Data.KeyCode == KeyCode.VcRightShift)
            {
                MainWindow.isShiftPressed = true;
                Utility.IsShift = true;
            }
            else if (e.Data.KeyCode == KeyCode.VcCapsLock)
            {
                MainWindow.isCapsLockActive = !MainWindow.isCapsLockActive;
                Utility.IsShift = MainWindow.isCapsLockActive || MainWindow.isShiftPressed;
                Dispatcher.UIThread.Post(() => { _mainWindow.CapsDown(); });
            }
            else if (e.Data.KeyCode == KeyCode.VcLeftControl || e.Data.KeyCode == KeyCode.VcRightControl ||
                     e.Data.KeyCode == KeyCode.VcLeftAlt || e.Data.KeyCode == KeyCode.VcRightAlt ||
                     e.Data.KeyCode == KeyCode.VcLeftMeta || e.Data.KeyCode == KeyCode.VcRightMeta)
            {
                IsCtrlAltPressed = true;
            }
            else if (Utility.IsInputEnabled && !IsCtrlAltPressed)
            {
                foreach (var entry in LayoutLoader.KeyVal)
                {
                    var (keyChar, shiftChar, keyId) = entry.Value;
                    
                    if (e.Data.KeyCode.ToString().Equals(keyId, StringComparison.OrdinalIgnoreCase))
                    {
                        string inputChar = LayoutLoader.GetChar(entry.Key);

                        if (!string.IsNullOrEmpty(inputChar))
                        {
                            e.SuppressEvent = true;
                            _outputInjector.SimulateTextEntry(inputChar);
                        }
                        break;
                    } 
                }
            }
        }
        
        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcLeftShift || e.Data.KeyCode == KeyCode.VcRightShift)
            {
                MainWindow.isShiftPressed = false;
                Utility.IsShift = MainWindow.isCapsLockActive;
            }
            else if (e.Data.KeyCode == KeyCode.VcCapsLock)
            {
                Utility.IsShift = MainWindow.isCapsLockActive || MainWindow.isShiftPressed;
            }
            else if (e.Data.KeyCode == KeyCode.VcLeftControl || e.Data.KeyCode == KeyCode.VcRightControl ||
                     e.Data.KeyCode == KeyCode.VcLeftAlt || e.Data.KeyCode == KeyCode.VcRightAlt ||
                     e.Data.KeyCode == KeyCode.VcLeftMeta || e.Data.KeyCode == KeyCode.VcRightMeta)
            {
                IsCtrlAltPressed = false;
            }
        }
    }
}