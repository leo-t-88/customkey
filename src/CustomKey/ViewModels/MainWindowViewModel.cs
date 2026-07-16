using System;
using System.Linq;
using CustomKey.Common;

namespace CustomKey.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Utility.GlobalRefresh += () => OnPropertyChanged(string.Empty);
            ReloadLayouts();
        }
        
        private string[] _layoutNames = Array.Empty<string>();
        public string[] LayoutNames
        {
            get => _layoutNames;
            private set
            {
                _layoutNames = value;
                OnPropertyChanged(nameof(LayoutNames));
            }
        }
        private string[] _layoutFiles;
        private int _selectedLayoutIndex;

        public int SelectedLayoutIndex {
            get => _selectedLayoutIndex;
            set
            { 
                _selectedLayoutIndex = value;
                if (value >= 0 && value < _layoutFiles.Length) LayoutManager.LoadLayoutFromFile(_layoutFiles[value]);
                OnPropertyChanged(string.Empty);
            }
        }

        private bool _isInputOn = true;
        public bool IsInputOn
        {
            get => _isInputOn;
            private set
            {
                _isInputOn = value;
                Utility.IsInputEnabled = value;
            }
        }

        private bool _isEditMode = false;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode)
                {
                    LayoutManager.SaveCurrentLayout();
                    ReloadLayouts();
                }

                _isEditMode = value;
                IsInputOn = !value;

                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(IsInputOn));

                Utility.RaiseEditModeChanged();
            }
        }
        
        public string CurrentLayoutName
        {
            get => LayoutManager.GetCurrentLayoutName();
            set
            {
                LayoutManager.UpdateLayoutName(value);
                OnPropertyChanged(nameof(CurrentLayoutName));
            }
        }
        
        public void ReloadLayouts()
        {
            var layouts = LayoutManager.GetLayouts().OrderBy(kv => kv.Key).ToArray();

            LayoutNames = layouts.Select(kv => kv.Key).ToArray();
            _layoutFiles = layouts.Select(kv => kv.Value).ToArray();

            if (LayoutNames.Length == 0) return;
            
            int index = Array.IndexOf(_layoutFiles, LayoutManager.CurrentLayoutJson);
            if (index < 0) index = 0;
            SelectedLayoutIndex = index;

            OnPropertyChanged(string.Empty);
        }

        public string VersionNumber => "Version " + Utility.GetAssemblyVersion();
    }
}