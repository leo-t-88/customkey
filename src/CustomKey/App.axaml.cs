using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CustomKey.ViewModels;
using CustomKey.Views;
using SharpHook;
using SharpHook.Native;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using CustomKey.Common;

namespace CustomKey
{
    public partial class App : Application
    {
        // Windows and macOS - Not use on Linux
        private IEventSimulator outputInjector = new EventSimulator();
        private SimpleGlobalHook _hook = new();
        // =================================
        private string _currentAppTheme;
        private MainWindow? _mainWindow;
        public static bool isCtrlAltPressed;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Open SplahScreen on app launching
                LoadScreen splashScreen = new() { DataContext = new LoadScreenViewModel() };
                splashScreen.Show();

                // Verify if the settings json file existe, else it will create it with defaults settings
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (!File.Exists(jsonFilePath))
                {
                    var settings = new { theme = "System", custombg = false, bgpath = "avares://CustomKey/Assets/background/gradient.jpg", custombgpath = "", custombgpath2 = "", language = "English", autoupdate = true };
                    string jsonString = JsonSerializer.Serialize(settings);
                    File.WriteAllText(jsonFilePath, jsonString);
                }
                
                // Load settings and apply theme
                SettingsReader.LoadSettings();
                SettingsReader.ApplyTheme(SettingsReader._currentAppTheme);

                var viewModel = (LoadScreenViewModel)splashScreen.DataContext;
                
                Task.Run(async () =>
                {
                    await viewModel.SimulateLoadingAsync();
                    // Assure que la fermeture de l'écran de chargement et l'ouverture de la fenêtre principale se fassent sur le thread UI.
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
                        desktop.MainWindow = _mainWindow;
                        _mainWindow.Show();
                        /*EditLayoutWindow layoutWindow = new EditLayoutWindow();
                        layoutWindow.Show();*/
                        splashScreen.Close();

                        // Global Hook and Text Simulation are very limited and low on Linux
                        // => Linux will use Avalonia Input instead but will only work if the MainWindow is focus
                        if (!OperatingSystem.IsLinux())
                        {
                            _hook.KeyPressed += OnKeyPressed;
                            _hook.KeyReleased += OnKeyReleased;
                            _hook.RunAsync();
                            _mainWindow.Closing += (_, _) => _hook.Dispose(); //Stop the Hook when MainWindow is closed else the app will be not stopped
                        }
                    });
                });
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
                Dispatcher.UIThread.Post(() => { _mainWindow.capsDown(); });
            }
            else if (e.Data.KeyCode == KeyCode.VcLeftControl || e.Data.KeyCode == KeyCode.VcRightControl ||
                     e.Data.KeyCode == KeyCode.VcLeftAlt || e.Data.KeyCode == KeyCode.VcRightAlt ||
                     e.Data.KeyCode == KeyCode.VcLeftMeta || e.Data.KeyCode == KeyCode.VcRightMeta)
            {
                isCtrlAltPressed = true;
            }
            else if (Utility._inputOn && !isCtrlAltPressed)
            {
                foreach (var entry in LayoutInit.KeyVal)
                {
                    var (keyChar, shiftChar, keyId) = entry.Value;
                    
                    if (e.Data.KeyCode.ToString().Equals(keyId, StringComparison.OrdinalIgnoreCase))
                    {
                        string inputChar = LayoutInit.GetChar(entry.Key);

                        if (!string.IsNullOrEmpty(inputChar))
                        {
                            e.SuppressEvent = true;
                            outputInjector.SimulateTextEntry(inputChar);
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
                isCtrlAltPressed = false;
            }
        }
    }
}