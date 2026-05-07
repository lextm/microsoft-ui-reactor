# Settings Hub Build Plan

## Goal
Build a Windows Settings-style multi-page settings app using Reactor's
NavigationView integration and shared state management.

## Scope
- NavigationView with 5 categories: General, Appearance, Notifications, Privacy, About
- Each category renders a dedicated settings page component
- UseReducer for typed settings state management
- UseContext for sharing state across page components
- Form controls: ToggleSwitch, Slider, ComboBox, RadioButtons, TextBox
- ContentDialog for destructive action confirmation
- Reset to Defaults per page

## Architecture

### Entry shell
- `App.cs` hosts the entire sample in a single file.
- `SettingsHubApp` provides context and renders NavigationView.
- Selected tag drives which page component renders in the content area.

### State management
- `AppSettings` immutable record holds all settings values.
- `UseReducer` with typed action dispatch handles state transitions.
- `UseContext` shares settings + dispatch across all page components.

### Page components
- `GeneralPage`: App name, language, auto-save toggle
- `AppearancePage`: Theme radio buttons, accent color, font size slider
- `NotificationsPage`: Email, push, sound, badge toggle switches
- `PrivacyPage`: Analytics toggle, data collection toggle, clear data button
- `AboutPage`: Version info, build info, license text

### UI composition
- NavigationView with icon + label items
- Card-style sections with labeled controls
- Reset button at bottom of each page
- Confirmation dialog for destructive actions (clear data)

## Verification
1. Build `SettingsHub.csproj` in Debug.
2. Fix all compiler errors until 0 errors, 0 warnings.
