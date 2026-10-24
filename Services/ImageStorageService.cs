namespace Municipal_Elections_Management_System.Services;

// saves and deletes uploaded logos and photos
// local: wwwroot/uploads, azure: Uploads:Path = /home/data/uploads
public class ImageStorageService
{
    public string RootPath { get; }

    public ImageStorageService(IWebHostEnvironment env, IConfiguration configuration)
    {
        RootPath = configuration["Uploads:Path"] ?? Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(RootPath);
    }

    // returns an error message, or null if ok
    public string? CheckImage(IFormFile? file)
    {
        // image is optional
        if (file == null)
        {
            return null;
        }

        if (file.Length == 0)
        {
            return "The image file is empty.";
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowed.Contains(extension) || !file.ContentType.StartsWith("image/"))
        {
            return "Only .jpg, .jpeg, .png or .webp images are allowed.";
        }

        // 2 MB max
        if (file.Length > 2 * 1024 * 1024)
        {
            return "The image must be 2 MB or smaller.";
        }

        return null;
    }

    // save with a random name, returns e.g. "logos/abc123.png"
    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        var fileName = Guid.NewGuid().ToString() + extension;

        var folderPath = Path.Combine(RootPath, folder);
        Directory.CreateDirectory(folderPath);

        using (var stream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return folder + "/" + fileName;
    }

    // delete an image file
    public void Delete(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var fullPath = Path.Combine(RootPath, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
