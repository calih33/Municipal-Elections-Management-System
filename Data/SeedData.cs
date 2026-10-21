using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Models;

namespace Municipal_Elections_Management_System.Data;

/// <summary>
/// Applies migrations, then seeds roles, the required admin users and sample domain data.
/// Safe to run on every startup: each step only inserts what is missing.
/// </summary>
public static class SeedData
{
    private const string SeedPassword = "P@$$w0rd";
    private static readonly DateTime ElectionDate = new(2026, 10, 17);

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<ApplicationDbContext>();

        EnsureDatabaseDirectoryExists(db);
        await db.Database.MigrateAsync();

        await SeedMunicipalitiesAsync(db);
        await SeedRolesAsync(db, provider.GetRequiredService<RoleManager<IdentityRole>>());
        await SeedUsersAsync(provider.GetRequiredService<UserManager<IdentityUser>>());
    }

    // SQLite creates the file but not its folder (e.g. /home/data on Azure).
    private static void EnsureDatabaseDirectoryExists(ApplicationDbContext db)
    {
        var dataSource = new SqliteConnectionStringBuilder(db.Database.GetConnectionString()).DataSource;
        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static async Task SeedRolesAsync(ApplicationDbContext db, RoleManager<IdentityRole> roleManager)
    {
        var roleNames = await db.Municipalities.Select(m => m.AdminRoleName).ToListAsync();
        roleNames.Insert(0, AppRoles.SuperAdmin);

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        await EnsureUserAsync(userManager, "super", "super@elections.local", AppRoles.SuperAdmin);
        await EnsureUserAsync(userManager, "pm", "pm@elections.local", AppRoles.ForMunicipality("Pitt Meadows"));
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string userName, string email, string role)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new IdentityUser { UserName = userName, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, SeedPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed user '{userName}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedMunicipalitiesAsync(ApplicationDbContext db)
    {
        if (await db.Municipalities.AnyAsync())
        {
            return;
        }

        db.Municipalities.AddRange(
            BuildMunicipality(
                "Pitt Meadows",
                "A small city on the north bank of the Fraser River, known for its farmland, dikes and regional airport.",
                new()
                {
                    ["Mayor"] = (1, ["Eleanor Vasquez", "Graham Whitfield"]),
                    ["Councilor"] = (6, ["Priya Sandhu", "Marcus Bell", "Hannah O'Connor", "Daniel Okafor",
                                         "Sophie Tremblay", "Kevin Nakamura", "Rachel Lindqvist", "Omar Haddad"]),
                    ["School Trustee"] = (2, ["Laura Chen", "Brian McAllister", "Fatima Rahimi"]),
                }),
            BuildMunicipality(
                "Maple Ridge",
                "A growing community between the Fraser River and the Golden Ears mountains, with a historic downtown and large parks.",
                new()
                {
                    ["Mayor"] = (1, ["Thomas Archer", "Natalie Dubois"]),
                    ["Councilor"] = (6, ["Jasmine Gill", "Peter Kowalski", "Aiden Murphy", "Leah Thompson", "Victor Silva"]),
                    ["School Trustee"] = (2, ["Grace Fontaine", "Samuel Park", "Irene Novak"]),
                }),
            BuildMunicipality(
                "Burnaby",
                "One of the largest cities in Metro Vancouver, home to Metrotown, Burnaby Mountain and Simon Fraser University.",
                new()
                {
                    ["Mayor"] = (1, ["Michael Leung", "Andrea Kovacs"]),
                    ["Councilor"] = (8, ["Ravi Dhillon", "Caroline Price", "Ethan Wong", "Isabel Moreno"]),
                    ["School Trustee"] = (7, ["Monica Ferreira", "Jordan Blake", "Wei Zhang"]),
                }));

        await db.SaveChangesAsync();
    }

    private static readonly string[] Priorities =
    [
        "affordable housing and responsible growth",
        "safer streets and better transit connections",
        "protecting parks, farmland and green space",
        "keeping property taxes predictable",
        "support for local businesses and jobs",
        "inclusive, well-funded public schools",
        "climate resilience and flood protection",
        "transparent, accessible local government",
    ];

    private static Municipality BuildMunicipality(
        string name,
        string description,
        Dictionary<string, (int Seats, string[] Candidates)> positions)
    {
        var municipality = new Municipality
        {
            Name = name,
            Province = "British Columbia",
            ElectionDate = ElectionDate,
            Description = description,
            AdminRoleName = AppRoles.ForMunicipality(name),
        };

        var priorityIndex = name.Length;
        foreach (var (positionName, (seats, candidateNames)) in positions)
        {
            var position = new Position { Name = positionName, SeatsAvailable = seats };

            foreach (var candidateName in candidateNames)
            {
                var priority = Priorities[priorityIndex++ % Priorities.Length];
                var slug = candidateName.ToLowerInvariant().Replace("'", "").Replace(' ', '.');

                position.Candidates.Add(new Candidate
                {
                    Name = candidateName,
                    Profile = $"{candidateName} is running for {positionName} in {name}. A long-time resident and community " +
                              $"volunteer, {candidateName.Split(' ')[0]} is campaigning on {priority}, and believes local " +
                              "decisions should be made in the open with residents at the table.",
                    Email = $"{slug}@example.com",
                    Website = priorityIndex % 2 == 0 ? $"https://example.com/{slug.Replace('.', '-')}" : null,
                    DateCreated = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                });
            }

            municipality.Positions.Add(position);
        }

        return municipality;
    }
}
