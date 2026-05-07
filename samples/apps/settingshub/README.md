# Settings Hub

A Windows Settings-style multi-page settings app built with Reactor,
demonstrating NavigationView routing, shared state via UseContext, and
form input controls.

## What It Does
- NavigationView with 5 categories: General, Appearance, Notifications, Privacy, About
- Each page renders dedicated settings controls (toggles, sliders, dropdowns, radio buttons)
- Settings state shared across pages via UseContext + UseReducer
- Reset to Defaults on each page
- Destructive actions (Clear Data) require ContentDialog confirmation

## Reactor Features Exercised
| Feature | Usage |
|---|---|
| `NavigationView` | Left pane with icon+label items driving page selection |
| `UseContext` | Shared settings state across all page components |
| `UseReducer` | Typed action dispatch for settings mutations |
| `ToggleSwitch` | Boolean toggles (notifications, privacy, auto-save) |
| `Slider` | Font size range control |
| `ComboBox` | Language dropdown |
| `RadioButtons` | Theme selector (Light/Dark/System) |
| `TextBox` | App name text field |
| `ContentDialog` | Destructive action confirmation |
| Component composition | Each settings page is a separate component |

## Build & Run
```
dotnet build samples/apps/settingshub/SettingsHub.csproj
dotnet run --project samples/apps/settingshub/SettingsHub.csproj
```

## Build Metrics

| Metric | Value |
|---|---|
| **Agent model** | `claude-opus-4.6` |
| **Agent session** | Fresh (isolated sub-agent, no shared context) |
| **Input tokens** | 1,015,888 |
| **Output tokens** | 10,162 |
| **Total tokens** | 1,026,050 |
| **Peak context window** | 80,555 tokens |
| **Turns to completion** | 14 |
| **Wall-clock time** | 7 min 1 sec |
| **First-compile success** | No |
| **Compile errors fixed** | 18 |
| **Build → fix cycles** | 3 |
| **First-run success** | _(manual)_ |
| **Runtime errors** | _(manual)_ |
| **Human interventions** | 0 |
| **Feature completeness** | 100% — all planned features delivered |
| **Lines of code** | 377 |
| **Source files** | 1 (`App.cs`) |
