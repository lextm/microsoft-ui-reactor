// API Dashboard — Reactor sample demonstrating async data patterns.
// Fetches posts and comments from JSONPlaceholder (https://jsonplaceholder.typicode.com).

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Hooks;
using Microsoft.UI.Reactor.Navigation;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static Microsoft.UI.Reactor.Core.Theme;

namespace ApiDash;

// ═══════════════════════════════════════════════════════════════════
//  Data models
// ═══════════════════════════════════════════════════════════════════

record Post(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body);

record Comment(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("postId")] int PostId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("body")] string Body);

record NewPost(string Title, string Body);

// ═══════════════════════════════════════════════════════════════════
//  API client (string cursors for pagination compatibility)
// ═══════════════════════════════════════════════════════════════════

static class Api
{
    private static readonly HttpClient Http = new() { BaseAddress = new Uri("https://jsonplaceholder.typicode.com") };
    private const int PageSize = 10;

    public static async Task<Page<Post, string>> GetPostsPageAsync(string? cursor, CancellationToken ct)
    {
        int start = cursor is null ? 0 : int.Parse(cursor);
        var posts = await Http.GetFromJsonAsync<List<Post>>(
            $"/posts?_start={start}&_limit={PageSize}", ct) ?? [];
        string? next = posts.Count < PageSize ? null : (start + PageSize).ToString();
        return new Page<Post, string>(posts, next, 100);
    }

    public static async Task<Post> GetPostAsync(int id, CancellationToken ct) =>
        await Http.GetFromJsonAsync<Post>($"/posts/{id}", ct)
        ?? throw new InvalidOperationException($"Post {id} not found");

    public static async Task<List<Comment>> GetCommentsAsync(int postId, CancellationToken ct) =>
        await Http.GetFromJsonAsync<List<Comment>>($"/posts/{postId}/comments", ct) ?? [];

