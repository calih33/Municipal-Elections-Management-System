---
description: Audit admin controllers for Super Admin vs municipality admin access leaks
agent: ask
---

Review every controller in `Areas/Admin/Controllers/` for authorization gaps.

For each action, report whether:

1. The controller has `[Area("Admin")]` and `[Authorize]`.
2. Municipality Create/Delete is restricted to `AppRoles.SuperAdmin`.
3. List and detail queries are filtered by `IMunicipalityAccessService.GetManageableMunicipalityIdsAsync(User)`.
4. GET actions for a single record return `NotFound()` or `Forbid()` when the record belongs to another municipality.
5. POST actions re-check `CanManageAsync` for both the stored record and any municipality/position ID posted in the form.
6. Dropdowns only offer municipalities/positions the user can manage.

Walk through this concrete attack for each POST: the `pm` user (Pitt Meadows Admin) edits the hidden `MunicipalityId` or `PositionId` to a Burnaby value. Say whether it is blocked and where.

List problems as `file:line – problem – fix`. Don't change code unless asked.
