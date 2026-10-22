---
description: Generate a municipality-scoped admin CRUD controller and views for an entity
agent: agent
---

Create full admin CRUD for the `${input:entity:Entity name, e.g. Position}` entity.

Follow the `scoped-admin-crud` skill in `.github/skills/scoped-admin-crud/SKILL.md` and the repository instructions.

Deliverables:

1. `Areas/Admin/Controllers/${input:entity}sController.cs` with Index, Details, Create (GET/POST), Edit (GET/POST), Delete (GET) and DeleteConfirmed (POST).
2. Views in `Areas/Admin/Views/${input:entity}s/` for each GET action, using Bootstrap cards/tables.
3. Dropdowns loaded into `ViewData` by a private helper (scoped to manageable municipalities); a `[NotMapped]` `IFormFile` on the entity if the form uploads an image (see the `image-upload` skill).
4. A link to the new pages in the admin navigation.

Before finishing, check:

- Every query is filtered with `IMunicipalityAccessService`.
- Every POST re-checks access for the existing record **and** any newly selected municipality/position.
- `dotnet build` succeeds with no new warnings.
