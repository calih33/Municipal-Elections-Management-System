---
name: scoped-admin-crud
description: How to build admin CRUD controllers in this project so municipality admins can only see and change their own municipality's data while Super Admins can manage everything. Use when adding or changing anything under Areas/Admin.
---

# Scoped admin CRUD

The marking scheme checks that a municipality admin (e.g. user `pm`, role "Pitt Meadows Admin") cannot see or change another municipality's data. All scoping goes through `IMunicipalityAccessService` (`Services/MunicipalityAccessService.cs`).

## Controller skeleton

```csharp
// admins only
[Area("Admin")]
[Authorize]
public class PositionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMunicipalityAccessService _access;

    public PositionsController(ApplicationDbContext context, IMunicipalityAccessService access)
    {
        _context = context;
        _access = access;
    }

    // GET: Admin/Positions
    public async Task<IActionResult> Index()
    {
        // super admin gets all, others only their own
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);
        var positions = await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .Where(p => ids.Contains(p.MunicipalityId!.Value))
            .ToListAsync();
        return View(positions);
    }
}
```

## Rules per action

| Action | Rule |
|---|---|
| Index / Dashboard | Filter by `GetManageableMunicipalityIdsAsync(User)`. |
| Details / Edit GET / Delete GET | Load the record; if missing → `NotFound()`; if `!CanManageAsync(User, record's municipality)` → `Forbid()`. |
| Create POST | Check the municipality chosen in the form. For candidates, look the position up in the database and check its municipality (`CanManagePositionAsync`); a position that doesn't exist is also `Forbid()`. |
| Edit POST | Load the **stored** record and check its municipality, **then** check the newly posted municipality/position too. Copy the allowed fields onto the stored entity; don't attach the posted object. |
| Delete POST | Load, check, delete uploaded image files, remove. |
| Dropdowns | Build them only from municipalities/positions the user can manage, in `ViewData["MunicipalityId"]` / `ViewData["PositionId"]`. The candidate position dropdown labels read "Pitt Meadows - Mayor". |

For candidates the municipality is `candidate.Position.Municipality`: always `Include(c => c.Position)` and check `Position.MunicipalityId`.

## Municipalities

- `Create` and `Delete` (GET and POST): `[Authorize(Roles = AppRoles.SuperAdmin)]`.
- On create, set `AdminRoleName = AppRoles.ForMunicipality(Name)` and create that role with `RoleManager<IdentityRole>` if it doesn't exist.
- Municipality admins may `Edit` their own municipality (including the logo) but never change `AdminRoleName`.

## POST checklist

- `[HttpPost, ValidateAntiForgeryToken]`
- Bind only form fields: `[Bind("Name,PositionId,Profile,Website,Email,PhotoFile")]` and similar.
- On invalid model state, reload the dropdowns before `return View(model)`.
- `TempData["Message"] = "..."` (shown by `_Layout`), then `RedirectToAction(nameof(Index))`.
- Uploads: see the `image-upload` skill.

## Verify

Log in as `pm` / `P@$$w0rd` and confirm:

- only Pitt Meadows rows appear
- `/Admin/Positions/Edit/{burnabyId}` goes to Access Denied
- posting a Burnaby `PositionId` or `MunicipalityId` from dev tools is rejected

Then log in as `super` and confirm everything is visible. The `/review-authorization` prompt walks through this.
