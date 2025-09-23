using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CustomKey.ViewModels;
using System.IO;
using CustomKey.Common;

namespace CustomKey.Views
{
    public partial class SettingsWindow : Window
    {
        private SettingsWindowViewModel _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsWindowViewModel();
            _viewModel.InitializeImages(CustomBG1, CustomBG2, CurrentBG);
            
            // Attaching click events
            _viewModel.AttachImageClickEvents(
                this.FindControl<Image>("CurvesImage"),
                this.FindControl<Image>("GeoImage"),
                this.FindControl<Image>("GradientImage"),
                this.FindControl<Image>("RedImage")
            );
            
            LaunchRepoLinkItem.Click += (sender, args) => { Utility.OpenURL("https://github.com/leo-t-88/customkey"); };
            OpenLayout.Click += (sender, args) => { Utility.OpenURL(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key")); };
            OpenGetKey.Click += OpenGetKeyClick;
            UploadImageButton.Click += OnUploadImageClick;
            DataContext = _viewModel;
            
            this.Closed += (sender, args) =>
            {
                GC.Collect(); // Clean memory (mainly used by images) when this windows is closed
                GC.WaitForPendingFinalizers();
            };
        }
        
        private async void OpenGetKeyClick(object sender, RoutedEventArgs e)
        {
            EditLayoutWindow layoutWindow = new EditLayoutWindow();
            await layoutWindow.ShowDialog(this);
        }

        private async void OnUploadImageClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new()
                {
                    new FileDialogFilter() { Name = "Image Files", Extensions = { "png", "jpg", "jpeg", "webp" } }
                }
            };

            var result = await openFileDialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                var imagePath = result[0];
                if (File.Exists(imagePath))
                {
                    _viewModel.UploadImage(imagePath);
                }
            }
        }
    }
}