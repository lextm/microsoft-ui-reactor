using Microsoft.UI.Xaml;
using ApiDash.Pages;

namespace ApiDash;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        RootFrame.Navigate(typeof(PostListPage));
    }
}
