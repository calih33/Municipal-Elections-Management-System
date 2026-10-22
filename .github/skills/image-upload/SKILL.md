---
name: image-upload
description: How municipality logos and candidate photos are uploaded, checked, stored and displayed with ImageStorageService, locally and on Azure App Service. Use when touching LogoFile/LogoFileName, PhotoFile/PhotoFileName, or any IFormFile.
---

# Image uploads

All file handling goes through `ImageStorageService` (`Services/ImageStorageService.cs`), registered as a singleton in `Program.cs`.

| Method | What it does |
|---|---|
| `CheckImage(IFormFile?)` | Returns an error message, or `null` when the file is fine or absent (images are optional) |
| `SaveAsync(IFormFile, folder)` | Saves under a new GUID name and returns the relative path, e.g. `"candidates/3f2a….png"` |
| `Delete(string? fileName)` | Deletes a previously saved file; does nothing for `null` |
| `RootPath` | Folder that holds all uploads |

## Restrictions

- Extensions: `.jpg`, `.jpeg`, `.png`, `.webp`, and the content type must start with `image/`
- Max size: 2 MB
- Checked twice: in the browser by `wwwroot/js/image-validation.js`, and on the server by `CheckImage`

## Model binding

The upload lives on the entity as a `[NotMapped]` property next to the stored file name:

| Entity | Upload property (form) | Stored column |
|---|---|---|
| `Municipality` | `LogoFile` | `LogoFileName` |
| `Candidate` | `PhotoFile` | `PhotoFileName` |

Include the upload property in the action's `[Bind(...)]` list, never the file-name column.

## Controller pattern

```csharp
var photoError = _images.CheckImage(candidate.PhotoFile);
if (photoError != null)
{
    ModelState.AddModelError("PhotoFile", photoError);
}

if (ModelState.IsValid)
{
    if (candidate.PhotoFile != null)
    {
        candidate.PhotoFileName = await _images.SaveAsync(candidate.PhotoFile, "candidates");
    }
    ...
}
```

- Folders: `"logos"` for municipalities, `"candidates"` for candidates.
- On edit with a new file: `_images.Delete(existing.PhotoFileName)` first, then save and store the new name on the **existing** entity.
- On delete: delete the file before removing the record. Deleting a position or municipality cascades to its candidates in the database, so loop over them and delete their photos too.

## Views

- Forms with uploads use `enctype="multipart/form-data"` and a `<span asp-validation-for="PhotoFile">` (or `LogoFile`) under the input; `image-validation.js` writes its message into that span.
- Load the script after the validation scripts:

  ```cshtml
  @section Scripts {
      <partial name="_ValidationScriptsPartial" />
      <script src="~/js/image-validation.js" asp-append-version="true"></script>
  }
  ```

- Display: `~/uploads/{FileName}`, falling back to `~/images/placeholder-candidate.svg` or `~/images/placeholder-logo.svg` when the file name is null.

## Where files live

- **Locally:** `wwwroot/uploads/` (git-ignored).
- **Azure App Service:** set the app setting `Uploads__Path=/home/data/uploads`. Zip deployments make `wwwroot` read-only, and `/home` survives restarts and redeploys.
- `Program.cs` serves `RootPath` at `/uploads` with `UseStaticFiles` + `PhysicalFileProvider`, because `MapStaticAssets` only knows about files that existed at build time.

## Verify

Upload a valid PNG (succeeds), a 3 MB JPG (rejected in the browser, and on the server with JavaScript off), and a `.gif` (rejected). Replace and then delete the record, and confirm the old file is gone from the uploads folder.
