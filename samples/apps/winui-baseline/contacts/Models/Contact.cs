using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Contacts.Models;

public class Contact : INotifyPropertyChanged
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _company = string.Empty;
    private string _notes = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Company
    {
        get => _company;
        set => SetProperty(ref _company, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials =>
        (FirstName.Length > 0 ? FirstName[..1] : "") +
        (LastName.Length > 0 ? LastName[..1] : "");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(FirstName) || propertyName == nameof(LastName))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Initials)));
        }
    }
}
