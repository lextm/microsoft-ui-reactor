# Kanban Board

A Reactor sample app modeling a Kanban workflow with draggable task cards, reducer-driven state, and composable column/card components.

## Features

- Three columns: To Do, In Progress, Done
- Typed drag-and-drop between columns with hover feedback
- Add/edit cards via `ContentDialog`
- Right-click context menu for edit/delete
- Reducer-driven board state
- Context-provided dispatch for deep component access
- Memo-friendly card components with keyed lists

## Reactor APIs Exercised

- `UseReducer` — board state (add, move, edit, delete)
- `UseContext` / `.Provide()` — dispatch sharing
- `OnDragStart<T, TPayload>` / `OnDrop<T, TPayload>` — typed drag-drop
- `OnDragEnter` / `OnDragLeave` — hover feedback
- `ForEach(...).WithKey(...)` — stable card lists
- `Component<T, P>` — memo-friendly card rendering
- `ContentDialog` — add/edit dialogs
- `MenuFlyout` — context menu
- `FlexRow` / `FlexColumn` — layout
- Theme tokens

## Project Structure

- `App.cs` — entry point
- `src/BoardModels.cs` — models + action definitions
- `src/BoardReducer.cs` — reducer logic
- `src/KanbanApp.cs` — main shell + dialog
- `src/BoardComponents.cs` — column + card components

## Build

```sh
dotnet build samples/apps/kanban/Kanban.csproj -c Debug
```

## AI Build Metrics

| Metric | Value |
|---|---|
| Agent model | claude-opus-4.6 (Anthropic), premium, 200K context |
| Agent session | Fresh (no shared context) |
| Total tokens (in + out) | 1,651,184 |
| Input tokens | 1,638,180 |
| Output tokens | 13,004 |
| Peak context (single turn) | 129,944 |
| Turns to completion | 19 |
| Wall-clock time | 6 min 21 s |
| First-compile success | ❌ No |
| Compile errors fixed | 13 |
| Build→fix cycles | 3 |
| First-run success | ⏳ Not yet validated |
| Runtime errors | ⏳ Not yet validated |
| Human interventions | 0 |
| Feature completeness | 100% |
| Lines of C# | 271 |
| Source files | 6 |
