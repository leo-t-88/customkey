using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using Avalonia;
using Avalonia.Layout;
using CustomKey.Common;
using CustomKey.ViewModels;
using FluentAvalonia.UI.Controls;

namespace CustomKey.Views
{
    public partial class MainWindow : Window
    {
        public static bool IsCapsLockActive;
        public static bool IsShiftPressed;
        private bool _isCtrlAltPressed;
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            
            SettingsButton.Click += OpenSettingsWindow;
            
            if (OperatingSystem.IsLinux())
            {
                this.KeyDown += OnKeyDown;
                this.KeyUp += OnKeyUp;
            }

            LoadBackgroundSettings();
            
            // Add click event to most of all Keyboard Buttons
            for (int i = 1; i <= 47; i++)
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
            
            Utility.EditModeChanged += () => Avalonia.Threading.Dispatcher.UIThread.Post(UpdateEditMode);
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

            foreach (var (btn, normalText) in keys) btn.Content = _viewModel.IsEditMode ? "\uE72E" : normalText;
            
            LayoutEditor.IsVisible = _viewModel.IsEditMode;
            LayoutsList.IsVisible = !_viewModel.IsEditMode;
            InputState.IsEnabled = !_viewModel.IsEditMode;
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
                    if (_viewModel.IsEditMode)
                    {
                        OpenEditDialog(btnName);
                        break;
                    }
                    var charToInsert = LayoutManager.GetChar(btnName);
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
            ((MainWindowViewModel)DataContext!).IsEditMode = false;
            
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
                _isCtrlAltPressed = true;
            }
            else if (Utility.IsInputEnabled && !_isCtrlAltPressed)
            {
                string? vcKey = LayoutManager.ConvertToVcKey(e.Key.ToString());

                if (vcKey != null)
                {
                    foreach (var entry in LayoutManager.KeyVal)
                    {
                        var (_, _, keyId) = entry.Value;
                    
                        if (vcKey.Equals(keyId, StringComparison.OrdinalIgnoreCase))
                        {
                            string inputChar = LayoutManager.GetChar(entry.Key);

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
                _isCtrlAltPressed = false;
            }
        }
        
        private async void OpenEditDialog(string keyName)
        {
            var (currentChar, currentShift, currentId) = LayoutManager.KeyVal[keyName];

            Button btnId = new Button { Content = currentId };
            btnId.Click += (_, _) =>
            {
                this.KeyDown += OnDetectKey;
                void OnDetectKey(object? s, KeyEventArgs e)
                {
                    this.KeyDown -= OnDetectKey;
                    var vc = LayoutManager.ConvertToVcKey(e.Key.ToString());
                    if (vc != null) btnId.Content = vc;
                }
            };

            TextBox tbVal = new TextBox { Text = currentChar, Height = 32, TextWrapping = TextWrapping.NoWrap }; 
            TextBox tbShift = new TextBox { Text = currentShift, Height = 32, TextWrapping = TextWrapping.NoWrap };
            TextBlock lblId = new TextBlock { Text = "Key ID" };
            TextBlock lblVal = new TextBlock { Text = "Value" };
            TextBlock lblShift = new TextBlock { Text = "Caps Lock Value" };
            CheckBox cbUpShift = new CheckBox { Content = $"Uppercase default when Caps Lock", IsChecked = currentShift == null };

            Grid grid = new Grid
            {
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                HorizontalAlignment = HorizontalAlignment.Center, ColumnSpacing = 8
            };

            Grid.SetRow(lblId, 0); Grid.SetColumn(lblId, 0);
            Grid.SetRow(lblVal, 0); Grid.SetColumn(lblVal, 1);
            Grid.SetRow(lblShift, 0); Grid.SetColumn(lblShift, 2);
            Grid.SetRow(btnId, 1); Grid.SetColumn(btnId, 0);
            Grid.SetRow(tbVal, 1); Grid.SetColumn(tbVal, 1);
            Grid.SetRow(tbShift, 1); Grid.SetColumn(tbShift, 2);
            Grid.SetRow(cbUpShift, 2); Grid.SetColumn(cbUpShift, 2);

            grid.Children.Add(lblId); grid.Children.Add(lblVal); grid.Children.Add(lblShift);
            grid.Children.Add(btnId); grid.Children.Add(tbVal); grid.Children.Add(tbShift);
            grid.Children.Add(cbUpShift);

            FAContentDialog dialog = new FAContentDialog { Content = grid, PrimaryButtonText = "Save", CloseButtonText = "Cancel" };

            if (await dialog.ShowAsync() == FAContentDialogResult.Primary)
            {
                LayoutManager.UpdateKey(keyName, tbVal.Text, cbUpShift.IsChecked == true ? null : tbShift.Text, btnId.Content?.ToString() ?? "");
                _viewModel.NotifyKeyChanged();
            }
        }
    }
}