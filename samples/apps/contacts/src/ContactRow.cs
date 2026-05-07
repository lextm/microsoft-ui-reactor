using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;
using static Microsoft.UI.Reactor.Core.Theme;

namespace Contacts;

public record ContactRowProps(Contact Contact, bool IsSelected, Action OnSelect);

public class ContactRow : Component<ContactRowProps>
{
    public override Element Render()
    {
        var c = Props.Contact;
        var isSelected = Props.IsSelected;

        return (FlexColumn(
            TextBlock(c.Name).FontSize(14).FontWeight(new Windows.UI.Text.FontWeight(600)),
            TextBlock(c.Email).FontSize(12).Foreground(SecondaryText),
            TextBlock(c.Phone).FontSize(12).Foreground(TertiaryText)
        ) with { RowGap = 2 })
        .Padding(12, 8, 12, 8)
        .Background(isSelected ? Accent : CardBackground)
        .CornerRadius(6)
        .Set(el => el.PointerPressed += (_, _) => Props.OnSelect());
    }
}
