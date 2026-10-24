namespace Municipal_Elections_Management_System.Data;

public static class AppRoles
{
    public const string SuperAdmin = "Super Admin";

    // Policy for the Admin area: the user must belong to at least one admin group.
    // Mobile app users register through the API and have no roles, so they can't get in.
    public const string AdminPolicy = "Admin";

    // Each municipality gets its own admin group, e.g. "Pitt Meadows Admin".
    public static string ForMunicipality(string municipalityName) => $"{municipalityName} Admin";
}