    public static async Task<Post> CreatePostAsync(NewPost input, CancellationToken ct)
    {
        var response = await Http.PostAsJsonAsync(
            "/posts", new { title = input.Title, body = input.Body, userId = 1 }, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Post>(ct)
            ?? throw new InvalidOperationException("Failed to create post");
    }

    public static async Task<bool> DeletePostAsync(int id, CancellationToken ct)
    {
        var response = await Http.DeleteAsync($"/posts/{id}", ct);
        response.EnsureSuccessStatusCode();
        return true;
    }
}

// ═══════════════════════════════════════════════════════════════════
//  Routes
// ═══════════════════════════════════════════════════════════════════

abstract record AppRoute;
sealed record PostListRoute : AppRoute;
sealed record PostDetailRoute(int PostId) : AppRoute;

// ═══════════════════════════════════════════════════════════════════
//  App shell — navigation host
// ═══════════════════════════════════════════════════════════════════

class App : Component
{
    public override Element Render()
    {
        var nav = UseNavigation<AppRoute>(new PostListRoute());

        return FlexColumn(
            // Title bar
            FlexRow(
                TextBlock("API Dashboard").FontSize(20).Bold()
                    .VAlign(VerticalAlignment.Center),
                TextBlock("JSONPlaceholder").FontSize(12)
                    .Foreground(SecondaryText).VAlign(VerticalAlignment.Center)
                    .Margin(12, 0, 0, 0)
            ).Padding(16).Background(CardBackground),

            // Navigation content
            NavigationHost(nav, route => route switch
            {
                PostListRoute => Component<PostListPage>(),
                PostDetailRoute detail => Component<PostDetailPage, int>(detail.PostId),
                _ => TextBlock("Unknown route"),
            }).Flex(grow: 1)
        );
    }
}

// ═══════════════════════════════════════════════════════════════════
//  Post list page — infinite scroll + create + delete
// ═══════════════════════════════════════════════════════════════════

class PostListPage : Component
{
    public override Element Render()
    {
        var nav = UseNavigation<AppRoute>();
        var (showCreate, setShowCreate) = UseState(false);
        var (deletedIds, setDeletedIds) = UseState<IReadOnlyList<int>>(Array.Empty<int>());
        var (createdPosts, setCreatedPosts) = UseState<IReadOnlyList<Post>>(Array.Empty<Post>());
        var (confirmDeleteId, setConfirmDeleteId) = UseState<int?>(null);

        var posts = UseInfiniteResource<Post, string>(
            fetchPage: Api.GetPostsPageAsync,
            deps: new object[] { "posts" });

        var createMutation = UseMutation<NewPost, Post>(
            mutator: Api.CreatePostAsync,
            options: new MutationOptions<NewPost, Post>(
                OnSuccess: (post, _) =>
                {
                    setCreatedPosts([.. createdPosts, post]);
                    setShowCreate(false);
                }));

        var deleteMutation = UseMutation<int, bool>(
            mutator: (id, ct) => Api.DeletePostAsync(id, ct),
            options: new MutationOptions<int, bool>(
                OnOptimistic: id => setDeletedIds([.. deletedIds, id]),
                OnError: (_, id) => setDeletedIds([.. deletedIds.Where(x => x != id)])));

        // Build visible items from loaded pages, filtering deleted
        var visibleItems = posts.Items
            .Where(p => p is not null && !deletedIds.Contains(p.Id))
            .Cast<Post>()
            .ToList();

        // Prepend optimistically created posts
        var allPosts = createdPosts.Concat(visibleItems).ToList();

        var isLoading = posts.LoadState is LoadState.Loading;
        var isError = posts.LoadState is LoadState.Error;
        var errorMessage = (posts.LoadState as LoadState.Error)?.Exception.Message ?? "";

        // Build list content items
        var listItems = new List<Element>();
        foreach (var post in allPosts)
            listItems.Add(PostCard(post, nav, setConfirmDeleteId));

        if (posts.HasMore)
            listItems.Add(LoadMoreButton(posts));
        else if (posts.LoadState is LoadState.Error)
            listItems.Add(ErrorCard(errorMessage, () => posts.Retry()));
        else
            listItems.Add(TextBlock("— End of posts —")
                .Foreground(TertiaryText).HAlign(HorizontalAlignment.Center).Margin(16));

        return FlexColumn(
            // Toolbar
            FlexRow(
                Heading("Posts"),
                Button("+ New Post", () => setShowCreate(true)).Margin(12, 0, 0, 0)
            ).Padding(16, 12, 16, 8),

            // Content
            isLoading && allPosts.Count == 0
                ? (Element)FlexColumn(
                    ProgressRing().Width(32).Height(32).Margin(24),
                    TextBlock("Loading posts…").Foreground(SecondaryText)
                        .HAlign(HorizontalAlignment.Center)
                  ).HAlign(HorizontalAlignment.Center).Padding(48)
                : isError && allPosts.Count == 0
                    ? ErrorCard(errorMessage, () => posts.Retry())
                    : ScrollView(
                        FlexColumn(listItems.ToArray()).Padding(16)
                      ).Flex(grow: 1),

            // Create dialog
            CreatePostDialog(showCreate, setShowCreate, createMutation),

            // Delete confirmation dialog
            DeleteConfirmDialog(confirmDeleteId, setConfirmDeleteId, deleteMutation)
        );
    }

    private static Element PostCard(Post post, NavigationHandle<AppRoute> nav, Action<int?> onDelete)
    {
        return Border(
            FlexColumn(
                FlexRow(
                    TextBlock($"#{post.Id}").Foreground(SecondaryText).FontSize(12),
                    TextBlock($"User {post.UserId}").Foreground(TertiaryText).FontSize(12)
                        .Margin(8, 0, 0, 0)
                ),
                TextBlock(Capitalize(post.Title)).Bold().FontSize(14)
                    .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                TextBlock(Truncate(post.Body, 120)).Foreground(SecondaryText).FontSize(12)
                    .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                FlexRow(
                    Button("View Details", () => nav.Navigate(new PostDetailRoute(post.Id))),
                    Button("Delete", () => onDelete(post.Id)).Margin(8, 0, 0, 0)
                ).Margin(0, 8, 0, 0)
            ).Padding(12) with { RowGap = 4 }
        ).Background(CardBackground).WithBorder(CardStroke, 1).CornerRadius(8)
         .Margin(0, 0, 0, 8).WithKey($"post-{post.Id}");
    }

