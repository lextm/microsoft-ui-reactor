using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Kanban.Models;

namespace Kanban.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TaskItem> TodoTasks { get; } = new();
    public ObservableCollection<TaskItem> InProgressTasks { get; } = new();
    public ObservableCollection<TaskItem> DoneTasks { get; } = new();

    private int _todoCount;
    private int _inProgressCount;
    private int _doneCount;

    public int TodoCount
    {
        get => _todoCount;
        set => SetProperty(ref _todoCount, value);
    }

    public int InProgressCount
    {
        get => _inProgressCount;
        set => SetProperty(ref _inProgressCount, value);
    }

    public int DoneCount
    {
        get => _doneCount;
        set => SetProperty(ref _doneCount, value);
    }

    public MainViewModel()
    {
        SeedData();
        UpdateCounts();
    }

    private void SeedData()
    {
        AddTask(new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Design wireframes",
            Description = "Create wireframes for the new dashboard feature",
            Priority = Priority.High,
            Column = ColumnType.Todo,
            CreatedAt = DateTime.Now.AddDays(-3)
        });

        AddTask(new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Set up CI/CD pipeline",
            Description = "Configure GitHub Actions for automated builds and tests",
            Priority = Priority.Medium,
            Column = ColumnType.InProgress,
            CreatedAt = DateTime.Now.AddDays(-5)
        });

        AddTask(new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Write unit tests",
            Description = "Add unit tests for the authentication module",
            Priority = Priority.Low,
            Column = ColumnType.InProgress,
            CreatedAt = DateTime.Now.AddDays(-2)
        });

        AddTask(new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Fix login bug",
            Description = "Users unable to log in with SSO on Firefox",
            Priority = Priority.High,
            Column = ColumnType.Done,
            CreatedAt = DateTime.Now.AddDays(-7)
        });
    }

    public void AddTask(TaskItem task)
    {
        GetCollectionForColumn(task.Column).Add(task);
        UpdateCounts();
    }

    public void MoveTaskRight(TaskItem task)
    {
        var currentColumn = task.Column;
        ColumnType newColumn = currentColumn switch
        {
            ColumnType.Todo => ColumnType.InProgress,
            ColumnType.InProgress => ColumnType.Done,
            _ => currentColumn
        };

        if (newColumn != currentColumn)
        {
            GetCollectionForColumn(currentColumn).Remove(task);
            task.Column = newColumn;
            GetCollectionForColumn(newColumn).Add(task);
            UpdateCounts();
        }
    }

    public void MoveTaskLeft(TaskItem task)
    {
        var currentColumn = task.Column;
        ColumnType newColumn = currentColumn switch
        {
            ColumnType.InProgress => ColumnType.Todo,
            ColumnType.Done => ColumnType.InProgress,
            _ => currentColumn
        };

        if (newColumn != currentColumn)
        {
            GetCollectionForColumn(currentColumn).Remove(task);
            task.Column = newColumn;
            GetCollectionForColumn(newColumn).Add(task);
            UpdateCounts();
        }
    }

    public void DeleteTask(TaskItem task)
    {
        GetCollectionForColumn(task.Column).Remove(task);
        UpdateCounts();
    }

    public void UpdateTask(TaskItem task, string title, string description, Priority priority)
    {
        task.Title = title;
        task.Description = description;
        task.Priority = priority;
    }

    private ObservableCollection<TaskItem> GetCollectionForColumn(ColumnType column) => column switch
    {
        ColumnType.Todo => TodoTasks,
        ColumnType.InProgress => InProgressTasks,
        ColumnType.Done => DoneTasks,
        _ => TodoTasks
    };

    private void UpdateCounts()
    {
        TodoCount = TodoTasks.Count;
        InProgressCount = InProgressTasks.Count;
        DoneCount = DoneTasks.Count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
