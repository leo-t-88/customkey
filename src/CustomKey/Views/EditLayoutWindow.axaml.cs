using Avalonia.Controls;
using Avalonia.Input;
using CustomKey.Common;

namespace CustomKey.Views
{
    public partial class EditLayoutWindow : Window
    {
        private string _keyId;

        public EditLayoutWindow()
        {
            InitializeComponent();
            this.KeyDown += OnKeyDown; // KeyDown event subscription
            
            Utility.IsInputEnabled = false;
            this.Closed += (sender, args) => Utility.IsInputEnabled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _keyId = LayoutLoader.ConvertToVcKey(e.Key.ToString());
            KeyPressedTextBlock.Text = $"Key pressed ID: {_keyId}";
        }

        private async void CopyKeyId(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_keyId))
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(_keyId);
                }
            }
        }
    }
}

/*  Just a reminder to how Indented JSON (for better reading for raw edition)
        in C# for when i will implement a real Keyboard disposition Editor

    var options = new JsonSerializerOptions { WriteIndented = true };
    string jsonString = JsonSerializer.Serialize(settings, options);
    File.WriteAllText(jsonFilePath, jsonString);*/