---
description: Make a schema change the Code First way and generate a migration
agent: agent
---

Apply this schema change: ${input:change:Describe the change, e.g. add a Twitter URL to Candidate}

Steps:

1. Edit the entity in `Models/` (and add data annotations for validation).
2. If the change needs relationship or index configuration, update `OnModelCreating` in `Data/ApplicationDbContext.cs`.
3. Update `Data/SeedData.cs` if seeded rows need the new data. Keep it idempotent.
4. Run `dotnet ef migrations add ${input:name:PascalCase migration name}`.
5. Read the generated migration and confirm it only contains the intended change (no dropped tables or data loss).
6. Run the app once so the migration is applied at startup, and confirm it starts without errors.

Never edit migrations that are already pushed; add a new one instead.
