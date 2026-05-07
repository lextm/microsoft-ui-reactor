using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;

namespace SettingsHub;

// ── Data Model ──────────────────────────────────────────────────────

record AppSettings(
    string AppName,
    string Language,
    bool AutoSave,
    string Theme,
    string AccentColor,
    double FontSize,
    bool EmailNotifications,
    bool PushNotifications,
    bool SoundEnabled,
    bool BadgeEnabled,
    bool AnalyticsEnabled,
    bool DataCollection)
{
    public static AppSettings Default => new(
        AppName: "My Application",
        Language: "English",
        AutoSave: true,
        Theme: "System",
        AccentColor: "Blue",
        FontSize: 14,
        EmailNotifications: true,
        PushNotifications: true,
        SoundEnabled: true,
        BadgeEnabled: false,
        AnalyticsEnabled: false,
        DataCollection: false);
}

// ── Actions ─────────────────────────────────────────────────────────

abstract record SettingsAction;
record SetAppName(string Value) : SettingsAction;
record SetLanguage(string Value) : SettingsAction;
record SetAutoSave(bool Value) : SettingsAction;
record SetTheme(string Value) : SettingsAction;
record SetAccentColor(string Value) : SettingsAction;
record SetFontSize(double Value) : SettingsAction;
record SetEmailNotifications(bool Value) : SettingsAction;
record SetPushNotifications(bool Value) : SettingsAction;
record SetSoundEnabled(bool Value) : SettingsAction;
record SetBadgeEnabled(bool Value) : SettingsAction;
record SetAnalyticsEnabled(bool Value) : SettingsAction;
record SetDataCollection(bool Value) : SettingsAction;
record ResetGeneral : SettingsAction;
record ResetAppearance : SettingsAction;
record ResetNotifications : SettingsAction;
record ResetPrivacy : SettingsAction;
record ClearAllData : SettingsAction;

// ── Reducer ─────────────────────────────────────────────────────────

static class SettingsReducer
{
    public static AppSettings Reduce(AppSettings state, SettingsAction action) => action switch
    {
        SetAppName a => state with { AppName = a.Value },
        SetLanguage a => state with { Language = a.Value },
        SetAutoSave a => state with { AutoSave = a.Value },
        SetTheme a => state with { Theme = a.Value },
        SetAccentColor a => state with { AccentColor = a.Value },
        SetFontSize a => state with { FontSize = a.Value },
        SetEmailNotifications a => state with { EmailNotifications = a.Value },
        SetPushNotifications a => state with { PushNotifications = a.Value },
        SetSoundEnabled a => state with { SoundEnabled = a.Value },
        SetBadgeEnabled a => state with { BadgeEnabled = a.Value },
        SetAnalyticsEnabled a => state with { AnalyticsEnabled = a.Value },
        SetDataCollection a => state with { DataCollection = a.Value },
        ResetGeneral => state with
        {
            AppName = AppSettings.Default.AppName,
            Language = AppSettings.Default.Language,
            AutoSave = AppSettings.Default.AutoSave,
        },
        ResetAppearance => state with
        {
            Theme = AppSettings.Default.Theme,
            AccentColor = AppSettings.Default.AccentColor,
            FontSize = AppSettings.Default.FontSize,
        },
        ResetNotifications => state with
        {
            EmailNotifications = AppSettings.Default.EmailNotifications,
            PushNotifications = AppSettings.Default.PushNotifications,
            SoundEnabled = AppSettings.Default.SoundEnabled,
            BadgeEnabled = AppSettings.Default.BadgeEnabled,
        },
        ResetPrivacy => state with
        {
            AnalyticsEnabled = AppSettings.Default.AnalyticsEnabled,
            DataCollection = AppSettings.Default.DataCollection,
        },
        ClearAllData => AppSettings.Default,
        _ => state,
    };
}

// ── Context ─────────────────────────────────────────────────────────

static class SettingsContext
{
    public static readonly Context<AppSettings> Settings = new(AppSettings.Default);
    public static readonly Context<Action<SettingsAction>> Dispatch = new(_ => { });
}

// ── Root App ────────────────────────────────────────────────────────

