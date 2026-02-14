using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CustomKey.Common;
using CustomKey.ViewModels;
using System;
using System.IO;

namespace CustomKey.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsWindowViewModel _viewModel;

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
            Layout.ItemsSource = LayoutLoader.GetLayoutNames();
            UploadImageButton.Click += OnUploadImageClick;
            DataContext = _viewModel;
            
            this.Closed += (_, _) =>
            {
                GC.Collect(); // Clean memory (mainly used by images) when this windows is closed
                GC.WaitForPendingFinalizers();
            };
        }
        
        private async void OpenEditLayoutClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string layoutName)
            {
                EditLayoutWindow layoutWindow = new EditLayoutWindow(layoutName);
                await layoutWindow.ShowDialog(this);
            }
        }
        
        private void DeleteLayoutClick(object? sender, RoutedEventArgs e)
        {
            //To do
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
                var file = files[0];
                var path = file.Path.LocalPath;

                if (File.Exists(path))
                {
                    _viewModel.UploadImage(path);
                }
            }
        }
    }
}