using Microsoft.UI.Reactor;

namespace Kanban;

class Program
{
    [STAThread]
    static void Main() => ReactorApp.Run<KanbanApp>("Kanban Board", 1050, 700);
}
