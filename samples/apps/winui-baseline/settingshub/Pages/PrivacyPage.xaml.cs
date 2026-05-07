using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsHub.ViewModels;

namespace SettingsHub.Pages;

public sealed partial class PrivacyPage : Page
{
    public SettingsViewModel ViewModel => SettingsViewModel.Instance;

    public PrivacyPage()
    {
        InitializeComponent();
    }

    private async void ClearDataButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Clear All Data",
            Content = "This will reset all settings to their defaults. This action cannot be undone.",
            PrimaryButtonText = "Clear",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.ClearAllData();
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetPrivacy();
    }
}
