---
name: voting-guide
description: How the anonymous voting guide stores selected candidates with VotingGuideService and how the printable guide is built. Use when working on VotingGuideController, the "Add to guide" button (_GuideButton), the navbar guide count, or the print page.
---

# Voting guide

Visitors don't log in. Their selected candidate IDs are stored in a cookie by `VotingGuideService` (`Services/VotingGuideService.cs`), registered as scoped with `AddHttpContextAccessor()` in `Program.cs`.

## VotingGuideService

| Member | What it does |
|---|---|
| `CookieName` | `"VotingGuide"`; value is comma-separated IDs, e.g. `4,12,17` |
| `GetIds()` | Reads the cookie; skips anything that isn't a number and duplicates |
| `Add(int id)` / `Remove(int id)` | Updates the list and re-saves the cookie |
| `Clear()` | Deletes the cookie |

The cookie is `HttpOnly` and expires after 60 days. IDs whose candidate has been deleted are simply not shown, because the guide only loads candidates that exist.

## VotingGuideController (public, no `[Authorize]`)

| Action | Verb | Notes |
|---|---|---|
| `Index` | GET | Loads the selected candidates with `Position.Municipality` |
| `Add(int id, string? returnUrl)` | POST | `[ValidateAntiForgeryToken]`; returns to `returnUrl` only if `Url.IsLocalUrl` |
| `Remove(int id, string? returnUrl)` | POST | Same as `Add` |
| `Clear` | POST | Empties the guide, back to `Index` |
| `Print` | GET | Uses `_PrintLayout`; sets `ViewData["PrintDate"]` |

## Views

- `Views/Shared/_GuideButton.cshtml` (model: `Candidate`) shows "Add to guide" or "Remove from guide" as a POST form and passes the current path + query string as `returnUrl`. Use it on any candidate card or details page: `<partial name="_GuideButton" model="candidate" />`.
- `_Layout.cshtml` injects `VotingGuideService` and shows the count in the "My Guide" navbar badge.

## Printable guide must include

- Municipality name and election date (one section per municipality)
- Each selected candidate's name and position (photo optional)
- Print date in Pacific time: `TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Vancouver")`, because Azure servers run on UTC

`_PrintLayout` has no navbar or footer. The "Print" button calls `window.print()` and is wrapped in Bootstrap's `d-print-none` so it doesn't appear on paper.
