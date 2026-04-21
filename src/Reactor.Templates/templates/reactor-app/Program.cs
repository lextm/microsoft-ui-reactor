using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

ReactorApp.Run<App>("Reactor.AppTemplate", width: 900, height: 600);

class App : Component
{
    public override Element Render()
    {
        var (name, setName) = UseState("World");

        var titleBar = TitleBar("Reactor.AppTemplate").Flex(shrink: 0);

        var content = Border(
            VStack(12,
                Heading($"Hello, {name}!"),
                TextField(name, setName, placeholder: "Your name")
            )
        ).Padding(24);

        return FlexColumn(titleBar, content);
    }
}
