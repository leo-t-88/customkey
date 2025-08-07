using System.ComponentModel;
using System.Threading.Tasks;

namespace CustomKey.ViewModels
{
    public class LoadScreenViewModel : INotifyPropertyChanged
    {
        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LoadScreenViewModel()
        {
            LoadingText = "Loading...";
        }

        public async Task SimulateLoadingAsync()
        {
            await Task.Delay(500);
            LoadingText = "Launching...";
        }
    }
}
