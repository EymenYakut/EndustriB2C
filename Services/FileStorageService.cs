using System.Text;
using System.Text.RegularExpressions;

namespace EndustriB2C.Services;

public class FileStorageService
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveProductImageAsync(IFormFile file) =>
        await SaveImageAsync(file, "products");

    public async Task<string> SaveCategoryImageAsync(IFormFile file) =>
        await SaveImageAsync(file, "categories");

    private async Task<string> SaveImageAsync(IFormFile file, string folderName)
    {
        if (file.Length is <= 0 or > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException("Dosya boyutu 1 byte ile 5 MB arasında olmalıdır.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!Allowed.Contains(ext))
        {
            throw new InvalidOperationException("Sadece jpg, png ve webp dosyaları yüklenebilir.");
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header);
        if (!IsImage(header, read))
        {
            throw new InvalidOperationException("Dosya geçerli bir görsel değil.");
        }

        var folder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", folderName);
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var path = Path.Combine(folder, name);
        await using (var fs = File.Create(path))
        {
            stream.Position = 0;
            await stream.CopyToAsync(fs);
        }

        return $"/uploads/{folderName}/{name}";
    }

    public void DeleteIfLocal(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !filePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relative = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var full = Path.Combine(_env.ContentRootPath, "wwwroot", relative);
        if (File.Exists(full))
        {
            File.Delete(full);
        }
    }

    public static string Slugify(string value)
    {
        var map = new Dictionary<char, string>
        {
            ['ç'] = "c", ['ğ'] = "g", ['ı'] = "i", ['ö'] = "o", ['ş'] = "s", ['ü'] = "u",
            ['Ç'] = "c", ['Ğ'] = "g", ['İ'] = "i", ['Ö'] = "o", ['Ş'] = "s", ['Ü'] = "u"
        };
        var sb = new StringBuilder();
        foreach (var c in value.Trim().ToLowerInvariant())
        {
            if (map.TryGetValue(c, out var r)) sb.Append(r);
            else if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (c is ' ' or '-' or '_') sb.Append('-');
        }
        var slug = Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "urun" : slug;
    }

    private static bool IsImage(byte[] h, int len)
    {
        if (len < 4) return false;
        if (h[0] == 0xFF && h[1] == 0xD8) return true;
        if (h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return true;
        if (len >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return true;
        return false;
    }
}
