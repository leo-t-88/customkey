using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CustomKey.ViewModels;
using CustomKey.Views;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using CustomKey.Common;

namespace CustomKey
{
    public partial class App : Application
    {
        private string _currentAppTheme;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Open SplahScreen on app launching
                LoadScreen splashScreen = new() { DataContext = new LoadScreenViewModel() };
                splashScreen.Show();

                // Vérifier et créer le fichier JSON au premier démarrage
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (!File.Exists(jsonFilePath))
                {
                    var settings = new { theme = "System", custombg = false, bgpath = "avares://CustomKey/Assets/background/gradient.jpg", custombgpath = "", custombgpath2 = "", language = "English", autoupdate = true };
                    string jsonString = JsonSerializer.Serialize(settings);
                    File.WriteAllText(jsonFilePath, jsonString);
                }
                
                // Load settings and apply theme
                SettingsReader.LoadSettings();
                SettingsReader.ApplyTheme(SettingsReader._currentAppTheme);

                var viewModel = (LoadScreenViewModel)splashScreen.DataContext;
                Task.Run(async () =>
                {
                    await viewModel.SimulateLoadingAsync();
                    // Assure que la fermeture de l'écran de chargement et l'ouverture de la fenêtre principale se fassent sur le thread UI.
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // Open MainWindow (before in order to not close the app) and next close the SplashScreen
                        desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
                        desktop.MainWindow.Show();
                        splashScreen.Close();
                    });
                });
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}