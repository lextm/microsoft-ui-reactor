using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Kanban.Models;
using Kanban.ViewModels;

namespace Kanban;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Kanban Board";

        TodoList.ItemsSource = _viewModel.TodoTasks;
        InProgressList.ItemsSource = _viewModel.InProgressTasks;
        DoneList.ItemsSource = _viewModel.DoneTasks;

        _viewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.TodoCount):
                    TodoCountText.Text = _viewModel.TodoCount.ToString();
                    break;
                case nameof(MainViewModel.InProgressCount):
                    InProgressCountText.Text = _viewModel.InProgressCount.ToString();
                    break;
                case nameof(MainViewModel.DoneCount):
                    DoneCountText.Text = _viewModel.DoneCount.ToString();
                    break;
            }
        };

        // Initialize counts
        TodoCountText.Text = _viewModel.TodoCount.ToString();
        InProgressCountText.Text = _viewModel.InProgressCount.ToString();
        DoneCountText.Text = _viewModel.DoneCount.ToString();
    }

    private void PriorityIndicator_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Ellipse ellipse && ellipse.Tag is Priority priority)
        {
            ellipse.Fill = priority switch
            {
                Priority.High => new SolidColorBrush(Colors.Red),
                Priority.Medium => new SolidColorBrush(Colors.Gold),
                Priority.Low => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }

    private void AddTodoTask_Click(object sender, RoutedEventArgs e) => ShowAddDialog(ColumnType.Todo);
    private void AddInProgressTask_Click(object sender, RoutedEventArgs e) => ShowAddDialog(ColumnType.InProgress);
    private void AddDoneTask_Click(object sender, RoutedEventArgs e) => ShowAddDialog(ColumnType.Done);

    private async void ShowAddDialog(ColumnType column)
    {
        var titleBox = new TextBox { PlaceholderText = "Task title", Margin = new Thickness(0, 0, 0, 8) };
        var descBox = new TextBox { PlaceholderText = "Description", AcceptsReturn = true, Height = 80, Margin = new Thickness(0, 0, 0, 8) };
        var priorityCombo = new ComboBox { Header = "Priority", HorizontalAlignment = HorizontalAlignment.Stretch };
        priorityCombo.Items.Add("Low");
        priorityCombo.Items.Add("Medium");
        priorityCombo.Items.Add("High");
        priorityCombo.SelectedIndex = 0;

        var panel = new StackPanel();
        panel.Children.Add(titleBox);
        panel.Children.Add(descBox);
        panel.Children.Add(priorityCombo);

        var dialog = new ContentDialog
        {
            Title = "Add New Task",
            Content = panel,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(titleBox.Text))
        {
            var priority = priorityCombo.SelectedIndex switch
            {
                2 => Priority.High,
                1 => Priority.Medium,
                _ => Priority.Low
            };

            _viewModel.AddTask(new TaskItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = titleBox.Text,
                Description = descBox.Text ?? string.Empty,
                Priority = priority,
                Column = column,
                CreatedAt = DateTime.Now
            });
        }
    }

    private void EditTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
        {
            ShowEditDialog(task);
        }
    }

    private async void ShowEditDialog(TaskItem task)
    {
        var titleBox = new TextBox { Text = task.Title, Margin = new Thickness(0, 0, 0, 8) };
        var descBox = new TextBox { Text = task.Description, AcceptsReturn = true, Height = 80, Margin = new Thickness(0, 0, 0, 8) };
        var priorityCombo = new ComboBox { Header = "Priority", HorizontalAlignment = HorizontalAlignment.Stretch };
        priorityCombo.Items.Add("Low");
        priorityCombo.Items.Add("Medium");
        priorityCombo.Items.Add("High");
        priorityCombo.SelectedIndex = (int)task.Priority;

        var panel = new StackPanel();
        panel.Children.Add(titleBox);
        panel.Children.Add(descBox);
        panel.Children.Add(priorityCombo);

        var dialog = new ContentDialog
        {
            Title = "Edit Task",
            Content = panel,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(titleBox.Text))
        {
            var priority = priorityCombo.SelectedIndex switch
            {
                2 => Priority.High,
                1 => Priority.Medium,
                _ => Priority.Low
            };

            _viewModel.UpdateTask(task, titleBox.Text, descBox.Text ?? string.Empty, priority);
        }
    }

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
        {
            ShowDeleteConfirmation(task);
        }
    }

    private async void ShowDeleteConfirmation(TaskItem task)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete Task",
            Content = $"Are you sure you want to delete \"{task.Title}\"?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _viewModel.DeleteTask(task);
        }
    }

    private void MoveRight_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
        {
            _viewModel.MoveTaskRight(task);
        }
    }

    private void MoveLeft_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
        {
            _viewModel.MoveTaskLeft(task);
        }
    }
}
