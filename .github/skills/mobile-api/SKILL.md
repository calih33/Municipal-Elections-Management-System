---
name: mobile-api
description: How the JSON API for the SwiftUI mobile app is built - JWT auth alongside the website's cookie login, DTOs instead of entities, ProblemDetails errors and the per-user SavedCandidates voting guide. Use when adding or changing anything under Controllers/Api, Models/Api, JwtTokenService or the Jwt settings.
---

# Mobile API

The COMP4977 iOS app ("Municipal Elections Companion") talks to `/api/*` in this same app and database. The full contract the app relies on is in `docs/api-endpoints.md`, so keep it in sync with any change here.

## Authentication: two schemes, cookie stays the default

`Program.cs` calls `AddDefaultIdentity` (cookie, the default scheme), then `AddAuthentication().AddJwtBearer(...)` **with no default scheme argument**. Passing `JwtBearerDefaults.AuthenticationScheme` to `AddAuthentication` would break the website's admin login.

| Where | Attribute |
|---|---|
| Public API endpoints | none (anonymous) |
| Protected API endpoints | `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` |
| Admin area | `[Authorize(Policy = AppRoles.AdminPolicy)]` (cookie, needs at least one role) |

A plain `[Authorize]` on an API controller uses the cookie scheme and answers with a 302 to the login page instead of a 401. Don't use it.

- `JwtBearerOptions.MapInboundClaims = false`, so claims keep their JWT names. Read the user id with `User.FindFirstValue(JwtRegisteredClaimNames.Sub)`. `User.Identity.Name` is the `name` claim.
- `JwtTokenService.CreateToken(user, name)` issues tokens with `sub`, `email`, `name` and `jti`. It is signed with HMAC-SHA256 using `Jwt:Key`.
- `Jwt:Key` must be at least 32 characters and is a secret. It goes in `appsettings.Development.json` locally (the `.example` file has a placeholder) and in the `Jwt__Key` App Service setting on Azure. The app refuses to start without it.

## Users

- Register sets `UserName = Email`. `IdentityUser` has no name column, so the display name is saved as a `name` user claim (`AddClaimAsync`). Seeded admins have no such claim and fall back to `UserName`.
- Mobile users get **no roles**. That is what keeps them out of `/Admin` (see `AppRoles.AdminPolicy`).
- Login uses `CheckPasswordSignInAsync(..., lockoutOnFailure: true)`.

## Controllers (`Controllers/Api/*ApiController.cs`)

- Every API controller has `[ApiController]` and `[Route("api/<resource>")]`. Class names end in `ApiController` so they never clash with the MVC controllers of the same name during link generation.
- Return `ActionResult<TDto>`. Map entities with the static `From(...)` methods in `Models/Api/ElectionDtos.cs` after `ToListAsync()`. Never return an EF entity.
- Use `Include(...).ThenInclude(...)` for `Position.Municipality` before mapping a `CandidateDto`. The mapper throws if it wasn't loaded.
- Return errors with `Problem(title: "...", statusCode: StatusCodes.Status404NotFound)`. For field errors, use `ModelState.AddModelError(field, message)` followed by `return ValidationProblem(ModelState)`.
- Image URLs: `ApiUrls.Upload(Request, fileName)` builds an absolute `/uploads/...` URL, or returns `null`.

## SavedCandidates

- Entity: `SavedCandidateId`, `UserId` (FK to `AspNetUsers`), `CandidateId`, `DateSaved`.
  - It has a unique index on `(UserId, CandidateId)`.
  - Rows cascade-delete with the user or the candidate.
- Every query filters on the token's `sub`. Copy it into a local variable before the LINQ query.
- `POST` returns 404 for an unknown candidate and 409 for a duplicate. `DELETE /{id}` takes the `savedCandidateId` and returns 404 if the row isn't the caller's.
- The website's anonymous cookie voting guide (`voting-guide` skill) does **not** use this table. COMP4976 requires that guide to work without login.

## Field names

The API uses the COMP4977 spec's names. The database keeps the COMP4976 names, so don't rename columns to match the API.

| API (DTO) | Entity |
|---|---|
| `biography` | `Candidate.Profile` |
| `photoUrl` | `Candidate.PhotoFileName` |
| `logoUrl` | `Municipality.LogoFileName` |
| `municipalityId` on a candidate | `Candidate.Position.MunicipalityId` |
| `electionDate` (`DateOnly`, `"yyyy-MM-dd"`) | `Municipality.ElectionDate` |
