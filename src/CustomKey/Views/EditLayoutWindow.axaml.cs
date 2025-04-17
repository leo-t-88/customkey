using Avalonia.Controls;
using Avalonia.Threading;
using SharpHook;
using System;

namespace CustomKey.Views
{
    public partial class EditLayoutWindow : Window
    {
        private string _keyId;
        private TextBlock _keyPressedTextBlock;
        //private IGlobalHook _hook;

        public EditLayoutWindow()
        {
            InitializeComponent();
            this.CanResize = false;

            _keyPressedTextBlock = this.FindControl<TextBlock>("KeyPressedTextBlock");

            //SetupGlobalHook();
        }

        /*private void SetupGlobalHook()
        {
            _hook = new SimpleGlobalHook();
            _hook.KeyPressed += HookOnKeyPressed;
            _hook.RunAsync(); // Lance le hook en async
        }*/

        private void HookOnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            // e.Data.KeyCode contient l’identifiant de la touche
            _keyId = e.Data.KeyCode.ToString();

            // Mise à jour de l'UI depuis le thread principal
            Dispatcher.UIThread.Post(() =>
            {
                if (_keyPressedTextBlock != null)
                {
                    _keyPressedTextBlock.Text = $"Key pressed ID: {_keyId}";
                }
            });
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

        /*protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _hook?.Dispose(); // Libération du hook
        }*/
    }
}