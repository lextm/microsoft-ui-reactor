using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Layout;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;

namespace Kanban;

// ── Column props + component ────────────────────────────────────────

public record KanbanColumnProps(string ColumnId, string Label, IReadOnlyList<KanbanCard> Cards);

sealed class KanbanColumn : Component<KanbanColumnProps>
{
    public override Element Render()
    {
        var dispatch = UseContext(BoardContext.Dispatch);
        var (isDragOver, setDragOver) = UseState(false);

        var columnId = Props.ColumnId;
        var cards = Props.Cards;

        var headerColor = columnId switch
        {
            Columns.ToDo => "#4A90D9",
            Columns.InProgress => "#E8943A",
            Columns.Done => "#5CB85C",
            _ => "#888888",
        };

        return FlexColumn(
            // Column header
            (FlexRow(
                TextBlock(Props.Label).Bold().FontSize(16)
                    .Foreground("#FFFFFF"),
                TextBlock($"{cards.Count}").FontSize(14)
                    .Foreground("#FFFFFF")
                    .Opacity(0.8)
            ) with { JustifyContent = FlexJustify.SpaceBetween, AlignItems = FlexAlign.Center })
            .Background(headerColor)
            .Padding(12, 8)
            .CornerRadius(8, 8, 0, 0),

            // Card list (scrollable drop target)
            ScrollView(
                (FlexColumn(
                    ForEach(cards, card =>
                        Component<CardView, KanbanCard>(card).WithKey(card.Id)
                    )
                ) with { RowGap = 8 })
                .Padding(8)
            )
            .OnDrop<ScrollViewElement, string>(cardId =>
            {
                dispatch(new MoveCard(cardId, columnId));
                setDragOver(false);
            })
            .OnDragEnter(_ => setDragOver(true))
            .OnDragLeave(_ => setDragOver(false)),

            // Add button
            Button("+ Add Card", () => dispatch(new OpenDialog(columnId)))
                .HAlign(HorizontalAlignment.Stretch)
                .Margin(8)
        )
        .WithBorder(isDragOver ? "#0078D4" : "#E0E0E0", isDragOver ? 2 : 1)
        .CornerRadius(8)
        .Background(isDragOver ? "#F0F6FF" : "#FAFAFA")
        .MinHeight(300)
        .Width(300);
    }
}

// ── Card props + component ──────────────────────────────────────────

sealed class CardView : Component<KanbanCard>
{
    public override Element Render()
    {
        var dispatch = UseContext(BoardContext.Dispatch);
        var card = Props;

        var snippet = card.Description.Length > 60
            ? card.Description[..60] + "…"
            : card.Description;

        return MenuFlyout(
            (FlexColumn(
                TextBlock(card.Title).Bold().FontSize(14),
                TextBlock(snippet).FontSize(12).Opacity(0.7)
            ) with { RowGap = 4 })
            .Padding(12)
            .Background("#FFFFFF")
            .CornerRadius(6)
            .WithBorder("#E0E0E0")
            .OnDragStart<FlexElement, string>(() => card.Id),

            MenuItem("Edit", () => dispatch(new OpenDialog(card.ColumnId, card.Id))),
            MenuItem("Delete", () => dispatch(new DeleteCard(card.Id)))
        );
    }
}
