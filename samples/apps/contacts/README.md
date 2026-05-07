# Contacts AddressBook

A Reactor sample app demonstrating a desktop address book with search, sort, master-detail editing, add/delete dialogs, and inline validation.

## Features

- Scrollable contact list with 18 seed contacts
- Search-as-you-type filtering via `AutoSuggestBox`
- Sort toggle (name / email)
- Master-detail layout: select to view/edit
- Add contact via `ContentDialog`
- Delete with confirmation dialog
- Inline validation (required name, email format)
- Theme-consistent selection highlighting

## Reactor APIs Exercised

- `UseCollection` — mutable contact list
- `UseMemo` — filtered/sorted view
- `UseCallback` — stable event handlers
- `UseState` — selected contact, edit mode, sort direction
- `UseEffect` — side effects
- `ContentDialog` — add/delete workflows
- `Component<T, P>` — ContactRow component with props
- `FlexRow` / `FlexColumn` — master-detail layout
- `.WithKey()` — stable list reconciliation
- `AutoSuggestBox` — search input
- Theme tokens

## Project Structure

- `App.cs` — entry point
- `src/Contact.cs` — model, seed data, validation
- `src/ContactsApp.cs` — main component
- `src/ContactRow.cs` — list row component

## Build

```sh
dotnet build samples/apps/contacts/Contacts.csproj -c Debug
```

## AI Build Metrics

| Metric | Value |
|---|---|
| Agent model | claude-opus-4.6 (Anthropic), premium, 200K context |
| Agent session | Fresh (no shared context) |
| Total tokens (in + out) | 674,689 |
| Input tokens | 670,332 |
| Output tokens | 4,357 |
| Peak context (single turn) | 124,032 |
| Turns to completion | 6 |
| Wall-clock time | 6 min 30 s |
| First-compile success | ❌ No |
| Compile errors fixed | 13 |
| Build→fix cycles | 4 |
| First-run success | ⏳ Not yet validated |
| Runtime errors | ⏳ Not yet validated |
| Human interventions | 0 |
| Feature completeness | 100% |
| Lines of C# | 289 |
| Source files | 5 |
