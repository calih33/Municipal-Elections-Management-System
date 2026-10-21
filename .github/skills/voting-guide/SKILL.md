---
name: voting-guide
description: How the anonymous voting guide stores selected candidates and how the printable guide is built. Use when working on VotingGuideController, "Add to guide" buttons, or the print page.
---

# Voting guide

Visitors don't log in. Their selected candidate IDs are stored in a cookie.

## Storage

- Cookie name: `VotingGuide`; value: comma-separated candidate IDs (e.g. `4,12,17`).
- Options: `HttpOnly = true`, `IsEssential = true`, `SameSite = Lax`, `Secure = true`, expires after 60 days.
- Wrap reading/writing in a small service (e.g. `VotingGuideService` with `GetIds()`, `Add(int)`, `Remove(int)`, `Clear()`), registered as scoped and using `IHttpContextAccessor`.
- Ignore IDs that are not numbers or no longer exist in the database.

## Controller

`VotingGuideController` (public, no `[Authorize]`):

| Action | Verb | Notes |
|---|---|---|
| `Index` | GET | Load selected candidates with `Position.Municipality`, group by municipality then position. Warn when selections exceed `SeatsAvailable`. |
| `Add(int id, string? returnUrl)` | POST | Antiforgery token; redirect back with `LocalRedirect` (only local URLs). |
| `Remove(int id)` | POST | Same as above. |
| `Clear` | POST | Empties the guide. |
| `Print` | GET | Uses `_PrintLayout`. |

Show the selected count in the navbar via a view component.

## Printable guide must include

- Municipality name and election date (one section per municipality)
- Each selected candidate's name and position (photo optional)
- Print date in Pacific time: `TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Vancouver")` – Azure servers run on UTC.

`_PrintLayout` has no navbar or footer and includes a "Print" button (`window.print()`) hidden with `@media print { .no-print { display: none; } }`.
