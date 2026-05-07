# API Dashboard

A Reactor sample demonstrating async data patterns against the
[JSONPlaceholder](https://jsonplaceholder.typicode.com) REST API.

## What It Does
- Paginated post list with "Load More" infinite scrolling
- Post detail view with comments (separate async fetch)
- Create new posts via ContentDialog form
- Delete posts with confirmation dialog
- Loading spinners, error cards with retry, and back navigation

## Reactor Features Exercised
| Feature | Usage |
|---|---|
| `UseInfiniteResource` | Paginated post list with page cursors |
| `UseResource` | Post detail + comments fetching |
| `UseMutation` | Create and delete with optimistic UI |
| `AsyncValue` matching | Loading / data / error / reloading states |
| `UseNavigation` | List ↔ detail route navigation |
| `ContentDialog` | Create form and delete confirmation |
| `Pending` | Bubble-up loading fallback |
| Theme tokens | Card backgrounds, accent colors, text styles |

## Build & Run
```
dotnet build samples/apps/apidash/ApiDash.csproj
dotnet run --project samples/apps/apidash/ApiDash.csproj
```

## Build Metrics

| Metric | Value |
|---|---|
| **Agent model** | `claude-opus-4.6` |
| **Agent session** | Fresh (isolated sub-agent, no shared context) |
| **Input tokens** | 1,280,288 |
| **Output tokens** | 6,526 |
| **Total tokens** | 1,286,814 |
| **Peak context window** | 135,170 tokens |
| **Turns to completion** | 15 |
| **Wall-clock time** | 13 min 36 sec |
| **First-compile success** | No |
| **Compile errors fixed** | 16 |
| **Build → fix cycles** | 2 |
| **First-run success** | _(manual)_ |
| **Runtime errors** | _(manual)_ |
| **Human interventions** | 0 |
| **Feature completeness** | 100% — all planned features delivered |
| **Lines of code** | 361 |
| **Source files** | 1 (`App.cs`) |