class App : Component
{
    public override Element Render()
    {
        var (settings, dispatch) = UseReducer<AppSettings, SettingsAction>(
            SettingsReducer.Reduce, AppSettings.Default);

        var (selectedTag, setSelectedTag) = UseState("general");

        var menuItems = new[]
        {
            NavItem("General", icon: "Setting", tag: "general"),
            NavItem("Appearance", icon: "ColorBackground", tag: "appearance"),
            NavItem("Notifications", icon: "Message", tag: "notifications"),
            NavItem("Privacy", icon: "Permissions", tag: "privacy"),
            NavItem("About", icon: "Info", tag: "about"),
        };

        Element page = selectedTag switch
        {
            "general" => Component<GeneralPage>(),
            "appearance" => Component<AppearancePage>(),
            "notifications" => Component<NotificationsPage>(),
            "privacy" => Component<PrivacyPage>(),
            "about" => Component<AboutPage>(),
            _ => Component<GeneralPage>(),
        };

        return NavigationView(menuItems, page)
            .Set(nv =>
            {
                nv.SelectedItem = nv.MenuItems
                    .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag as string == selectedTag);
                nv.IsSettingsVisible = false;
                nv.PaneTitle = "Settings Hub";
            })
            .Provide(SettingsContext.Settings, settings)
            .Provide(SettingsContext.Dispatch, dispatch)
            with
            {
                SelectedTag = selectedTag,
                OnSelectionChanged = tag => { if (tag is not null) setSelectedTag(tag); },
                IsSettingsVisible = false,
                PaneTitle = "Settings Hub",
            };
    }
}

// ── Helpers ─────────────────────────────────────────────────────────

static class UI
{
    public static Element SectionCard(string title, params Element[] children)
    {
        var content = VStack(8, children).Padding(16);
        return Border(
            VStack(4,
                TextBlock(title).FontSize(16),
                content
            )
        )
        .Padding(16)
        .CornerRadius(8)
        .Margin(0, 0, 0, 12);
    }

    public static Element PageLayout(string title, string subtitle, Element[] sections, Action onReset)
    {
        return ScrollView(
            VStack(12,
                [
                    TextBlock(title).FontSize(28),
                    TextBlock(subtitle).FontSize(14),
                    .. sections,
                    Button("Reset to Defaults", onReset).Margin(0, 8, 0, 24),
                ]
            ).Padding(32)
        );
    }

    public static Element LabeledControl(string label, Element control)
    {
        return VStack(4,
            TextBlock(label).FontSize(13),
            control
        ).Margin(0, 4, 0, 4);
    }
}

// ── General Page ────────────────────────────────────────────────────

class GeneralPage : Component
{
    public override Element Render()
    {
        var settings = UseContext(SettingsContext.Settings);
        var dispatch = UseContext(SettingsContext.Dispatch);

        var languages = new[] { "English", "Spanish", "French", "German", "Japanese" };
        var selectedLangIdx = Array.IndexOf(languages, settings.Language);
        if (selectedLangIdx < 0) selectedLangIdx = 0;

        return UI.PageLayout(
            "General",
            "Configure basic application settings.",
            [
                UI.SectionCard("Application",
                    UI.LabeledControl("App Name",
                        TextField(settings.AppName,
                            v => dispatch(new SetAppName(v)),
                            placeholder: "Enter app name")),
                    UI.LabeledControl("Language",
                        ComboBox(languages, selectedLangIdx,
                            i => dispatch(new SetLanguage(languages[i]))))
                ),
                UI.SectionCard("Behavior",
                    ToggleSwitch(settings.AutoSave,
                        v => dispatch(new SetAutoSave(v)),
                        onContent: "On", offContent: "Off",
                        header: "Auto-save")
                ),
            ],
            () => dispatch(new ResetGeneral()));
    }
}

// ── Appearance Page ─────────────────────────────────────────────────

class AppearancePage : Component
{
    public override Element Render()
    {
        var settings = UseContext(SettingsContext.Settings);
        var dispatch = UseContext(SettingsContext.Dispatch);

        var themes = new[] { "Light", "Dark", "System" };
        var selectedThemeIdx = Array.IndexOf(themes, settings.Theme);
        if (selectedThemeIdx < 0) selectedThemeIdx = 2;

        var accents = new[] { "Blue", "Red", "Green", "Purple", "Orange" };
        var selectedAccentIdx = Array.IndexOf(accents, settings.AccentColor);
        if (selectedAccentIdx < 0) selectedAccentIdx = 0;

        return UI.PageLayout(
            "Appearance",
            "Customize the look and feel of the application.",
            [
                UI.SectionCard("Theme",
                    RadioButtons(themes, selectedThemeIdx,
                        i => dispatch(new SetTheme(themes[i])))
                        with { Header = "App Theme" }
                ),
                UI.SectionCard("Accent Color",
                    RadioButtons(accents, selectedAccentIdx,
                        i => dispatch(new SetAccentColor(accents[i])))
                        with { Header = "Accent Color" }
                ),
                UI.SectionCard("Typography",
                    UI.LabeledControl($"Font Size: {settings.FontSize:F0}px",
                        Slider(settings.FontSize, min: 10, max: 24,
                            v => dispatch(new SetFontSize(v)))
                            with { StepFrequency = 1 }),
                    TextBlock("The quick brown fox jumps over the lazy dog.")
                        .FontSize(settings.FontSize)
                        .Margin(0, 8, 0, 0)
                ),
            ],
            () => dispatch(new ResetAppearance()));
    }
}