    private static Element LoadMoreButton(InfiniteResource<Post> posts)
    {
        var isLoadingMore = posts.LoadState is LoadState.Loading;
        return FlexColumn(
            isLoadingMore
                ? (Element)FlexRow(
                    ProgressRing().Width(20).Height(20),
                    TextBlock("Loading more…").Foreground(SecondaryText)
                        .Margin(8, 0, 0, 0).VAlign(VerticalAlignment.Center)
                  ).HAlign(HorizontalAlignment.Center).Margin(12)
                : Button("Load More", () => posts.FetchNext())
                    .HAlign(HorizontalAlignment.Center).Margin(12)
        );
    }

    private static Element ErrorCard(string message, Action onRetry) =>
        Border(
            FlexColumn(
                TextBlock("Something went wrong").Bold().Foreground(SystemCritical),
                TextBlock(message).Foreground(SecondaryText).FontSize(12)
                    .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                Button("Retry", onRetry).Margin(0, 8, 0, 0)
            ).Padding(12) with { RowGap = 4 }
        ).Background(SystemCriticalBackground).WithBorder(SystemCritical, 1)
         .CornerRadius(8).Margin(0, 0, 0, 8);

    private static Element CreatePostDialog(bool isOpen, Action<bool> setOpen,
        Mutation<NewPost, Post> mutation)
    {
        return ContentDialog(
            "Create New Post",
            Component<CreatePostForm>(),
            primaryButtonText: "Create"
        ) with
        {
            IsOpen = isOpen,
            CloseButtonText = "Cancel",
            OnClosed = result =>
            {
                if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    var title = CreatePostForm.CurrentTitle;
                    var body = CreatePostForm.CurrentBody;
                    if (!string.IsNullOrWhiteSpace(title))
                        _ = mutation.RunAsync(new NewPost(title, body));
                }
                setOpen(false);
            }
        };
    }

    private static Element DeleteConfirmDialog(int? postId, Action<int?> setPostId,
        Mutation<int, bool> mutation)
    {
        return ContentDialog(
            "Delete Post",
            TextBlock(postId.HasValue
                ? $"Are you sure you want to delete post #{postId}?"
                : ""),
            primaryButtonText: "Delete"
        ) with
        {
            IsOpen = postId.HasValue,
            CloseButtonText = "Cancel",
            OnClosed = result =>
            {
                if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && postId.HasValue)
                    _ = mutation.RunAsync(postId.Value);
                setPostId(null);
            }
        };
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

// ═══════════════════════════════════════════════════════════════════
//  Create post form (embedded in dialog)
// ═══════════════════════════════════════════════════════════════════

class CreatePostForm : Component
{
    // Static fields for cross-component data sharing with the dialog callback.
    // In a real app, use a context or state management solution.
    internal static string CurrentTitle = "";
    internal static string CurrentBody = "";

    public override Element Render()
    {
        var (title, setTitle) = UseState("");
        var (body, setBody) = UseState("");

        UseEffect(() =>
        {
            CurrentTitle = title;
            CurrentBody = body;
        }, title, body);

        return FlexColumn(
            TextField(title, v => setTitle(v), placeholder: "Enter post title", header: "Title"),
            TextField(body, v => setBody(v), placeholder: "Enter post body", header: "Body")
        ) with { RowGap = 12 };
    }
}

// ═══════════════════════════════════════════════════════════════════
//  Post detail page — post content + comments
// ═══════════════════════════════════════════════════════════════════

class PostDetailPage : Component<int>
{
    public override Element Render()
    {
        var postId = Props;
        var nav = UseNavigation<AppRoute>();

        var post = UseResource(
            ct => Api.GetPostAsync(postId, ct),
            deps: new object[] { postId });

        var comments = UseResource(
            ct => Api.GetCommentsAsync(postId, ct),
            deps: new object[] { $"comments-{postId}" });

        return FlexColumn(
            // Back button
            FlexRow(
                Button("← Back", () => nav.GoBack())
            ).Padding(16, 12, 16, 8),

            // Post content
            ScrollView(
                FlexColumn(
                    PostContent(post),
                    CommentsSection(comments)
                ).Padding(16) with { RowGap = 16 }
            ).Flex(grow: 1)
        );
    }

