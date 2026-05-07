using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsHub.Pages;

namespace SettingsHub;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            var pageType = tag switch
            {
                "General" => typeof(GeneralPage),
                "Appearance" => typeof(AppearancePage),
                "Notifications" => typeof(NotificationsPage),
                "Privacy" => typeof(PrivacyPage),
                "About" => typeof(AboutPage),
                _ => typeof(GeneralPage)
            };
            ContentFrame.Navigate(pageType);
        }
    }
}
