using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsHub.ViewModels;

namespace SettingsHub.Pages;

public sealed partial class GeneralPage : Page
{
    public SettingsViewModel ViewModel => SettingsViewModel.Instance;

    public GeneralPage()
    {
        InitializeComponent();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetGeneral();
    }
}
