using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kanban.Models;

public enum Priority { Low, Medium, High }

public enum ColumnType { Todo, InProgress, Done }

public class TaskItem : INotifyPropertyChanged
{
    private string _id = string.Empty;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private Priority _priority;
    private ColumnType _column;
    private DateTime _createdAt;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public Priority Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    public ColumnType Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
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
