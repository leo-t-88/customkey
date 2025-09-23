using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        public SettingsWindowViewModel()
        {
            Utility.GlobalRefresh += () => OnPropertyChanged("");
        }

        public string VersionNumber => "Version " + Utility.GetAssemblyVersion();
        public string[] AppThemes { get; } = new[] { "System", "Light", "Dark" };
        public string[] Languages { get; } = Translator.Languages;

        public string CurrentAppTheme
        {
            get
            {
                Console.WriteLine($"Loaded theme: {SettingsReader.ThemeValue}");
                return SettingsReader.ThemeValue;
            }
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
            if (_customBg1 != null && 
                !string.IsNullOrEmpty(SettingsReader.CustomBackgroundPath) && 
                File.Exists(SettingsReader.CustomBackgroundPath) &&
                ( SettingsReader.CustomBackgroundPath.EndsWith("png", StringComparison.OrdinalIgnoreCase) || 
                  SettingsReader.CustomBackgroundPath.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader.CustomBackgroundPath.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader.CustomBackgroundPath.EndsWith("webp", StringComparison.OrdinalIgnoreCase) )) {
                _customBg1.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader.CustomBackgroundPath), 180, BitmapInterpolationMode.LowQuality);
            } else {
                _customBg1.Source = null;
            }

            if (_customBg2 != null && 
                !string.IsNullOrEmpty(SettingsReader.CustomBackgroundPath2) && 
                File.Exists(SettingsReader.CustomBackgroundPath2) &&
                ( SettingsReader.CustomBackgroundPath2.EndsWith("png", StringComparison.OrdinalIgnoreCase) || 
                  SettingsReader.CustomBackgroundPath2.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader.CustomBackgroundPath2.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader.CustomBackgroundPath2.EndsWith("webp", StringComparison.OrdinalIgnoreCase) )) {
                _customBg2.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader.CustomBackgroundPath2), 180, BitmapInterpolationMode.LowQuality);
            } else {
                _customBg2.Source = null;
            }

            if (_currentBg != null && 
                !string.IsNullOrEmpty(SettingsReader.BackgroundPath) && 
                ( SettingsReader.BackgroundPath.StartsWith("avares://") || 
                  ( File.Exists(SettingsReader.BackgroundPath) && 
                    ( SettingsReader.BackgroundPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader.BackgroundPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader.BackgroundPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader.BackgroundPath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) )
                  )
                )) 
            {
                try {
                    if (SettingsReader.BackgroundPath.StartsWith("avares://")) {
                        Stream asset = AssetLoader.Open(new Uri(SettingsReader.BackgroundPath));
                        _currentBg.Source = asset != null ? Bitmap.DecodeToWidth(asset, 960, BitmapInterpolationMode.LowQuality) : null;
                    } else {
                        _currentBg.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader.BackgroundPath), 960, BitmapInterpolationMode.LowQuality);
                    }
                } catch {
                    _currentBg.Source = null;
                }
            } else {
                _currentBg.Source = null;
            }
            
            // Free unused memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            get => SettingsReader.BackgroundPath;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader.BackgroundPath, value));
            }
        }
        
        public string CustomBgpath
        {
            get => SettingsReader.CustomBackgroundPath;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader.CustomBackgroundPath, value));
            }
        }

        public string CustomBgpath2
        {
            get => SettingsReader.CustomBackgroundPath2;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader.CustomBackgroundPath2, value));
            }
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
        private Image _customBg1;
        private Image _customBg2;
        private Image _currentBg;

        // Method for initializing images in the view
        public void InitializeImages(Image customBg1, Image customBg2, Image currentBg)
        {
            _customBg1 = customBg1;
            _customBg2 = customBg2;
            _currentBg = currentBg;
            
            UpdateImageSources();
        }

        // Method for attaching click events to images
        public void AttachImageClickEvents(Image curvesImage, Image geoImage, Image gradientImage, Image redImage)
        {
            // Load image source at the same time
            curvesImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/curves.jpg")), 200, BitmapInterpolationMode.LowQuality);
            geoImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/geo.jpg")), 200, BitmapInterpolationMode.LowQuality);
            gradientImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/gradient.jpg")), 200, BitmapInterpolationMode.LowQuality);
            redImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/red.jpg")), 200, BitmapInterpolationMode.LowQuality);
            
            curvesImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/curves.jpg");
            geoImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/geo.jpg");
            gradientImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/gradient.jpg");
            redImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/red.jpg");

            // Added click events for custom images
            _customBg1.PointerPressed += (sender, e) => OnImageClicked("CustomBG1");
            _customBg2.PointerPressed += (sender, e) => OnImageClicked("CustomBG2");
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