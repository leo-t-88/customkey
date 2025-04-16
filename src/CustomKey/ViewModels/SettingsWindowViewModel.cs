using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        public SettingsWindowViewModel()
        {
            VersionNumber = "Version " + Utility.GetAssemblyVersion();
        }

        private string _versionNumber;

        public string VersionNumber
        {
            get { return _versionNumber; }
            set
            {
                _versionNumber = value;
                OnPropertyChanged(nameof(VersionNumber));
            }
        }
        public string[] AppThemes { get; } = new[] { "System", "Light", "Dark" };
        
        public string[] Languages { get; } = new[] { "English", "Not Implemented" };

        public string CurrentAppTheme
        {
            get
            {
                Console.WriteLine($"Loaded theme: {SettingsReader._currentAppTheme}"); // Print le theme enregistré dans les paramètres à supprimer une fois que tout sera ok
                return SettingsReader._currentAppTheme;
            }
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._currentAppTheme, value))
                {
                    SettingsReader.ApplyTheme(value);
                    SettingsReader.SaveSettings(); // Sauvegarde des paramètres après changement du thème
                }
            }
        }
        
        private void UpdateImageSources()
        {
            if (_customBG1 != null && 
                !string.IsNullOrEmpty(SettingsReader._customBgpath) && 
                File.Exists(SettingsReader._customBgpath) &&
                ( SettingsReader._customBgpath.EndsWith("png", StringComparison.OrdinalIgnoreCase) || 
                  SettingsReader._customBgpath.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader._customBgpath.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader._customBgpath.EndsWith("webp", StringComparison.OrdinalIgnoreCase) )) {
                _customBG1.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader._customBgpath), 180, BitmapInterpolationMode.LowQuality);
            } else {
                _customBG1.Source = null;
            }

            if (_customBG2 != null && 
                !string.IsNullOrEmpty(SettingsReader._customBgpath2) && 
                File.Exists(SettingsReader._customBgpath2) &&
                ( SettingsReader._customBgpath2.EndsWith("png", StringComparison.OrdinalIgnoreCase) || 
                  SettingsReader._customBgpath2.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader._customBgpath2.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase) ||
                  SettingsReader._customBgpath2.EndsWith("webp", StringComparison.OrdinalIgnoreCase) )) {
                _customBG2.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader._customBgpath2), 180, BitmapInterpolationMode.LowQuality);
            } else {
                _customBG2.Source = null;
            }

            if (_currentBG != null && 
                !string.IsNullOrEmpty(SettingsReader._Bgpath) && 
                ( SettingsReader._Bgpath.StartsWith("avares://") || 
                  ( File.Exists(SettingsReader._Bgpath) && 
                    ( SettingsReader._Bgpath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader._Bgpath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader._Bgpath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                      SettingsReader._Bgpath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) )
                  )
                )) 
            {
                try {
                    if (SettingsReader._Bgpath.StartsWith("avares://")) {
                        Stream asset = AssetLoader.Open(new Uri(SettingsReader._Bgpath));
                        _currentBG.Source = asset != null ? Bitmap.DecodeToWidth(asset, 960, BitmapInterpolationMode.LowQuality) : null;
                    } else {
                        _currentBG.Source = Bitmap.DecodeToWidth(File.OpenRead(SettingsReader._Bgpath), 960, BitmapInterpolationMode.LowQuality);
                    }
                } catch {
                    _currentBG.Source = null;
                }
            } else {
                _currentBG.Source = null;
            }
            
            // Free unused memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Ajout des nouvelles propriétés pour les autres paramètres
        public bool CustomBg
        {
            get => SettingsReader._customBg;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._customBg, value))
                {
                    SettingsReader.SaveSettings();
                }
            }
        }
        
        public string Bgpath
        {
            get => SettingsReader._Bgpath;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._Bgpath, value));
            }
        }
        
        public string CustomBgpath
        {
            get => SettingsReader._customBgpath;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._customBgpath, value));
            }
        }

        public string CustomBgpath2
        {
            get => SettingsReader._customBgpath2;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._customBgpath2, value));
            }
        }
        
        public string Language
        {
            get => SettingsReader._language;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._language, value))
                {
                    SettingsReader.SaveSettings();
                }
            }
        }

        public bool AutoUpdate
        {
            get => SettingsReader._autoUpdate;
            set
            {
                if (RaiseAndSetIfChanged(ref SettingsReader._autoUpdate, value))
                {
                    SettingsReader.SaveSettings();
                }
            }
        }

        // Les nouvelles références pour les images
        private Image _customBG1;
        private Image _customBG2;
        private Image _currentBG;

        // Méthode pour initialiser les images dans la vue
        public void InitializeImages(Image customBG1, Image customBG2, Image CurrentBG)
        {
            _customBG1 = customBG1;
            _customBG2 = customBG2;
            _currentBG = CurrentBG;
            
            // Mettre à jour les sources des images
            UpdateImageSources();
        }

        // Nouvelle méthode pour attacher les événements de clic aux images
        public void AttachImageClickEvents(Image blueImage, Image curvesImage, Image geoImage, Image gradientImage, Image redImage)
        {
            // Load image source at the same time
            blueImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/blue.jpg")), 200, BitmapInterpolationMode.LowQuality);
            curvesImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/curves.jpg")), 200, BitmapInterpolationMode.LowQuality);
            geoImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/geo.jpg")), 200, BitmapInterpolationMode.LowQuality);
            gradientImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/gradient.jpg")), 200, BitmapInterpolationMode.LowQuality);
            redImage.Source = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://CustomKey/Assets/background/red.jpg")), 200, BitmapInterpolationMode.LowQuality);
            
            // Ajout des événements de clic pour les nouvelles images
            blueImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/blue.jpg");
            curvesImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/curves.jpg");
            geoImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/geo.jpg");
            gradientImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/gradient.jpg");
            redImage.PointerPressed += (sender, e) => OnImageClicked("avares://CustomKey/Assets/background/red.jpg");

            // Ajout des événements de clic pour les images personnalisées
            _customBG1.PointerPressed += (sender, e) => OnImageClicked("CustomBG1");
            _customBG2.PointerPressed += (sender, e) => OnImageClicked("CustomBG2");
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
            if (string.IsNullOrEmpty(SettingsReader._customBgpath))
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