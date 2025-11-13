using Avalonia.Controls;
using CustomKey.ViewModels;

namespace CustomKey.Views;

public partial class AboutDialog : UserControl
{
    public AboutDialog()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}