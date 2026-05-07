using System.Collections.ObjectModel;
using ApiDash.Models;
using ApiDash.Services;

namespace ApiDash.ViewModels;

public class PostDetailViewModel : ViewModelBase
{
    private readonly ApiService _api = new();

    private Post? _post;
    public Post? Post { get => _post; set => SetProperty(ref _post, value); }

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    private bool _hasError;
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }

    private string _errorMessage = string.Empty;
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    public ObservableCollection<Comment> Comments { get; } = [];

    public async Task LoadAsync(int postId)
    {
        IsLoading = true;
        HasError = false;
        Comments.Clear();

        try
        {
            Post = await _api.GetPostAsync(postId);
            var comments = await _api.GetCommentsAsync(postId);
            foreach (var c in comments) Comments.Add(c);
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
}
