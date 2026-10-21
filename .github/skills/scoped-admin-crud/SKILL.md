---
name: scoped-admin-crud
description: How to build admin CRUD controllers in this project so municipality admins can only see and change their own municipality's data while Super Admins can manage everything. Use when adding or changing anything under Areas/Admin.
---

# Scoped admin CRUD

The marking scheme checks that a municipality admin (e.g. user `pm`, role "Pitt Meadows Admin") cannot see or change another municipality's data. All scoping goes through `IMunicipalityAccessService` (`Services/MunicipalityAccessService.cs`).

## Controller skeleton

```csharp
[Area("Admin")]
[Authorize]
public class PositionsController(ApplicationDbContext db, IMunicipalityAccessService access) : Controller
{
    public async Task<IActionResult> Index()
    {
        var allowed = await access.GetManageableMunicipalityIdsAsync(User);
        var positions = await db.Positions
            .Include(p => p.Municipality)
            .Where(p => allowed.Contains(p.MunicipalityId!.Value))
            .OrderBy(p => p.Municipality!.Name).ThenBy(p => p.Name)
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
| Create POST | `CanManageAsync` for the municipality chosen in the form (for candidates: the chosen position's municipality, looked up from the DB). |
| Edit POST | Load the **stored** record and check its municipality, **then** check the newly posted municipality/position too. Copy allowed fields onto the stored entity; don't attach the posted object. |
| Delete POST | Load, check, remove, delete any uploaded image file. |
| Dropdowns | Build `SelectList`s only from municipalities/positions the user can manage. |

For candidates the municipality is `candidate.Position.Municipality` – always `Include(c => c.Position)` and check `Position.MunicipalityId`.

## Municipalities

- `Create` and `Delete` actions: `[Authorize(Roles = AppRoles.SuperAdmin)]`.
- On create, set `AdminRoleName = AppRoles.ForMunicipality(Name)` and create that role with `RoleManager<IdentityRole>` if it doesn't exist.
- Municipality admins may `Edit` their own municipality (including the logo) but never change `AdminRoleName`.

## POST checklist

- `[HttpPost, ValidateAntiForgeryToken]`
- Access check before `ModelState.IsValid` handling, so a forbidden request never re-renders data.
- On invalid model state, rebuild dropdowns before `return View(model)`.
- `TempData["Success"] = "..."` then `RedirectToAction(nameof(Index))`.

## Verify

Log in as `pm` / `P@$$w0rd` and confirm: only Pitt Meadows rows appear; `/Admin/Positions/Edit/{burnabyId}` returns 403; posting a Burnaby `PositionId` from dev tools is rejected. Then log in as `super` and confirm everything is visible.
