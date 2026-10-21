using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;

namespace Municipal_Elections_Management_System.Services;

/// <summary>
/// Decides which municipalities the signed-in admin may manage.
/// Super Admins manage all of them; everyone else manages only the municipalities
/// whose AdminRoleName matches one of their roles. Use this for every admin
/// query and re-check it on every POST, never trust a MunicipalityId from a form.
/// </summary>
public interface IMunicipalityAccessService
{
    bool IsSuperAdmin(ClaimsPrincipal user);

    Task<IReadOnlyList<int>> GetManageableMunicipalityIdsAsync(ClaimsPrincipal user);

    Task<bool> CanManageAsync(ClaimsPrincipal user, int municipalityId);
}

public class MunicipalityAccessService(ApplicationDbContext db) : IMunicipalityAccessService
{
    public bool IsSuperAdmin(ClaimsPrincipal user) => user.IsInRole(AppRoles.SuperAdmin);

    public async Task<IReadOnlyList<int>> GetManageableMunicipalityIdsAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return [];
        }

        if (IsSuperAdmin(user))
        {
            return await db.Municipalities.Select(m => m.MunicipalityId).ToListAsync();
        }

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return await db.Municipalities
            .Where(m => roles.Contains(m.AdminRoleName))
            .Select(m => m.MunicipalityId)
            .ToListAsync();
    }

    public async Task<bool> CanManageAsync(ClaimsPrincipal user, int municipalityId)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (IsSuperAdmin(user))
        {
            return true;
        }

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return await db.Municipalities
            .AnyAsync(m => m.MunicipalityId == municipalityId && roles.Contains(m.AdminRoleName));
    }
}
