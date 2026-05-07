# Kanban Board Build Plan

## Goal
Build a self-contained Reactor sample app that models a small Kanban workflow with draggable task cards, reducer-driven state, and composable column/card components.

## Core Behavior
1. Three columns: To Do, In Progress, Done.
2. Cards can be dragged between columns.
3. Add new cards via a dialog.
4. Edit/delete cards via right-click context menu.
5. Reducer manages all board state mutations.
6. Dispatch is shared via `UseContext`/`Provide`.

## Reactor Features To Exercise
- `UseReducer` for board state (add, move, edit, delete)
- `UseContext` / `.Provide(dispatch)` for deeply nested dispatch access
- `OnDragStart<T, TPayload>` / `OnDrop<T, TPayload>` for typed drag-drop
- `OnDragEnter` / `OnDragOver` / `OnDragLeave` for hover feedback
- `ForEach(...).WithKey(...)` for stable card lists
- `Component<T, P>` for memo-friendly card rendering
- `ContentDialog` for add/edit
- `OnRightTapped` + context flyout menu
- Grid + Flex layouts
- Theme tokens

## Data Model
- `KanbanCard`: Id, Title, Description, ColumnId
- `BoardState`: list of cards, dialog state
- Actions: AddCard, MoveCard, EditCard, DeleteCard, OpenDialog, CloseDialog

## UI Layout
- Three equal-width columns in a `FlexRow`
- Each column has a header + scrollable card list + "Add" button
- Cards show title + description snippet
- Drag feedback: column highlights on drag-over

## Verification
1. Build the new project in Debug configuration.
2. Fix any compile or warning issues.
3. Confirm the sample builds cleanly with 0 errors and 0 warnings.
