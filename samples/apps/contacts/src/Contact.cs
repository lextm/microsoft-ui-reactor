namespace Contacts;

public record Contact(string Id, string Name, string Email, string Phone)
{
    public static List<Contact> SeedData() =>
    [
        new("c01", "Alice Johnson",    "alice.johnson@example.com",    "(555) 100-1001"),
        new("c02", "Bob Martinez",     "bob.martinez@example.com",     "(555) 100-1002"),
        new("c03", "Carol Chen",       "carol.chen@example.com",       "(555) 100-1003"),
        new("c04", "David Kim",        "david.kim@example.com",        "(555) 100-1004"),
        new("c05", "Elena Petrova",    "elena.petrova@example.com",    "(555) 100-1005"),
        new("c06", "Frank Nakamura",   "frank.nakamura@example.com",   "(555) 100-1006"),
        new("c07", "Grace O'Brien",    "grace.obrien@example.com",     "(555) 100-1007"),
        new("c08", "Hector Ruiz",      "hector.ruiz@example.com",      "(555) 100-1008"),
        new("c09", "Ingrid Larsen",    "ingrid.larsen@example.com",    "(555) 100-1009"),
        new("c10", "James Okafor",     "james.okafor@example.com",     "(555) 100-1010"),
        new("c11", "Keiko Tanaka",     "keiko.tanaka@example.com",     "(555) 100-1011"),
        new("c12", "Liam Fitzgerald",  "liam.fitzgerald@example.com",  "(555) 100-1012"),
        new("c13", "Mei-Ling Wu",      "meiling.wu@example.com",       "(555) 100-1013"),
        new("c14", "Noah Patel",       "noah.patel@example.com",       "(555) 100-1014"),
        new("c15", "Olivia Santos",    "olivia.santos@example.com",    "(555) 100-1015"),
        new("c16", "Paul Andersen",    "paul.andersen@example.com",    "(555) 100-1016"),
        new("c17", "Quinn Dubois",     "quinn.dubois@example.com",     "(555) 100-1017"),
        new("c18", "Rosa Colombo",     "rosa.colombo@example.com",     "(555) 100-1018"),
    ];

    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email)
        && email.Contains('@')
        && email.IndexOf('@') > 0
        && email.IndexOf('@') < email.Length - 1
        && email.Contains('.', StringComparison.Ordinal);
}
