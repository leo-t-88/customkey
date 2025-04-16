using Avalonia.Controls;
using CustomKey.ViewModels;

namespace CustomKey;

public partial class AboutDialog : UserControl
{
    public AboutDialog()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}