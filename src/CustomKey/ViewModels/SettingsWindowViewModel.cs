using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        public SettingsWindowViewModel()
        {
            Utility.GlobalRefresh += () => OnPropertyChanged(string.Empty);
        }
        
        public string VersionNumber => "Version " + Utility.GetAssemblyVersion();
        public string[] Languages { get; } = Translator.Languages;
        public string[] AppThemes { get; } = ["System", "Light", "Dark"];

        public string CurrentAppTheme
        {
            get => SettingsReader.ThemeValue;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader.ThemeValue, value))
                {
                    SettingsReader.ApplyTheme(value);
                    SettingsReader.SaveSettings(); // Saves settings after changing theme
                }
            }
        }
        
        private void UpdateImageSources()
        {
            IImage? TryLoad(string? path, int width, bool allowAvares = false)
            {
                if (string.IsNullOrEmpty(path)) return null;

                try
                {
                    if (allowAvares && path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
                    {
                        using var asset = AssetLoader.Open(new Uri(path));
                        return Bitmap.DecodeToWidth(asset, width, BitmapInterpolationMode.LowQuality);
                    }

                    if (!File.Exists(path)) return null;

                    if (!(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                          path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))
                        return null;

                    using var stream = File.OpenRead(path);
                    return Bitmap.DecodeToWidth(stream, width, BitmapInterpolationMode.LowQuality);
                }
                catch
                {
                    return null;
                }
            }

            if (_customBg1 != null) _customBg1.Source = TryLoad(SettingsReader.CustomBackgroundPath, 180);
            if (_customBg2 != null) _customBg2.Source = TryLoad(SettingsReader.CustomBackgroundPath2, 180);
            if (_currentBg != null) _currentBg.Source = TryLoad(SettingsReader.BackgroundPath, 960, allowAvares: true);
        }

        // Added new properties for other parameters
        public bool CustomBg
        {
            get => SettingsReader.CustomBgEnabled;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader.CustomBgEnabled, value))
                {
                    SettingsReader.SaveSettings();
                }
            }
        }
        
        public string Bgpath
        {
            set => RaiseAndSetIfChanged(ref SettingsReader.BackgroundPath, value);
        }

        private string CustomBgpath
        {
            get => SettingsReader.CustomBackgroundPath;
            set => RaiseAndSetIfChanged(ref SettingsReader.CustomBackgroundPath, value);
        }

        private string CustomBgpath2
        {
            get => SettingsReader.CustomBackgroundPath2;
            set => RaiseAndSetIfChanged(ref SettingsReader.CustomBackgroundPath2, value);
        }
        
        private CancellationTokenSource? _langCts; // Token to prevent temporary UI memory leaks when changing language rapidly
        public string Language
        {
            get => SettingsReader.LanguageCode;
            set
            {
                if (!RaiseAndSetIfChanged(ref SettingsReader.LanguageCode, value)) return;

                _langCts?.Cancel();
                _langCts = new CancellationTokenSource();
                var token = _langCts.Token;
                
                Task.Run(async () =>
                {
                    await Task.Delay(100, token);
                    if (token.IsCancellationRequested) return;
                    
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        SettingsReader.SaveSettings();
                        Translator.LoadSelectedLanguage();
                        Utility.RaiseGlobalRefresh(); // refresh translation bindings
                    });
                });
            }
        }

        // New references for images
        private Image? _customBg1;
        private Image? _customBg2;
        private Image? _currentBg;

        // Method for initializing images in the view
        public void InitializeImages(Image customBg1, Image customBg2, Image currentBg)
        {
            _customBg1 = customBg1;
            _customBg2 = customBg2;
            _currentBg = currentBg;
            
            UpdateImageSources();
        }

        // Method for attaching click events to images
        public void AttachImageClickEvents(Image? curvesImage, Image? geoImage, Image? gradientImage, Image? redImage)
        {
            // Load image source at the same time
            curvesImage?.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/curves.jpg")), 200, BitmapInterpolationMode.LowQuality);
            geoImage?.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/geo.jpg")), 200, BitmapInterpolationMode.LowQuality);
            gradientImage?.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/gradient.jpg")), 200, BitmapInterpolationMode.LowQuality);
            redImage?.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/red.jpg")), 200, BitmapInterpolationMode.LowQuality);
            
            curvesImage?.PointerPressed += (_, _) => OnImageClicked("avares://CustomKey/Assets/background/curves.jpg");
            geoImage?.PointerPressed += (_, _) => OnImageClicked("avares://CustomKey/Assets/background/geo.jpg");
            gradientImage?.PointerPressed += (_, _) => OnImageClicked("avares://CustomKey/Assets/background/gradient.jpg");
            redImage?.PointerPressed += (_, _) => OnImageClicked("avares://CustomKey/Assets/background/red.jpg");

            // Added click events for custom images
            _customBg1?.PointerPressed += (_, _) => OnImageClicked("CustomBG1");
            _customBg2?.PointerPressed += (_, _) => OnImageClicked("CustomBG2");
        }

        private void OnImageClicked(string imageName)
        {
            switch (imageName)
            {
                case "CustomBG1":
                    Bgpath = CustomBgpath;
                    break;
                case "CustomBG2":
                    Bgpath = CustomBgpath2;
                    break;
                default:
                    Bgpath = imageName;
                    break;
            }
            UpdateImageSources();
            SettingsReader.SaveSettings();
        }

        public void UploadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(SettingsReader.CustomBackgroundPath))
            {
                CustomBgpath = imagePath;
            }
            else
            {
                CustomBgpath2 = CustomBgpath;
                CustomBgpath = imagePath;
            }
            Bgpath = imagePath;
            UpdateImageSources();
            SettingsReader.SaveSettings();
        }
    }
}