using System.Linq;
using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string[] LayoutNames { get; set; }
        private string[] _layoutFiles;
        private int _selectedLayoutIndex;
        
        public MainWindowViewModel()
        {
            Utility.GlobalRefresh += () => OnPropertyChanged(string.Empty);
            ReloadLayouts();
        }

        public int SelectedLayoutIndex {
            get => _selectedLayoutIndex;
            set
            { 
                if (_selectedLayoutIndex != value) {
                    this.RaiseAndSetIfChanged(ref _selectedLayoutIndex, value);
                    if (value >= 0 && value < _layoutFiles.Length)
                    {
                        string file = _layoutFiles[value];
                        LayoutLoader.LoadLayoutFromFile(file);
                    }
                    OnPropertyChanged(string.Empty);
                }
            }
        }
        
        public void ReloadLayouts()
        {
            // Loads the names of the layouts (the “Name” property of the JSON files)
            var layouts = LayoutLoader.GetLayouts().OrderBy(kv => kv.Key).ToArray();

            LayoutNames = layouts.Select(kv => kv.Key).ToArray();
            _layoutFiles = layouts.Select(kv => kv.Value).ToArray();

            if (LayoutNames.Length > 0)
            {
                SelectedLayoutIndex = 0;
                LayoutLoader.LoadLayoutFromFile(_layoutFiles[0]);
                OnPropertyChanged(string.Empty);
            }
        }


        public string VersionNumber => "Version " + Utility.GetAssemblyVersion();
        
        public bool InputOn
        {
            get => Utility.IsInputEnabled;
            set => Utility.IsInputEnabled = value;
        }
    }
}