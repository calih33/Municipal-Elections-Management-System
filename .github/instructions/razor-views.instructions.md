---
applyTo: "**/*.cshtml"
---

# Razor view rules

- Use tag helpers (`asp-for`, `asp-action`, `asp-area`, `asp-route-*`), not hand-built URLs or `Html.*` helpers.
- Always set `ViewData["Title"]`.
- Forms: `method="post"`, `enctype="multipart/form-data"` when uploading, `asp-validation-summary="ModelOnly"`, and a `<span asp-validation-for>` under every input.
- File inputs set `accept=".jpg,.jpeg,.png,.webp"`.
- Include validation scripts at the bottom:

  ```cshtml
  @section Scripts {
      <partial name="_ValidationScriptsPartial" />
  }
  ```

- Images: `<img src="@Url.Content(path)" alt="@Model.Name" class="img-fluid">`, falling back to `~/images/placeholder-*.svg` when the file name is null.
- Links that leave the admin area set `asp-area=""`.
- Destructive actions (delete, remove from guide) are POST forms with a button, never GET links.
