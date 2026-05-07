using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ApiDash.Models;
using ApiDash.ViewModels;

namespace ApiDash.Pages;

public sealed partial class PostListPage : Page
{
    private readonly PostListViewModel _vm = new();

    public PostListPage()
    {
        this.InitializeComponent();
        PostsListView.ItemsSource = _vm.Posts;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadPostsAsync();
    }

    private async Task LoadPostsAsync()
    {
        UpdateLoadingState(true);
        await _vm.LoadInitialAsync();
        UpdateLoadingState(false);
    }

    private async void LoadMore_Click(object sender, RoutedEventArgs e)
    {
        UpdateLoadingState(true);
        await _vm.LoadMoreAsync();
        UpdateLoadingState(false);
    }

    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        UpdateLoadingState(true);
        await _vm.LoadMoreAsync();
        UpdateLoadingState(false);
    }

    private void UpdateLoadingState(bool loading)
    {
        LoadingRing.IsActive = loading;
        LoadingRing.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = _vm.HasError ? Visibility.Visible : Visibility.Collapsed;
        ErrorText.Text = _vm.ErrorMessage;
        LoadMoreButton.Visibility = !loading && !_vm.HasError && _vm.CanLoadMore
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Post_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Post post)
        {
            Frame.Navigate(typeof(PostDetailPage), post.Id);
        }
    }

    private async void NewPost_Click(object sender, RoutedEventArgs e)
    {
        var titleBox = new TextBox { PlaceholderText = "Title", Margin = new Thickness(0, 0, 0, 8) };
        var bodyBox = new TextBox
        {
            PlaceholderText = "Body",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 120
        };
        var panel = new StackPanel();
        panel.Children.Add(titleBox);
        panel.Children.Add(bodyBox);

        var dialog = new ContentDialog
        {
            Title = "Create New Post",
            Content = panel,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            if (!string.IsNullOrWhiteSpace(titleBox.Text))
            {
                await _vm.CreatePostAsync(titleBox.Text, bodyBox.Text);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Post post)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Post",
                Content = $"Are you sure you want to delete \"{post.Title}\"?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await _vm.DeletePostAsync(post);
            }
        }
    }
}
