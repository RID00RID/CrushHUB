using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CrushHUB.Domain.Entities;

namespace CrushHUB.Services;

/// <summary>
/// Шлёт уведомления в канал Discord через вебхук проекта.
/// Приём данных от игры не должен падать из-за недоступного Discord,
/// поэтому любые ошибки отправки только пишутся в лог.
/// </summary>
public class DiscordNotifier
{
    private const int CrashColor = 0xF85149;
    private const int ReportColor = 0xD29922;
    private const int DescriptionLimit = 1000;

    private readonly IHttpClientFactory _clients;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DiscordNotifier> _log;

    public DiscordNotifier(IHttpClientFactory clients, IWebHostEnvironment environment, ILogger<DiscordNotifier> log)
    {
        _clients = clients;
        _environment = environment;
        _log = log;
    }

    public Task NotifyCrashAsync(Project project, Crash crash, string? systemId, string baseUrl)
    {
        if (!project.NotifyOnCrash)
            return Task.CompletedTask;

        List<object> fields = [];

        AddField(fields, "Версия", crash.Version);
        AddField(fields, "Платформа", crash.Platform);
        AddField(fields, "System ID", systemId);

        return SendAsync(project, new
        {
            embeds = new[]
            {
                new
                {
                    title = Trim(crash.Title, 250),
                    url = $"{baseUrl}/Home/Crashes/{project.Id}?crash={crash.Id}",
                    description = Code(crash.Callstack),
                    color = CrashColor,
                    fields = fields.ToArray(),
                    footer = new { text = $"CrashHub · {project.Name}" },
                    timestamp = crash.OccurredAt.ToUniversalTime().ToString("o")
                }
            }
        });
    }

    public Task NotifyReportAsync(Project project, UserReport report, string? systemId, string baseUrl)
    {
        if (!project.NotifyOnReport)
            return Task.CompletedTask;

        List<object> fields = [];

        AddField(fields, "System ID", systemId);

        // Скриншот прикладываем файлом: панель может быть недоступна из интернета,
        // и ссылку на /uploads Discord тогда не заберёт.
        string? screenshot = ResolveScreenshot(report.ScreenshotPath);

        object payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = Trim(report.Category, 250),
                    url = $"{baseUrl}/Home/Reports/{project.Id}?report={report.Id}",
                    description = Trim(report.Description, DescriptionLimit),
                    color = ReportColor,
                    fields = fields.ToArray(),
                    image = screenshot is null ? null : new { url = $"attachment://{Path.GetFileName(screenshot)}" },
                    footer = new { text = $"CrashHub · {project.Name}" },
                    timestamp = report.CreatedAt.ToUniversalTime().ToString("o")
                }
            }
        };

        return SendAsync(project, payload, screenshot);
    }

    /// <summary>Путь к файлу скриншота на диске или null, если его нет.</summary>
    private string? ResolveScreenshot(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        string full = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        return File.Exists(full) ? full : null;
    }

    /// <summary>Проверочное сообщение из настроек проекта: здесь ошибку показываем администратору.</summary>
    public async Task<string?> SendTestAsync(string webhookUrl, string projectName)
    {
        object payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = "CrashHub на связи",
                    description = $"Уведомления проекта «{projectName}» настроены.",
                    color = 0x238636
                }
            }
        };

        try
        {
            HttpResponseMessage response = await Post(webhookUrl, payload);

            return response.IsSuccessStatusCode
                ? null
                : $"Discord ответил {(int)response.StatusCode}";
        }
        catch (Exception error)
        {
            return error.Message;
        }
    }

    private async Task SendAsync(Project project, object payload, string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(project.DiscordWebhookUrl))
            return;

        try
        {
            HttpResponseMessage response = await Post(project.DiscordWebhookUrl, payload, filePath);

            if (!response.IsSuccessStatusCode)
                _log.LogWarning("Discord вернул {Status} для проекта {Project}", response.StatusCode, project.Id);
        }
        catch (Exception error)
        {
            _log.LogWarning(error, "Не удалось отправить уведомление в Discord для проекта {Project}", project.Id);
        }
    }

    private async Task<HttpResponseMessage> Post(string webhookUrl, object payload, string? filePath = null)
    {
        HttpClient client = _clients.CreateClient(nameof(DiscordNotifier));
        client.Timeout = TimeSpan.FromSeconds(30);

        string json = JsonSerializer.Serialize(payload, JsonOptions);

        if (filePath is null)
            return await client.PostAsync(webhookUrl, new StringContent(json, Encoding.UTF8, "application/json"));

        using MultipartFormDataContent form = new();
        form.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

        ByteArrayContent file = new(await File.ReadAllBytesAsync(filePath));
        file.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeOf(filePath));
        form.Add(file, "files[0]", Path.GetFileName(filePath));

        return await client.PostAsync(webhookUrl, form);
    }

    private static string ContentTypeOf(string path) =>
        Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";

    /// <summary>Discord не принимает null-поля в embed, поэтому пустые не сериализуем.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static void AddField(List<object> fields, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            fields.Add(new { name, value = Trim(value, 100), inline = true });
    }

    private static string? Code(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : $"```\n{Trim(text, DescriptionLimit)}\n```";

    private static string Trim(string text, int limit) =>
        text.Length <= limit ? text : text[..limit] + "…";
}
