using System.Collections.ObjectModel;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Layout;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static Microsoft.UI.Reactor.Core.Theme;

namespace Contacts;

public enum SortField { Name, Email }

public class ContactsApp : Component
{
    private readonly ObservableCollection<Contact> _contacts = new(Contact.SeedData());

    public override Element Render()
    {
        var contacts = UseCollection(_contacts);
        var (search, setSearch) = UseState("");
        var (sortField, setSortField) = UseState(SortField.Name);
        var (sortAsc, setSortAsc) = UseState(true);
        var (selectedId, setSelectedId) = UseState<string?>(null);
        var (showAdd, setShowAdd) = UseState(false);
        var (showDelete, setShowDelete) = UseState(false);

        // Edit fields for detail pane
        var (editName, setEditName) = UseState("");
        var (editEmail, setEditEmail) = UseState("");
        var (editPhone, setEditPhone) = UseState("");
        var (dirty, setDirty) = UseState(false);

        // Add dialog fields
        var (addName, setAddName) = UseState("");
        var (addEmail, setAddEmail) = UseState("");
        var (addPhone, setAddPhone) = UseState("");

        var filtered = UseMemo(() =>
        {
            var q = search.Trim();
            IEnumerable<Contact> result = contacts;
            if (!string.IsNullOrEmpty(q))
                result = result.Where(c =>
                    c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(q, StringComparison.OrdinalIgnoreCase));
            result = sortField switch
            {
                SortField.Email => sortAsc
                    ? result.OrderBy(c => c.Email, StringComparer.OrdinalIgnoreCase)
                    : result.OrderByDescending(c => c.Email, StringComparer.OrdinalIgnoreCase),
                _ => sortAsc
                    ? result.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    : result.OrderByDescending(c => c.Name, StringComparer.OrdinalIgnoreCase),
            };
            return result.ToList();
        }, search, sortField, sortAsc, contacts.Count);

        var selected = UseMemo(() =>
            selectedId is not null ? contacts.FirstOrDefault(c => c.Id == selectedId) : null,
            selectedId!, contacts.Count);

        // Sync edit fields when selection changes
        UseEffect(() =>
        {
            if (selected is not null)
            {
                setEditName(selected.Name);
                setEditEmail(selected.Email);
                setEditPhone(selected.Phone);
                setDirty(false);
            }
        }, selectedId!);

        var onToggleSort = UseCallback(() =>
        {
            setSortAsc(!sortAsc);
        }, sortAsc);

        var onSave = UseCallback(() =>
        {
            if (selected is null || !dirty) return;
            var updated = selected with { Name = editName.Trim(), Email = editEmail.Trim(), Phone = editPhone.Trim() };
            _contacts[_contacts.IndexOf(selected)] = updated;
            setDirty(false);
        }, selected!, editName, editEmail, editPhone, dirty);

        // Validation
        var nameError = dirty && string.IsNullOrWhiteSpace(editName) ? "Name is required" : null;
        var emailError = dirty && !Contact.IsValidEmail(editEmail) ? "Valid email required" : null;
        var canSave = dirty
            && !string.IsNullOrWhiteSpace(editName)
            && Contact.IsValidEmail(editEmail);

        // Add dialog validation
        var addNameError = showAdd && string.IsNullOrWhiteSpace(addName) ? "Name is required" : null;
        var addEmailError = showAdd && addEmail.Length > 0 && !Contact.IsValidEmail(addEmail)
            ? "Valid email required" : null;
        var canAdd = !string.IsNullOrWhiteSpace(addName) && Contact.IsValidEmail(addEmail);

        // ── Toolbar ───────────────────────────────────────────────────
        var toolbar = (FlexRow(
            AutoSuggestBox(search, setSearch).Width(250).Margin(0, 0, 8, 0),
            ToggleButton($"Sort: {sortField} {(sortAsc ? "\u2191" : "\u2193")}",
                isChecked: false,
                onToggled: _ => onToggleSort()),
            Button("Sort by " + (sortField == SortField.Name ? "Email" : "Name"), () =>
                setSortField(sortField == SortField.Name ? SortField.Email : SortField.Name)),
            FlexColumn().Flex(grow: 1),
            Button("\uff0b Add", () =>
            {
                setAddName("");
                setAddEmail("");
                setAddPhone("");
                setShowAdd(true);
            }),
            Button("\U0001f5d1 Delete", () =>
            {
                if (selected is not null) setShowDelete(true);
            })
        ) with { ColumnGap = 6, AlignItems = FlexAlign.Center })
        .Padding(12).Flex(shrink: 0);

        // ── Master list ───────────────────────────────────────────────
        var listItems = filtered.Select(c =>
            Component<ContactRow, ContactRowProps>(
                new ContactRowProps(c, c.Id == selectedId, () =>
                {
                    setSelectedId(c.Id);
                    setEditName(c.Name);
                    setEditEmail(c.Email);
                    setEditPhone(c.Phone);
                    setDirty(false);
                })
            ).WithKey(c.Id)
        ).ToArray();

        var masterPane = FlexColumn(
            TextBlock($"{filtered.Count} contacts").FontSize(12).Foreground(SecondaryText)
                .Padding(12, 6, 12, 6).Flex(shrink: 0),
            ScrollView(
                VStack(4, listItems)
                    .Padding(8)
            ).Flex(grow: 1)
        ).Width(320).Flex(shrink: 0)
         .WithBorder(CardStroke, 1);

        // ── Detail pane ───────────────────────────────────────────────
        Element detailPane;
        if (selected is not null)
        {
            detailPane = (FlexColumn(
                TextBlock("Contact Details").FontSize(20)
                    .FontWeight(new Windows.UI.Text.FontWeight(600))
                    .Flex(shrink: 0),

                TextField(editName, v => { setEditName(v); setDirty(true); },
                    placeholder: "Full name", header: "Name")
                    .Flex(shrink: 0),
                nameError is not null
                    ? TextBlock(nameError).FontSize(12).Foreground("#E74C3C").Flex(shrink: 0)
                    : null,

                TextField(editEmail, v => { setEditEmail(v); setDirty(true); },
                    placeholder: "email@example.com", header: "Email")
                    .Flex(shrink: 0),
                emailError is not null
                    ? TextBlock(emailError).FontSize(12).Foreground("#E74C3C").Flex(shrink: 0)
                    : null,

                TextField(editPhone, v => { setEditPhone(v); setDirty(true); },
                    placeholder: "(555) 000-0000", header: "Phone")
                    .Flex(shrink: 0),

                (FlexRow(
                    Button("Save", onSave) with { IsEnabled = canSave },
                    Button("Revert", () =>
                    {
                        setEditName(selected.Name);
                        setEditEmail(selected.Email);
                        setEditPhone(selected.Phone);
                        setDirty(false);
                    }) with { IsEnabled = dirty }
                ) with { ColumnGap = 8 })
                .Margin(0, 8, 0, 0).Flex(shrink: 0)
            ) with { RowGap = 6 })
            .Padding(24).Flex(grow: 1);
        }
        else
        {
            detailPane = (FlexColumn(
                TextBlock("Select a contact").FontSize(16).Foreground(SecondaryText)
            ) with { AlignItems = FlexAlign.Center, JustifyContent = FlexJustify.Center })
            .Flex(grow: 1);
        }

        // ── Dialogs ───────────────────────────────────────────────────
        var addDialog = ContentDialog("New Contact",
            VStack(8,
                TextField(addName, setAddName, placeholder: "Full name", header: "Name"),
                addNameError is not null
                    ? TextBlock(addNameError).FontSize(12).Foreground("#E74C3C") : null,
                TextField(addEmail, setAddEmail, placeholder: "email@example.com", header: "Email"),
                addEmailError is not null
                    ? TextBlock(addEmailError).FontSize(12).Foreground("#E74C3C") : null,
                TextField(addPhone, setAddPhone, placeholder: "(555) 000-0000", header: "Phone")
            ).Width(320),
            "Add"
        ) with
        {
            IsOpen = showAdd,
            CloseButtonText = "Cancel",
            OnClosed = r =>
            {
                if (r == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && canAdd)
                {
                    var id = Guid.NewGuid().ToString("N")[..8];
                    _contacts.Add(new Contact(id, addName.Trim(), addEmail.Trim(), addPhone.Trim()));
                    setSelectedId(id);
                }
                setShowAdd(false);
            },
        };

        var deleteDialog = ContentDialog("Delete Contact",
            TextBlock(selected is not null
                ? $"Delete \"{selected.Name}\"? This cannot be undone."
                : ""),
            "Delete"
        ) with
        {
            IsOpen = showDelete,
            CloseButtonText = "Cancel",
            OnClosed = r =>
            {
                if (r == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary && selected is not null)
                {
                    _contacts.Remove(selected);
                    setSelectedId(null);
                }
                setShowDelete(false);
            },
        };

        // ── Root layout ───────────────────────────────────────────────
        return FlexColumn(
            toolbar,
            FlexRow(
                masterPane,
                detailPane
            ).Flex(grow: 1),
            addDialog,
            deleteDialog
        ).Background(SolidBackground);
    }
}
