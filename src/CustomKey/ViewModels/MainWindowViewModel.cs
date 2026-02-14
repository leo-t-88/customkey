using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string[] LayoutNames { get; }
        private string _selectedLayout = "";
        
        public MainWindowViewModel()
        {
            // Loads the names of the layouts (the “Name” property of the JSON files)
            LayoutNames = LayoutLoader.GetLayoutNames();
            if (LayoutNames.Length > 0)
            {
                SelectedLayout = LayoutNames[0];
            }
            
            Utility.GlobalRefresh += () => OnPropertyChanged(string.Empty);
        }

        public string SelectedLayout
        {
            get => _selectedLayout;
            set
            {
                if (_selectedLayout != value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedLayout, value);
                    if (!string.IsNullOrEmpty(LayoutLoader.GetJsonFileName(value)))
                    {
                        string? file = LayoutLoader.GetJsonFileName(value);
                        if (file != null) LayoutLoader.LoadLayoutFromFile(file);
                    }
                    OnPropertyChanged(string.Empty); // Update keys value
                }
            }
        }

        public string VersionNumber => "Version " + Utility.GetAssemblyVersion();
        
        public bool InputOn
        {
            get => Utility.IsInputEnabled;
            set
            {
                Utility.IsInputEnabled = value;
            }
        }
    }
}