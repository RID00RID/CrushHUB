namespace CrushHUB.Services;

public readonly record struct ScreenshotSaveResult(string? Path, string? Error);

/// <summary>
/// Складывает присланный игрой скриншот в wwwroot/uploads/{projectId} и отдаёт путь для страницы.
/// Принимаем только PNG и JPEG — проверяем по сигнатуре, а не по тому, что написал клиент.
/// </summary>
public class ScreenshotStorage
{
    public const int MaxBytes = 5 * 1024 * 1024;

    private const string FolderName = "uploads";

    private readonly IWebHostEnvironment _environment;

    public ScreenshotStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<ScreenshotSaveResult> SaveAsync(int projectId, string base64)
    {
        string payload = base64.Trim();

        int comma = payload.IndexOf(',');

        if (payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
            payload = payload[(comma + 1)..];

        byte[] bytes;

        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch (FormatException)
        {
            return new ScreenshotSaveResult(null, "Скриншот не является корректным base64");
        }

        if (bytes.Length == 0)
            return new ScreenshotSaveResult(null, "Скриншот пустой");

        if (bytes.Length > MaxBytes)
            return new ScreenshotSaveResult(null, $"Скриншот больше {MaxBytes / (1024 * 1024)} МБ");

        string? extension = DetectExtension(bytes);

        if (extension is null)
            return new ScreenshotSaveResult(null, "Поддерживаются только PNG и JPEG");

        string folder = Path.Combine(_environment.WebRootPath, FolderName, projectId.ToString());
        Directory.CreateDirectory(folder);

        string fileName = $"{Guid.NewGuid():N}{extension}";

        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), bytes);

        return new ScreenshotSaveResult($"/{FolderName}/{projectId}/{fileName}", null);
    }

    private static string? DetectExtension(byte[] bytes)
    {
        if (bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return ".png";

        if (bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return ".jpg";

        return null;
    }
}
