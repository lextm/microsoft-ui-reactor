using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ApiDash.ViewModels;

namespace ApiDash.Pages;

public sealed partial class PostDetailPage : Page
{
    private readonly PostDetailViewModel _vm = new();
    private int _postId;

    public PostDetailPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is int id)
        {
            _postId = id;
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        ErrorPanel.Visibility = Visibility.Collapsed;
        ContentPanel.Visibility = Visibility.Collapsed;

        await _vm.LoadAsync(_postId);

        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;

        if (_vm.HasError)
        {
            ErrorPanel.Visibility = Visibility.Visible;
            ErrorText.Text = _vm.ErrorMessage;
        }
        else
        {
            ContentPanel.Visibility = Visibility.Visible;
            PostTitle.Text = _vm.Post?.Title ?? "";
            PostBody.Text = _vm.Post?.Body ?? "";
            CommentsRepeater.ItemsSource = _vm.Comments;
        }
    }

    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }
}
