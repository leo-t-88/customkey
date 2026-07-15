using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CustomKey.Common;
using CustomKey.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CustomKey.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsWindowViewModel _viewModel;
        private bool _layoutToDelete;
            
        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsWindowViewModel();
            _viewModel.InitializeImages(CustomBg1, CustomBg2, CurrentBg);
            
            // Attaching click events
            _viewModel.AttachImageClickEvents(
                this.FindControl<Image>("CurvesImage"),
                this.FindControl<Image>("GeoImage"),
                this.FindControl<Image>("GradientImage"),
                this.FindControl<Image>("RedImage")
            );
            DataContext = _viewModel;
            
            UploadImageButton.Click += OnUploadImageClick;
            Layout.ItemsSource = LayoutLoader.GetLayouts().OrderBy(kv => kv.Key).Select(kv => new LayoutItem(kv.Key, kv.Value)).ToArray();
            
            this.Closed += (_, _) =>
            {
                UpdateParent();
                GC.Collect(); // Clean memory (mainly used by images) when this windows is closed
                GC.WaitForPendingFinalizers();
            };
        }
        
        private async void OnUploadImageClick(object? sender, RoutedEventArgs e)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Image Files") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"] }]
            });

            if (files.Count > 0)
            {
                var path = files[0].Path.LocalPath;
                if (File.Exists(path)) _viewModel.UploadImage(path);
            }
        }

        private void UpdateParent(string? editLayoutFile = null)
        {
            if (_layoutToDelete && Owner is MainWindow main && main.DataContext is MainWindowViewModel vmp)
            {
                _layoutToDelete = false;
                vmp.ReloadLayouts();
            }
            _layoutToDelete = false;

            if (editLayoutFile == null) return;
            LayoutLoader.LoadLayoutFromFile(editLayoutFile);
            Utility.IsEditingEnabled = true;
            Utility.RaiseGlobalRefresh();
            this.Close();
        }
        
        private void DeleteLayoutClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LayoutItem item)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "key", item.FileName);
                if (File.Exists(path)) File.Delete(path);
                Layout.ItemsSource = LayoutLoader.GetLayouts().OrderBy(kv => kv.Key).Select(kv => new LayoutItem(kv.Key, kv.Value)).ToArray();
                _layoutToDelete = true;
            }
        }
        
        private async void EditLayoutClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LayoutItem item) UpdateParent(item.FileName);
        }
        
        private void CreateLayoutClick(object? sender, RoutedEventArgs e)
        {
            Random random = Random.Shared;

            string? fileName = null;
            int attempts = 0;

            while (attempts < 3 && fileName is null)
            {
                attempts++;
                
                char[] chars = new char[5];
                for (int i = 0; i < 5; i++) chars[i] = random.Next(2) == 0 ? (char)('a' + random.Next(26)) : (char)('0' + random.Next(10));
                string tempname = new string(chars) + ".json";

                bool exists = LayoutLoader.GetLayouts().Values.Contains(tempname, StringComparer.OrdinalIgnoreCase);
                if (!exists) fileName = tempname;
            }

            if (fileName is null) return;

            string fullPath = Path.Combine(AppContext.BaseDirectory, "key", fileName);
            var json = JsonSerializer.Serialize( new { Name = "Unkown " + Path.GetFileNameWithoutExtension(fileName) }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fullPath, json);
            UpdateParent(fullPath);
        }
        
        public class LayoutItem
        {
            public string Name { get; }
            public string FileName { get; }

            public LayoutItem(string name, string fileName)
            {
                Name = name;
                FileName = fileName;
            }

            public override string ToString() => Name;
        }
    }
}