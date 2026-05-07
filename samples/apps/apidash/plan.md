# API Dashboard Build Plan

## Goal
Build a Reactor sample app that exercises the async resource and navigation APIs
with a realistic dashboard workflow against `https://jsonplaceholder.typicode.com`.

## Scope
- Paginated post list with infinite scrolling
- Post detail route with comments
- Create and delete mutations with optimistic state
- Visual states for loading, reloading, and error handling
- Clean WinUI 3 dashboard presentation using Reactor theme tokens

## Architecture

### Entry shell
- `App.cs` hosts the entire sample.
- `ApiDashApp` owns app-level optimistic state and the typed navigation handle.
- `NavigationHost` switches between `PostList` and `PostDetail(int postId)` routes.

### Data layer
- A small `JsonPlaceholderApi` helper wraps `HttpClient`.
- Read operations are idempotent and accept `CancellationToken`.
- Write operations (`POST`, `DELETE`) are isolated behind `UseMutation` mutators.

### Shared app state
- Maintain local optimistic posts created by the user.
- Maintain deleted post IDs so removals update the list immediately.
- Pass state + updater delegates into list/detail pages through props.

### Post list page
- Wrap the async list subtree in `Pending` to show a dashboard fallback during the first load.
- Use `UseInfiniteResource` with pagination.
- Render rows with load-more button and prefetch.
- Overlay optimistic local posts above the fetched server pages.
- Show footer states for loading-more, retry, and end-of-list.
- Include a compose panel for creating posts.

### Post detail page
- Route parameter selects the post.
- Use `UseResource` for the post and comments separately.
- Use `AsyncValue<T>` pattern matching for loading, data, error, and reloading states.
- Expose refresh and retry flows.

### Mutations
- `UseMutation` for create: optimistic insert → success: replace temp → error: rollback
- `UseMutation` for delete: optimistic hide → success: keep deleted → error: restore

## UI composition
- Use FlexColumn, FlexRow, Border, ScrollView, and theme tokens.
- Card-style panels for list and detail screens.
- Loading spinners, error cards with retry button.
- Back navigation button on detail page.

## Verification
1. Create all required files under `samples/apps/apidash/`.
2. Build `ApiDash.csproj` in Debug.
3. Fix all compiler errors and warnings until the build is clean.