// ── Notifications Page ──────────────────────────────────────────────

class NotificationsPage : Component
{
    public override Element Render()
    {
        var settings = UseContext(SettingsContext.Settings);
        var dispatch = UseContext(SettingsContext.Dispatch);

        return UI.PageLayout(
            "Notifications",
            "Manage how you receive notifications.",
            [
                UI.SectionCard("Channels",
                    ToggleSwitch(settings.EmailNotifications,
                        v => dispatch(new SetEmailNotifications(v)),
                        onContent: "On", offContent: "Off",
                        header: "Email Notifications"),
                    ToggleSwitch(settings.PushNotifications,
                        v => dispatch(new SetPushNotifications(v)),
                        onContent: "On", offContent: "Off",
                        header: "Push Notifications")
                ),
                UI.SectionCard("Feedback",
                    ToggleSwitch(settings.SoundEnabled,
                        v => dispatch(new SetSoundEnabled(v)),
                        onContent: "On", offContent: "Off",
                        header: "Sound"),
                    ToggleSwitch(settings.BadgeEnabled,
                        v => dispatch(new SetBadgeEnabled(v)),
                        onContent: "On", offContent: "Off",
                        header: "Badge Count")
                ),
            ],
            () => dispatch(new ResetNotifications()));
    }
}

// ── Privacy Page ────────────────────────────────────────────────────

class PrivacyPage : Component
{
    public override Element Render()
    {
        var settings = UseContext(SettingsContext.Settings);
        var dispatch = UseContext(SettingsContext.Dispatch);
        var (showClearDialog, setShowClearDialog) = UseState(false);

        return VStack(0,
            UI.PageLayout(
                "Privacy",
                "Control your data and privacy preferences.",
                [
                    UI.SectionCard("Data Collection",
                        ToggleSwitch(settings.AnalyticsEnabled,
                            v => dispatch(new SetAnalyticsEnabled(v)),
                            onContent: "On", offContent: "Off",
                            header: "Analytics"),
                        TextBlock("Help improve the app by sending anonymous usage data.")
                            .FontSize(12).Margin(0, 0, 0, 8),
                        ToggleSwitch(settings.DataCollection,
                            v => dispatch(new SetDataCollection(v)),
                            onContent: "On", offContent: "Off",
                            header: "Data Collection Consent")
                    ),
                    UI.SectionCard("Manage Data",
                        TextBlock("Remove all locally stored settings and cached data.").FontSize(13),
                        Button("Clear All Data", () => setShowClearDialog(true)).Margin(0, 8, 0, 0)
                    ),
                ],
                () => dispatch(new ResetPrivacy())),
            ContentDialog("Clear All Data?",
                TextBlock("This will reset all settings to their default values. This action cannot be undone."),
                "Clear")
                with
                {
                    IsOpen = showClearDialog,
                    SecondaryButtonText = "Cancel",
                    DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Secondary,
                    OnClosed = result =>
                    {
                        setShowClearDialog(false);
                        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                            dispatch(new ClearAllData());
                    },
                }
        );
    }
}

// ── About Page ──────────────────────────────────────────────────────

class AboutPage : Component
{
    public override Element Render()
    {
        return ScrollView(
            VStack(12,
                TextBlock("About").FontSize(28),
                TextBlock("Information about this application.").FontSize(14),
                UI.SectionCard("Application Info",
                    InfoRow("App Name", "Settings Hub"),
                    InfoRow("Version", "1.0.0"),
                    InfoRow("Build", "2025.01.15-release"),
                    InfoRow("Framework", "Microsoft.UI.Reactor")
                ),
                UI.SectionCard("License",
                    TextBlock("MIT License").FontSize(14),
                    TextBlock(
                        "Permission is hereby granted, free of charge, to any person " +
                        "obtaining a copy of this software and associated documentation " +
                        "files, to deal in the Software without restriction, including " +
                        "without limitation the rights to use, copy, modify, merge, publish, " +
                        "distribute, sublicense, and/or sell copies of the Software.")
                        .FontSize(12)
                ),
                UI.SectionCard("Links",
                    TextBlock("GitHub: github.com/microsoft/microsoft-ui-reactor").FontSize(13),
                    TextBlock("Documentation: learn.microsoft.com").FontSize(13)
                )
            ).Padding(32)
        );
    }

    static Element InfoRow(string label, string value)
    {
        return HStack(8,
            TextBlock($"{label}:").FontSize(13),
            TextBlock(value).FontSize(13)
        ).Margin(0, 2, 0, 2);
    }
}

// ── Entry Point ─────────────────────────────────────────────────────

class Program
{
    [STAThread]
    static void Main() => ReactorApp.Run<App>("Settings Hub", 1000, 700);
}
