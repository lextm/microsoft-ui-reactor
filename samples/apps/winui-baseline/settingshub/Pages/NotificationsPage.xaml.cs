using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsHub.ViewModels;

namespace SettingsHub.Pages;

public sealed partial class NotificationsPage : Page
{
    public SettingsViewModel ViewModel => SettingsViewModel.Instance;

    public NotificationsPage()
    {
        InitializeComponent();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetNotifications();
    }
}
