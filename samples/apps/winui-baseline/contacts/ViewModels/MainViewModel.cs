using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Contacts.Models;

namespace Contacts.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Contact> _allContacts = new();
    private Contact? _selectedContact;
    private string _searchText = string.Empty;

    public MainViewModel()
    {
        FilteredContacts = new ObservableCollection<Contact>();
        SeedData();
        ApplyFilter();
    }

    public ObservableCollection<Contact> FilteredContacts { get; }

    public Contact? SelectedContact
    {
        get => _selectedContact;
        set
        {
            if (_selectedContact == value) return;
            _selectedContact = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => _selectedContact != null;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public int ContactCount => FilteredContacts.Count;
    public int TotalCount => _allContacts.Count;
    public bool IsEmpty => FilteredContacts.Count == 0;

    public void AddContact(Contact contact)
    {
        _allContacts.Add(contact);
        ApplyFilter();
    }

    public void DeleteContact(Contact contact)
    {
        _allContacts.Remove(contact);
        if (SelectedContact == contact)
            SelectedContact = null;
        ApplyFilter();
    }

    public void RefreshFilter()
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredContacts.Clear();
        var query = _searchText.Trim();
        foreach (var c in _allContacts)
        {
            if (string.IsNullOrEmpty(query) ||
                c.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                FilteredContacts.Add(c);
            }
        }
        OnPropertyChanged(nameof(ContactCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void SeedData()
    {
        _allContacts.Add(new Contact
        {
            FirstName = "Alice", LastName = "Johnson",
            Email = "alice.johnson@example.com", Phone = "(555) 101-2001",
            Company = "Contoso Ltd", Notes = "Met at Build conference"
        });
        _allContacts.Add(new Contact
        {
            FirstName = "Bob", LastName = "Smith",
            Email = "bob.smith@example.com", Phone = "(555) 202-3002",
            Company = "Fabrikam Inc", Notes = "College friend"
        });
        _allContacts.Add(new Contact
        {
            FirstName = "Carol", LastName = "Williams",
            Email = "carol.w@example.com", Phone = "(555) 303-4003",
            Company = "Northwind Traders", Notes = "Project collaborator"
        });
        _allContacts.Add(new Contact
        {
            FirstName = "David", LastName = "Brown",
            Email = "david.brown@example.com", Phone = "(555) 404-5004",
            Company = "Adventure Works", Notes = "Mentor"
        });
        _allContacts.Add(new Contact
        {
            FirstName = "Eve", LastName = "Davis",
            Email = "eve.davis@example.com", Phone = "(555) 505-6005",
            Company = "Woodgrove Bank", Notes = "Former colleague"
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
