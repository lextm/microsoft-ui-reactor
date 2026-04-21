using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

ReactorApp.Run<App>("Reactor.AppTemplate", width: 900, height: 600);

class App : Component
{
    public override Element Render()
    {
        var (name, setName) = UseState("World");

        return VStack(
            Heading($"Hello, {name}!"),
            TextField(name, setName, placeholder: "Your name")
        );
    }
}
