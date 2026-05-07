using System.Collections.ObjectModel;
using ApiDash.Models;
using ApiDash.Services;

namespace ApiDash.ViewModels;

public class PostListViewModel : ViewModelBase
{
    private readonly ApiService _api = new();
    private const int PageSize = 10;
    private int _currentPage;

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    private bool _hasError;
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }

    private string _errorMessage = string.Empty;
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    private bool _canLoadMore = true;
    public bool CanLoadMore { get => _canLoadMore; set => SetProperty(ref _canLoadMore, value); }

    public ObservableCollection<Post> Posts { get; } = [];

    public async Task LoadInitialAsync()
    {
        _currentPage = 0;
        Posts.Clear();
        CanLoadMore = true;
        await LoadMoreAsync();
    }

    public async Task LoadMoreAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        HasError = false;

        try
        {
            var posts = await _api.GetPostsAsync(_currentPage * PageSize, PageSize);
            foreach (var p in posts) Posts.Add(p);
            _currentPage++;
            CanLoadMore = posts.Count == PageSize;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<Post?> CreatePostAsync(string title, string body)
    {
        var post = new Post { UserId = 1, Title = title, Body = body };
        var created = await _api.CreatePostAsync(post);
        if (created != null)
        {
            Posts.Insert(0, created);
        }
        return created;
    }

    public async Task<bool> DeletePostAsync(Post post)
    {
        try
        {
            await _api.DeletePostAsync(post.Id);
            Posts.Remove(post);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
