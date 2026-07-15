using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using CustomKey.Common;
using CustomKey.ViewModels;

namespace CustomKey.Views
{
    public partial class MainWindow : Window
    {
        public static bool IsCapsLockActive;
        public static bool IsShiftPressed;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            SettingsButton.Click += OpenSettingsWindow;
            
            if (OperatingSystem.IsLinux())
            {
                this.KeyDown += OnKeyDown;
                this.KeyUp += OnKeyUp;
            }

            LoadBackgroundSettings();
            
            // Add click event to most of all Keyboard Buttons
            for (int i = 1; i < 47; i++)
            {
                var keyName = $"Key{i}";
                var btn = this.FindControl<Button>(keyName);
                if (btn != null)
                {
                    string capturedKey = keyName;
                    btn.Click += (_, _) => OnButtonClick(capturedKey);
                }
            }
            
            Backspc.Click += (_, _) => OnButtonClick("Backspc");
            Tab.Click += (_, _) => OnButtonClick("Tab");
            Caps.Click += (_, _) => OnButtonClick("Caps");
            Enter.Click += (_, _) => OnButtonClick("Enter");
            Space.Click += (_, _) => OnButtonClick("Space");
            Left.Click += (_, _) => OnButtonClick("Left");
            Right.Click += (_, _) => OnButtonClick("Right");
            
            Utility.GlobalRefresh += () => Avalonia.Threading.Dispatcher.UIThread.Post(UpdateEditMode);
        }
        
        private void UpdateEditMode()
        {
            var keys = new (Button btn, string normalText)[]
            {
                (Backspc, "Backspc"),
                (Tab, "Tab"),
                (Enter, "Enter \uEB97"),
                (Shift, "\uE87F Shift"),
                (Shift2, "Shift \uE87F"),
                (Ctrl, "Ctrl"),
                (Alt, "Alt"),
                (AltGr, "Alt Gr"),
                (Left, "⮜"),
                (Right, "⮞")
            };

            foreach (var (btn, normalText) in keys) btn.Content = Utility.IsEditingEnabled ? "\uE72E" : normalText;
        }

        public void CapsDown()
        {
            Caps.RenderTransform = IsCapsLockActive ? new ScaleTransform(0.95, 0.95) : new ScaleTransform(1, 1);
        }

        private void OnButtonClick(string btnName)
        {
            switch (btnName)
            {
                case "Backspc":
                    int start = Math.Min(OutputBox.SelectionStart, OutputBox.SelectionEnd);
                    int end = Math.Max(OutputBox.SelectionStart, OutputBox.SelectionEnd);
                    if (end - start > 0)
                    {
                        OutputBox.Text = OutputBox.Text?.Remove(start, end - start);
                        OutputBox.CaretIndex = start;
                    }
                    else if (start > 0)
                    {
                        OutputBox.Text = OutputBox.Text?.Remove(start - 1, 1);
                        OutputBox.CaretIndex = start - 1;
                    }
                    break;
                case "Tab":
                    InsertText("\t");
                    break;
                case "Caps":
                    IsCapsLockActive = !IsCapsLockActive;
                    Utility.IsShift = IsCapsLockActive || IsShiftPressed;
                    CapsDown();
                    break;
                case "Enter":
                    InsertText("\n");
                    break;
                case "Space":
                    InsertText(" ");
                    break;
                case "Left":
                    if (OutputBox.CaretIndex > 0) OutputBox.CaretIndex -= 1;
                    break;
                case "Right":
                    OutputBox.CaretIndex += 1;
                    break;
                default:
                    var charToInsert = LayoutLoader.GetChar(btnName);
                    InsertText(charToInsert);
                    break;
            }
        }
        
        private void InsertText(string text)
        {
            OutputBox.Text ??= "";

            int start = Math.Min(OutputBox.SelectionStart, OutputBox.SelectionEnd);
            int end = Math.Max(OutputBox.SelectionStart, OutputBox.SelectionEnd);
            int length = end - start;

            if (length > 0) OutputBox.Text = OutputBox.Text.Remove(start, length);

            OutputBox.Text = OutputBox.Text.Insert(start, text);
            OutputBox.CaretIndex = start + text.Length;
        }

        private async void OpenSettingsWindow(object? sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            SettingsReader.SettingsChanged += LoadBackgroundSettings;

            await settingsWindow.ShowDialog(this);
        }
        
        private void LoadBackgroundSettings()
        {
            var backgroundImage = this.FindControl<Image>("BackgroundImage");
            if (SettingsReader.CustomBgEnabled && !string.IsNullOrEmpty(SettingsReader.BackgroundPath)) {
                backgroundImage?.IsVisible = true;
                try {
                    if (SettingsReader.BackgroundPath.StartsWith("avares://")) {
                        Stream asset = AssetLoader.Open(new Uri(SettingsReader.BackgroundPath));
                        backgroundImage?.Source = new Bitmap(asset); // No Decode needed because they are already in 720p
                    } else if (File.Exists(SettingsReader.BackgroundPath) &&
                              (SettingsReader.BackgroundPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || SettingsReader.BackgroundPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               SettingsReader.BackgroundPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || SettingsReader.BackgroundPath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))
                    {
                        backgroundImage?.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader.BackgroundPath), 1280, BitmapInterpolationMode.LowQuality);
                    } else {
                        backgroundImage?.Source = null;
                    }
                } catch {
                    backgroundImage?.Source = null;
                }
            } else {
                backgroundImage?.IsVisible = false;
            }
        }
        
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                IsShiftPressed = true;
                Utility.IsShift = true;
            }
            else if (e.Key == Key.CapsLock)
            {
                IsCapsLockActive = !IsCapsLockActive;
                Utility.IsShift = IsCapsLockActive || IsShiftPressed;
                CapsDown();
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                     e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                     e.Key == Key.LWin || e.Key == Key.RWin)
            {
                App.IsCtrlAltPressed = true;
            }
            else if (Utility.IsInputEnabled && !App.IsCtrlAltPressed)
            {
                string? vcKey = LayoutLoader.ConvertToVcKey(e.Key.ToString());

                if (vcKey != null)
                {
                    foreach (var entry in LayoutLoader.KeyVal)
                    {
                        var (_, _, keyId) = entry.Value;
                    
                        if (vcKey.Equals(keyId, StringComparison.OrdinalIgnoreCase))
                        {
                            string inputChar = LayoutLoader.GetChar(entry.Key);

                            if (!string.IsNullOrEmpty(inputChar))
                            {
                                e.Handled = true;
                                InsertText(inputChar);
                            }
                            break;
                        } 
                    }
                }
            }
        }
        
        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                IsShiftPressed = false;
                Utility.IsShift = IsCapsLockActive;
            }
            else if (e.Key == Key.CapsLock)
            {
                Utility.IsShift = IsCapsLockActive || IsShiftPressed;
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                     e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                     e.Key == Key.LWin || e.Key == Key.RWin)
            {
                App.IsCtrlAltPressed = false;
            }
        }
    }
}