    private static Element PostContent(AsyncValue<Post> post) =>
        post.Match<Element>(
            loading: () => FlexColumn(
                ProgressRing().Width(32).Height(32)
                    .HAlign(HorizontalAlignment.Center),
                TextBlock("Loading post…").Foreground(SecondaryText)
                    .HAlign(HorizontalAlignment.Center)
            ).Padding(24),
            data: p => Border(
                FlexColumn(
                    FlexRow(
                        TextBlock($"Post #{p.Id}").Foreground(SecondaryText).FontSize(12),
                        TextBlock($"by User {p.UserId}").Foreground(TertiaryText).FontSize(12)
                            .Margin(8, 0, 0, 0)
                    ),
                    Heading(Capitalize(p.Title))
                        .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                    TextBlock(p.Body).FontSize(14)
                        .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap)
                ).Padding(16) with { RowGap = 8 }
            ).Background(CardBackground).WithBorder(CardStroke, 1).CornerRadius(8),
            error: ex => ErrorCard($"Failed to load post: {ex.Message}"),
            reloading: p => Border(
                FlexColumn(
                    FlexRow(
                        TextBlock($"Post #{p.Id}").Foreground(SecondaryText).FontSize(12),
                        ProgressRing().Width(16).Height(16).Margin(8, 0, 0, 0)
                    ),
                    Heading(Capitalize(p.Title))
                        .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                    TextBlock(p.Body).FontSize(14)
                        .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap)
                ).Padding(16) with { RowGap = 8 }
            ).Background(CardBackground).WithBorder(CardStroke, 1).CornerRadius(8).Opacity(0.7));

    private static Element CommentsSection(AsyncValue<List<Comment>> comments) =>
        FlexColumn(
            SubHeading("Comments"),
            comments.Match<Element>(
                loading: () => FlexColumn(
                    ProgressRing().Width(24).Height(24)
                        .HAlign(HorizontalAlignment.Center),
                    TextBlock("Loading comments…").Foreground(SecondaryText)
                        .HAlign(HorizontalAlignment.Center)
                ).Padding(16),
                data: list => list.Count == 0
                    ? TextBlock("No comments yet.").Foreground(SecondaryText).Padding(8)
                    : FlexColumn(
                        list.Select(CommentCard).ToArray()
                      ) with { RowGap = 8 },
                error: ex => ErrorCard($"Failed to load comments: {ex.Message}"))
        ) with { RowGap = 8 };

    private static Element CommentCard(Comment c) =>
        Border(
            FlexColumn(
                FlexRow(
                    TextBlock(c.Name).Bold().FontSize(13)
                        .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap),
                    TextBlock(c.Email).Foreground(Accent).FontSize(11)
                        .Margin(8, 0, 0, 0).VAlign(VerticalAlignment.Center)
                ),
                TextBlock(c.Body).FontSize(12).Foreground(SecondaryText)
                    .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap)
            ).Padding(10) with { RowGap = 4 }
        ).Background(SubtleFill).WithBorder(DividerStroke, 1).CornerRadius(6)
         .WithKey($"comment-{c.Id}");

    private static Element ErrorCard(string message) =>
        Border(
            FlexColumn(
                TextBlock("Error").Bold().Foreground(SystemCritical),
                TextBlock(message).Foreground(SecondaryText).FontSize(12)
                    .Set(tb => tb.TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap)
            ).Padding(12) with { RowGap = 4 }
        ).Background(SystemCriticalBackground).WithBorder(SystemCritical, 1).CornerRadius(8);

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}

// ═══════════════════════════════════════════════════════════════════
//  Entry point
// ═══════════════════════════════════════════════════════════════════

class Program
{
    [STAThread]
    static void Main() => ReactorApp.Run<App>("API Dashboard", 900, 650);
}
