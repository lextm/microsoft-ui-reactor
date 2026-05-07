using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsHub.ViewModels;

namespace SettingsHub.Pages;

public sealed partial class AppearancePage : Page
{
    public SettingsViewModel ViewModel => SettingsViewModel.Instance;

    public AppearancePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set the selected radio button based on current theme
        foreach (var item in ThemeSelector.Items)
        {
            if (item is RadioButton rb && rb.Tag?.ToString() == ViewModel.Theme)
            {
                ThemeSelector.SelectedItem = rb;
                break;
            }
        }
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedItem is RadioButton rb)
        {
            ViewModel.Theme = rb.Tag?.ToString() ?? "System";
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetAppearance();
        OnLoaded(this, e);
    }

    public string FormatFontSize(double size) => $"Font Size: {size:F0}";
}
