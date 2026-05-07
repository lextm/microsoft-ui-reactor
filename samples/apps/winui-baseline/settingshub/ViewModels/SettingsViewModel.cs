using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SettingsHub.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public static SettingsViewModel Instance { get; } = new();

    private string _appName = "My Application";
    private string _language = "English";
    private bool _autoSave = true;
    private string _theme = "System";
    private string _accentColor = "Blue";
    private double _fontSize = 14.0;
    private bool _emailNotifications = true;
    private bool _pushNotifications = true;
    private bool _soundEnabled = true;
    private bool _badgeEnabled = true;
    private bool _analyticsEnabled;
    private bool _dataCollection;

    public string AppName { get => _appName; set => SetProperty(ref _appName, value); }
    public string Language { get => _language; set => SetProperty(ref _language, value); }
    public bool AutoSave { get => _autoSave; set => SetProperty(ref _autoSave, value); }
    public string Theme { get => _theme; set => SetProperty(ref _theme, value); }
    public string AccentColor { get => _accentColor; set => SetProperty(ref _accentColor, value); }
    public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
    public bool EmailNotifications { get => _emailNotifications; set => SetProperty(ref _emailNotifications, value); }
    public bool PushNotifications { get => _pushNotifications; set => SetProperty(ref _pushNotifications, value); }
    public bool SoundEnabled { get => _soundEnabled; set => SetProperty(ref _soundEnabled, value); }
    public bool BadgeEnabled { get => _badgeEnabled; set => SetProperty(ref _badgeEnabled, value); }
    public bool AnalyticsEnabled { get => _analyticsEnabled; set => SetProperty(ref _analyticsEnabled, value); }
    public bool DataCollection { get => _dataCollection; set => SetProperty(ref _dataCollection, value); }

    public string[] Languages => ["English", "Spanish", "French", "German", "Japanese", "Chinese"];
    public string[] AccentColors => ["Blue", "Red", "Green", "Purple", "Orange", "Teal"];

    public void ResetGeneral()
    {
        AppName = "My Application";
        Language = "English";
        AutoSave = true;
    }

    public void ResetAppearance()
    {
        Theme = "System";
        AccentColor = "Blue";
        FontSize = 14.0;
    }

    public void ResetNotifications()
    {
        EmailNotifications = true;
        PushNotifications = true;
        SoundEnabled = true;
        BadgeEnabled = true;
    }

    public void ResetPrivacy()
    {
        AnalyticsEnabled = false;
        DataCollection = false;
    }

    public void ClearAllData()
    {
        ResetGeneral();
        ResetAppearance();
        ResetNotifications();
        ResetPrivacy();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
