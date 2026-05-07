using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Layout;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;

namespace Kanban;

/// <summary>
/// Context key for sharing the dispatch function with nested components.
/// </summary>
public static class BoardContext
{
    public static readonly Context<Action<BoardAction>> Dispatch = new(_ => { });
}

/// <summary>
/// Root shell: owns the reducer, provides dispatch via context, renders columns + dialog.
/// </summary>
sealed class KanbanApp : Component
{
    static readonly BoardState InitialState = new(
    [
        new("seed1", "Design mockups", "Create wireframes for the new feature", Columns.ToDo),
        new("seed2", "Set up CI pipeline", "Configure GitHub Actions for build + test", Columns.ToDo),
        new("seed3", "Implement auth", "Add JWT-based authentication", Columns.InProgress),
        new("seed4", "Write unit tests", "Cover reducer and model logic", Columns.Done),
    ]);

    public override Element Render()
    {
        var (state, dispatch) = UseReducer<BoardState, BoardAction>(BoardReducer.Reduce, InitialState);

        return (FlexColumn(
            // Title bar
            TextBlock("Kanban Board").FontSize(28).Bold()
                .Margin(24, 16, 24, 8),

            // Columns row
            (FlexRow(
                Columns.All.Select(col =>
                    Component<KanbanColumn, KanbanColumnProps>(
                        new(col.Id, col.Label, state.Cards.Where(c => c.ColumnId == col.Id).ToList())
                    ).WithKey(col.Id)
                ).ToArray()
            ) with { AlignItems = FlexAlign.Stretch, ColumnGap = 12 }),

            // Add/Edit dialog
            RenderDialog(state, dispatch)
        ) with { RowGap = 8 })
        .Provide(BoardContext.Dispatch, dispatch)
        .FlexPadding(24);
    }

    static Element RenderDialog(BoardState state, Action<BoardAction> dispatch)
    {
        var isEditing = state.EditingCardId is not null;
        var title = isEditing ? "Edit Card" : "Add Card";
        var primaryText = isEditing ? "Save" : "Add";

        return ContentDialog(title,
            (FlexColumn(
                TextField(state.DialogTitle, v => dispatch(new SetDialogTitle(v)),
                    placeholder: "Card title", header: "Title")
                    .Width(360),
                TextField(state.DialogDescription, v => dispatch(new SetDialogDescription(v)),
                    placeholder: "Description", header: "Description")
                    .Width(360)
            ) with { RowGap = 12 })
        , primaryText) with
        {
            IsOpen = state.IsDialogOpen,
            CloseButtonText = "Cancel",
            OnClosed = result =>
            {
                if (result == ContentDialogResult.Primary && state.DialogTitle.Trim().Length > 0)
                {
                    if (state.EditingCardId is { } cardId)
                        dispatch(new EditCard(cardId, state.DialogTitle.Trim(), state.DialogDescription.Trim()));
                    else
                        dispatch(new AddCard(state.DialogTitle.Trim(), state.DialogDescription.Trim(), state.DialogColumnId));
                }
                else
                {
                    dispatch(new CloseDialog());
                }
            },
        };
    }
}
