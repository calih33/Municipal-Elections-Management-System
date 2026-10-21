namespace Municipal_Elections_Management_System.Data;

public static class AppRoles
{
    public const string SuperAdmin = "Super Admin";

    // Each municipality gets its own admin group, e.g. "Pitt Meadows Admin".
    public static string ForMunicipality(string municipalityName) => $"{municipalityName} Admin";
}
