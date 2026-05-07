namespace Kanban;

public static class BoardReducer
{
    public static BoardState Reduce(BoardState state, BoardAction action) => action switch
    {
        AddCard a => state with
        {
            Cards = [.. state.Cards, new KanbanCard(
                Id: Guid.NewGuid().ToString("N")[..8],
                Title: a.Title,
                Description: a.Description,
                ColumnId: a.ColumnId)],
            IsDialogOpen = false,
            EditingCardId = null,
            DialogTitle = "",
            DialogDescription = "",
        },

        MoveCard m => state with
        {
            Cards = state.Cards
                .Select(c => c.Id == m.CardId ? c with { ColumnId = m.TargetColumnId } : c)
                .ToList(),
        },

        EditCard e => state with
        {
            Cards = state.Cards
                .Select(c => c.Id == e.CardId ? c with { Title = e.Title, Description = e.Description } : c)
                .ToList(),
            IsDialogOpen = false,
            EditingCardId = null,
            DialogTitle = "",
            DialogDescription = "",
        },

        DeleteCard d => state with
        {
            Cards = state.Cards.Where(c => c.Id != d.CardId).ToList(),
        },

        OpenDialog o => state with
        {
            IsDialogOpen = true,
            DialogColumnId = o.ColumnId,
            EditingCardId = o.EditCardId,
            DialogTitle = o.EditCardId is { } id
                ? state.Cards.First(c => c.Id == id).Title
                : "",
            DialogDescription = o.EditCardId is { } eid
                ? state.Cards.First(c => c.Id == eid).Description
                : "",
        },

        CloseDialog => state with
        {
            IsDialogOpen = false,
            EditingCardId = null,
            DialogTitle = "",
            DialogDescription = "",
        },

        SetDialogTitle t => state with { DialogTitle = t.Title },
        SetDialogDescription d => state with { DialogDescription = d.Description },

        _ => state,
    };
}
