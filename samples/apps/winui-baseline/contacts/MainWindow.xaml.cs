using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Contacts.Models;
using Contacts.ViewModels;

namespace Contacts;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        this.InitializeComponent();
        ContactListView.ItemsSource = _viewModel.FilteredContacts;
        UpdateStatusBar();
        UpdateEmptyState();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        _viewModel.SearchText = sender.Text;
        UpdateStatusBar();
        UpdateEmptyState();
        UpdateDetailPanel();
    }

    private void ContactListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedContact = ContactListView.SelectedItem as Contact;
        UpdateDetailPanel();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var contact = new Contact();
        var saved = await ShowContactDialog("Add Contact", contact);
        if (saved)
        {
            _viewModel.AddContact(contact);
            UpdateStatusBar();
            UpdateEmptyState();
            ContactListView.SelectedItem = contact;
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedContact == null) return;
        await ShowContactDialog("Edit Contact", _viewModel.SelectedContact);
        _viewModel.RefreshFilter();
        UpdateDetailPanel();
        UpdateStatusBar();
        UpdateEmptyState();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedContact == null) return;

        var dialog = new ContentDialog
        {
            Title = "Delete Contact",
            Content = $"Are you sure you want to delete {_viewModel.SelectedContact.FullName}?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _viewModel.DeleteContact(_viewModel.SelectedContact);
            ContactListView.SelectedItem = null;
            UpdateStatusBar();
            UpdateEmptyState();
            UpdateDetailPanel();
        }
    }

    private async Task<bool> ShowContactDialog(string title, Contact contact)
    {
        var panel = new StackPanel { Spacing = 8, Width = 320 };

        var firstNameBox = new TextBox { Header = "First Name", Text = contact.FirstName };
        var lastNameBox = new TextBox { Header = "Last Name", Text = contact.LastName };
        var emailBox = new TextBox { Header = "Email", Text = contact.Email };
        var phoneBox = new TextBox { Header = "Phone", Text = contact.Phone };
        var companyBox = new TextBox { Header = "Company", Text = contact.Company };
        var notesBox = new TextBox
        {
            Header = "Notes", Text = contact.Notes,
            AcceptsReturn = true, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Height = 80
        };

        panel.Children.Add(firstNameBox);
        panel.Children.Add(lastNameBox);
        panel.Children.Add(emailBox);
        panel.Children.Add(phoneBox);
        panel.Children.Add(companyBox);
        panel.Children.Add(notesBox);

        var dialog = new ContentDialog
        {
            Title = title,
            Content = panel,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            contact.FirstName = firstNameBox.Text;
            contact.LastName = lastNameBox.Text;
            contact.Email = emailBox.Text;
            contact.Phone = phoneBox.Text;
            contact.Company = companyBox.Text;
            contact.Notes = notesBox.Text;
            return true;
        }
        return false;
    }

    private void UpdateDetailPanel()
    {
        var contact = _viewModel.SelectedContact;
        if (contact == null)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
            NoSelectionPanel.Visibility = Visibility.Visible;
            return;
        }

        NoSelectionPanel.Visibility = Visibility.Collapsed;
        DetailPanel.Visibility = Visibility.Visible;

        DetailInitials.Text = contact.Initials;
        DetailFullName.Text = contact.FullName;
        DetailCompany.Text = contact.Company;
        DetailEmail.Text = contact.Email;
        DetailPhone.Text = contact.Phone;
        DetailCompanyField.Text = contact.Company;
        DetailNotes.Text = contact.Notes;
    }

    private void UpdateStatusBar()
    {
        StatusText.Text = _viewModel.TotalCount == _viewModel.ContactCount
            ? $"{_viewModel.ContactCount} contacts"
            : $"{_viewModel.ContactCount} of {_viewModel.TotalCount} contacts";
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = _viewModel.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        ContactListView.Visibility = _viewModel.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
    }
}
