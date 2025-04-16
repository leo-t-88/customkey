using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string[] LayoutNames { get; }

        private string _selectedLayout;
        private string _versionNumber;

        public bool IsShift { get; set; } = false;

        public MainWindowViewModel()
        {
            // Charge les noms des layouts (propriété "Name" des fichiers JSON)
            LayoutNames = LayoutLoader.GetLayoutNames();
            if (LayoutNames.Length > 0)
            {
                SelectedLayout = LayoutNames[0]; // Initialise avec le premier layout
            }

            VersionNumber = "Version " + Utility.GetAssemblyVersion();
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
                        LayoutInit.LoadLayoutFromFile(LayoutLoader.GetJsonFileName(value));
                    }
                    OnPropertyChanged("");
                }
            }
        }

        public string VersionNumber
        {
            get => _versionNumber;
            set
            {
                _versionNumber = value;
                OnPropertyChanged(nameof(VersionNumber));
            }
        }

        // Key Bindings for Keyboard
        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key)) return "";
                return key.EndsWith("Shift")
                    ? (IsShift ? "" : LayoutInit.KeyVal[key.Replace("Shift", "")].shiftChar)
                    : LayoutInit.GetChar(key, IsShift);
            }
        }
    }
}