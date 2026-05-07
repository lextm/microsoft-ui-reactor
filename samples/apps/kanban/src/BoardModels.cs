namespace Kanban;

// ── Data model ──────────────────────────────────────────────────────

public record KanbanCard(string Id, string Title, string Description, string ColumnId);

public record BoardState(
    IReadOnlyList<KanbanCard> Cards,
    bool IsDialogOpen = false,
    string? EditingCardId = null,
    string DialogTitle = "",
    string DialogDescription = "",
    string DialogColumnId = "todo"
);

// ── Column definitions ──────────────────────────────────────────────

public static class Columns
{
    public const string ToDo = "todo";
    public const string InProgress = "inprogress";
    public const string Done = "done";

    public static readonly IReadOnlyList<(string Id, string Label)> All =
    [
        (ToDo, "To Do"),
        (InProgress, "In Progress"),
        (Done, "Done"),
    ];
}

// ── Actions ─────────────────────────────────────────────────────────

public abstract record BoardAction;
public record AddCard(string Title, string Description, string ColumnId) : BoardAction;
public record MoveCard(string CardId, string TargetColumnId) : BoardAction;
public record EditCard(string CardId, string Title, string Description) : BoardAction;
public record DeleteCard(string CardId) : BoardAction;
public record OpenDialog(string ColumnId, string? EditCardId = null) : BoardAction;
public record CloseDialog : BoardAction;
public record SetDialogTitle(string Title) : BoardAction;
public record SetDialogDescription(string Description) : BoardAction;
