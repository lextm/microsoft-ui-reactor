using System.Net.Http;
using System.Net.Http.Json;
using ApiDash.Models;

namespace ApiDash.Services;

public class ApiService
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
    };

    public async Task<List<Post>> GetPostsAsync(int skip, int take)
    {
        var posts = await _client.GetFromJsonAsync<List<Post>>(
            $"posts?_start={skip}&_limit={take}");
        return posts ?? [];
    }

    public async Task<Post?> GetPostAsync(int id)
    {
        return await _client.GetFromJsonAsync<Post>($"posts/{id}");
    }

    public async Task<List<Comment>> GetCommentsAsync(int postId)
    {
        var comments = await _client.GetFromJsonAsync<List<Comment>>(
            $"posts/{postId}/comments");
        return comments ?? [];
    }

    public async Task<Post?> CreatePostAsync(Post post)
    {
        var response = await _client.PostAsJsonAsync("posts", post);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Post>();
    }

    public async Task DeletePostAsync(int id)
    {
        var response = await _client.DeleteAsync($"posts/{id}");
        response.EnsureSuccessStatusCode();
    }
}
