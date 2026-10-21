---
description: Add a seeded municipality with positions and fictional candidates
agent: agent
---

Add seed data for the municipality `${input:municipality:Municipality name, e.g. Surrey}` in `Data/SeedData.cs`.

- Use `BuildMunicipality` like the existing entries, with the election date October 17, 2026 and province British Columbia.
- Positions: Mayor (1 seat), Councilor and School Trustee with realistic seat counts for that municipality.
- 2–3 candidates per position. **Candidates must be fictional** – never use names of real politicians or real people.
- Use `@example.com` emails and `https://example.com/...` websites only.
- Keep seeding idempotent: existing databases must not get duplicates. If the existing early-return on `Municipalities.AnyAsync()` would skip the new municipality, change it to check by name.
- The municipality's admin role is created from `AdminRoleName` by `SeedRolesAsync`; confirm it appears in `AspNetRoles` after running the app.
