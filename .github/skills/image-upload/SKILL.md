---
name: image-upload
description: How to accept, validate, store and display municipality logos and candidate photos, both locally and on Azure App Service. Use when touching LogoFileName, PhotoFileName, or any IFormFile.
---

# Image uploads

## Restrictions (enforce on client and server)

- Extensions: `.jpg`, `.jpeg`, `.png`, `.webp`
- Max size: 2 MB
- Content type must start with `image/`

## Validation

Use a custom attribute on the view model's `IFormFile` property that implements both `ValidationAttribute` (server) and `IClientModelValidator` (adds `data-val-*` attributes), plus a small jQuery Unobtrusive adapter in `wwwroot/js/site.js` that checks `this.files[0].size` and the extension. Also set `accept=".jpg,.jpeg,.png,.webp"` on the input.

Entities never hold `IFormFile`; the view model does, and the controller copies the stored file name onto the entity.

## Storage

Put file handling behind one service (e.g. `IImageStorage` with `SaveAsync(IFormFile, string folder)` and `Delete(string? fileName)`), registered in `Program.cs`.

- Save with a new name: `$"{Guid.NewGuid():N}{extension}"` – never the user's file name.
- Folders: `logos/` and `candidates/`.
- Store only the generated relative name in the database (`LogoFileName`, `PhotoFileName`).
- When replacing or deleting a record, delete the old file.

### Where files live

- **Locally:** `wwwroot/uploads/` (git-ignored).
- **Azure App Service:** zip deployments make `wwwroot` read-only, so read the upload root from configuration (e.g. `Uploads:Path = /home/data/uploads`) and serve it with an extra `UseStaticFiles` + `PhysicalFileProvider` mapped to `/uploads`. `/home` persists across restarts and deployments.

## Display

```cshtml
<img src="@(Model.PhotoFileName is null ? Url.Content("~/images/placeholder-candidate.svg") : Url.Content($"~/uploads/{Model.PhotoFileName}"))"
     alt="Photo of @Model.Name" class="img-fluid rounded">
```

## Verify

Upload a valid PNG (succeeds), a 3 MB JPG (rejected in the browser and, with JS disabled, on the server), and a `.gif` renamed to `.png` with a non-image content type (rejected on the server).
