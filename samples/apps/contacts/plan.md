# Contacts AddressBook Build Plan

## Goal
Build a self-contained Reactor sample app that demonstrates a desktop address book with search, sort, master-detail editing, add/delete, and validation.

## Core Behavior
1. Display a scrollable list of contacts (name, email, phone) in a master pane.
2. Search-as-you-type filtering via `AutoSuggestBox`.
3. Sort contacts by name or email (toggle button).
4. Select a contact to view/edit in a detail pane.
5. Add new contact via button → dialog.
6. Delete contact with confirmation dialog.
7. Inline validation for email format and required name field.

## Reactor Features To Exercise
- `UseCollection` for the mutable contact list
- `UseObservable` for search text changes
- `UseMemo` for filtered/sorted view
- `UseCallback` for stable event handlers
- `UseState` for selected contact, edit mode, sort direction
- `ContentDialog` for add/delete confirmation
- Master-detail layout with `FlexRow` / `FlexColumn`
- `.WithKey()` on list items
- Theme tokens

## Data Model
- `Contact` record: Id (string/guid), Name, Email, Phone
- Seed data: ~15-20 contacts with realistic names

## UI Layout
- Top bar with `AutoSuggestBox` for search and sort toggle
- Toolbar with Add and Delete buttons
- Master pane (left): scrollable contact list with selection highlight
- Detail pane (right): view/edit form for selected contact

## Verification
1. Build the new project in Debug configuration.
2. Fix any compile or warning issues.
3. Confirm the sample builds cleanly with 0 errors and 0 warnings.
