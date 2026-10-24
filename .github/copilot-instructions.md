# Municipal Elections Management System – AI Coding Instructions

These instructions apply to every AI-assisted change in this repository (GitHub Copilot, and Claude Code via `CLAUDE.md`).

## What this app is

An ASP.NET Core MVC site for municipal elections (next election: October 17, 2026).

- **Public site** (anonymous): browse municipalities and positions, search/filter candidates, view candidate profiles, build a personal voting guide and print it.
- **Admin area** (`/Admin`, login required): dashboard counts plus CRUD for municipalities (with logo upload), positions, and candidates (with photo upload).
- **Mobile API** (`/api`, COMP4977): JSON endpoints for the SwiftUI "Municipal Elections Companion" app. Read-only election data is anonymous; the app's voting guide (`/api/savedcandidates`) needs a JWT. See the `mobile-api` skill and `docs/api-endpoints.md`.

## Stack

- .NET 10, ASP.NET Core MVC + Razor Pages (Identity UI only)
- Entity Framework Core 10, **SQLite**, **Code First** with migrations
- ASP.NET Core Identity with roles (`IdentityUser`, `IdentityRole`); cookie login for the website, JWT bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) for the API
- Bootstrap 5 + jQuery Unobtrusive Validation (already in `wwwroot/lib`)
- Deployed to Azure App Service (Linux)

## Project layout

| Folder | Contents |
|---|---|
| `Models/` | EF entities: `Municipality`, `Position`, `Candidate` (uploads bind to `[NotMapped]` `LogoFile` / `PhotoFile`), `SavedCandidate` (mobile voting guide) |
| `Models/Api/` | API request classes and response records (DTOs) |
| `Data/` | `ApplicationDbContext`, `SeedData`, `AppRoles` |
| `Services/` | `MunicipalityAccessService`, `ImageStorageService`, `VotingGuideService`, `JwtTokenService` |
| `Controllers/` + `Views/` | Public site |
| `Controllers/Api/` | Mobile API controllers (`*ApiController`) |
| `Areas/Admin/` | Admin controllers and views |
| `Areas/Identity/` | Overridden Identity pages (username login, disabled register) |
| `Migrations/` | EF Core migrations – generated, never hand-written |

## Conventions

- Namespace root is `Municipal_Elections_Management_System` (underscores). Use file-scoped namespaces.
- PascalCase for types, members, and files. Inject dependencies through a constructor into `private readonly` `_camelCase` fields (`_context`, `_access`, `_images`).
- Short lowercase comments above actions and non-obvious blocks (`// GET: Admin/Positions`, `// admins only`).
- Nullable reference types are enabled. Don't silence warnings with `!` unless the value is guaranteed.
- Async all the way: EF calls use `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`; method names end in `Async` except controller actions.
- Use the spec's spelling of positions: "Mayor", "Councilor", "School Trustee".

## Data model rules

- `Municipality 1 → * Position 1 → * Candidate`. A candidate's municipality is `candidate.Position.Municipality`. **Do not add `MunicipalityId` to `Candidate`.** The API's `CandidateDto` exposes a derived `municipalityId` instead.
- `SavedCandidate` links an `IdentityUser` to a `Candidate` (unique per user + candidate, cascade delete on both). It is only for the mobile app; the website's voting guide stays cookie based with no login.
- Foreign keys used in forms are `int?` with `[Required]` so an empty "Select…" option fails validation cleanly.
- `Municipality.AdminRoleName` is set by the system (`AppRoles.ForMunicipality(name)`), never bound from a form.
- To change the schema: edit the model, then `dotnet ef migrations add <PascalCaseName>`. Never edit a migration that has already been pushed.
- Seed data lives in `Data/SeedData.cs` and must stay idempotent (only insert what is missing).
- The connection string lives only in `appsettings.Development.json` (git-ignored) locally and in App Service configuration on Azure. Never put it in `appsettings.json`. Never commit `*.db` files.
- The same goes for the JWT signing key `Jwt:Key` (at least 32 characters; `Jwt__Key` on Azure). Only the non-secret `Jwt:Issuer`, `Jwt:Audience` and `Jwt:ExpiresInMinutes` are in `appsettings.json`.

## Authorization rules (critical)

- Roles: `AppRoles.SuperAdmin` ("Super Admin") and one group per municipality, `AppRoles.ForMunicipality(name)` (e.g. "Pitt Meadows Admin").
- Every admin controller has `[Area("Admin")]` and `[Authorize(Policy = AppRoles.AdminPolicy)]`. The policy requires at least one role, so accounts registered through the mobile API (which have no roles) can't reach `/Admin`.
- Creating and deleting municipalities: `[Authorize(Roles = AppRoles.SuperAdmin)]`.
- **Every** admin query for municipalities, positions or candidates is filtered by `IMunicipalityAccessService.GetManageableMunicipalityIdsAsync(User)`.
- **Every** admin POST re-checks `CanManageAsync(User, municipalityId)` for the record being changed **and** for any new municipality/position chosen in the form. On failure return `Forbid()`. Never trust IDs from hidden fields or dropdowns.
- Dashboard counts are scoped the same way.
- Public controllers never require login and never expose admin-only data.
- API controllers that need a login use `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`, never a plain `[Authorize]` (that would use the cookie scheme and redirect to the HTML login page instead of returning 401). Never make JWT the default scheme; the website depends on the cookie default.
- API endpoints return DTOs from `Models/Api/`, never EF entities (keeps `AdminRoleName` private and avoids reference cycles).
- `/api/savedcandidates` queries are always filtered by the `sub` claim of the token; another user's row returns 404.

## Validation

- Put data annotations on the entities; the same attributes drive client and server validation.
- Every form view includes `<partial name="_ValidationScriptsPartial" />` in `@section Scripts` and an `asp-validation-summary`.
- Every MVC/form POST action has `[ValidateAntiForgeryToken]` and checks `ModelState.IsValid` before saving.
- API controllers use `[ApiController]`, which returns a 400 `ValidationProblemDetails` automatically when data annotations fail. They don't use antiforgery tokens (they're called with a JWT, not a cookie). Return errors with `Problem(title: ..., statusCode: ...)` so the app always gets ProblemDetails JSON.
- Image uploads: `.jpg`, `.jpeg`, `.png`, `.webp` only, max 2 MB, checked in the browser by `wwwroot/js/image-validation.js` and on the server by `ImageStorageService.CheckImage` (see the `image-upload` skill).

## UI

- Bootstrap 5 classes only; avoid inline styles. Put shared CSS in `wwwroot/css/site.css`.
- Layouts must work at phone width (use the grid, `table-responsive`, cards).
- Show a placeholder image when `PhotoFileName` or `LogoFileName` is null.
- Print pages use a dedicated layout and `@media print` rules; hide navigation and buttons when printing.

## Don't

- Don't add packages without a clear need; prefer what ASP.NET Core already provides.
- Don't scaffold the full Identity UI; only override the pages we need.
- Don't put anything but dropdown `SelectList`s and simple counts in `ViewData`/`ViewBag`.
- Don't catch and swallow exceptions.
