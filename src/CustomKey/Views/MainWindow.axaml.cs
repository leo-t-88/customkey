using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CustomKey.ViewModels;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using CustomKey.Common;

namespace CustomKey.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            
            settingsButton.Click += OpenSettingsWindow;
            aboutIco.PointerReleased += OpenAbout;

            LoadBackgroundSettings();
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
    }
}