using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using CustomKey.Common;
using CustomKey.ViewModels;

namespace CustomKey.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public static bool isCapsLockActive = false;
        public static bool isShiftPressed = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            Instance = this;

            settingsButton.Click += OpenSettingsWindow;
            aboutIco.PointerReleased += OpenAbout;

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
                    btn.Click += (s, e) => OnButtonClick(capturedKey);
                }
            }
            Backspc.Click += (s, e) => OnButtonClick("Backspc");
            Tab.Click += (s, e) => OnButtonClick("Tab");
            Caps.Click += (s, e) => OnButtonClick("Caps");
            Enter.Click += (s, e) => OnButtonClick("Enter");
            Space.Click += (s, e) => OnButtonClick("Space");
            Left.Click += (s, e) => OnButtonClick("Left");
            Right.Click += (s, e) => OnButtonClick("Right");
        }

        public void capsDown()
        {
            if (isCapsLockActive)
            {
                Caps.RenderTransform = new ScaleTransform(0.95, 0.95);
            }
            else
            {
                Caps.RenderTransform = new ScaleTransform(1, 1);
            }
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
                        OutputBox.Text = OutputBox.Text.Remove(start, end - start);
                        OutputBox.CaretIndex = start;
                    }
                    else if (start > 0)
                    {
                        OutputBox.Text = OutputBox.Text.Remove(start - 1, 1);
                        OutputBox.CaretIndex = start - 1;
                    }
                    break;
                case "Tab":
                    InsertText("\t");
                    break;
                case "Caps":
                    isCapsLockActive = !isCapsLockActive;
                    Utility.IsShift = isCapsLockActive || isShiftPressed;
                    capsDown();
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
                    var charToInsert = LayoutInit.GetChar(btnName);
                    InsertText(charToInsert);
                    break;
            }
        }
        
        private void InsertText(string text)
        {
            if (OutputBox.Text == null)
                OutputBox.Text = "";

            int start = Math.Min(OutputBox.SelectionStart, OutputBox.SelectionEnd);
            int end = Math.Max(OutputBox.SelectionStart, OutputBox.SelectionEnd);
            int length = end - start;

            if (length > 0)
                OutputBox.Text = OutputBox.Text.Remove(start, length);

            OutputBox.Text = OutputBox.Text.Insert(start, text);
            OutputBox.CaretIndex = start + text.Length;
        }

        private async void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            SettingsReader.SettingsChanged += LoadBackgroundSettings;

            await settingsWindow.ShowDialog(this);
        }

        public async void OpenAbout(object sender, PointerReleasedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                PrimaryButtonText = string.Empty,
                SecondaryButtonText = string.Empty,
                CloseButtonText = "Close",
                Content = new AboutDialog()
            };

            await dialog.ShowAsync();
        }
        
        private void LoadBackgroundSettings()
        {
            var backgroundImage = this.FindControl<Image>("BackgroundImage");
            if (SettingsReader._customBg && !string.IsNullOrEmpty(SettingsReader._Bgpath)) {
                backgroundImage.IsVisible = true;
                try {
                    if (SettingsReader._Bgpath.StartsWith("avares://")) {
                        Stream asset = AssetLoader.Open(new Uri(SettingsReader._Bgpath));
                        backgroundImage.Source = asset != null ? new Bitmap(asset) : null; // No Decode needed because they are already in 720p
                    } else if (File.Exists(SettingsReader._Bgpath) &&
                              (SettingsReader._Bgpath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || SettingsReader._Bgpath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               SettingsReader._Bgpath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || SettingsReader._Bgpath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))
                    {
                        backgroundImage.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader._Bgpath), 1280, BitmapInterpolationMode.LowQuality);
                    } else {
                        backgroundImage.Source = null;
                    }
                } catch {
                    backgroundImage.Source = null;
                }
            } else {
                backgroundImage.IsVisible = false;
            }
        }
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftPressed = true;
                Utility.IsShift = true;
            }
            else if (e.Key == Key.CapsLock)
            {
                isCapsLockActive = !isCapsLockActive;
                Utility.IsShift = isCapsLockActive || isShiftPressed;
                capsDown();
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                     e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                     e.Key == Key.LWin || e.Key == Key.RWin)
            {
                App.isCtrlAltPressed = true;
            }
            else if (Utility._inputOn && !App.isCtrlAltPressed)
            {
                //To do
            }
        }
        
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftPressed = false;
                Utility.IsShift = isCapsLockActive;
            }
            else if (e.Key == Key.CapsLock)
            {
                Utility.IsShift = isCapsLockActive || isShiftPressed;
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                     e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                     e.Key == Key.LWin || e.Key == Key.RWin)
            {
                App.isCtrlAltPressed = false;
            }
        }
    }
}