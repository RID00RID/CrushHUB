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
    private readonly ILogger<DiscordNotifier> _log;

    public DiscordNotifier(IHttpClientFactory clients, ILogger<DiscordNotifier> log)
    {
        _clients = clients;
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

        return SendAsync(project, new
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
                    footer = new { text = $"CrashHub · {project.Name}" },
                    timestamp = report.CreatedAt.ToUniversalTime().ToString("o")
                }
            }
        });
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

    private async Task SendAsync(Project project, object payload)
    {
        if (string.IsNullOrWhiteSpace(project.DiscordWebhookUrl))
            return;

        try
        {
            HttpResponseMessage response = await Post(project.DiscordWebhookUrl, payload);

            if (!response.IsSuccessStatusCode)
                _log.LogWarning("Discord вернул {Status} для проекта {Project}", response.StatusCode, project.Id);
        }
        catch (Exception error)
        {
            _log.LogWarning(error, "Не удалось отправить уведомление в Discord для проекта {Project}", project.Id);
        }
    }

    private async Task<HttpResponseMessage> Post(string webhookUrl, object payload)
    {
        HttpClient client = _clients.CreateClient(nameof(DiscordNotifier));
        client.Timeout = TimeSpan.FromSeconds(10);

        string json = JsonSerializer.Serialize(payload);

        return await client.PostAsync(webhookUrl, new StringContent(json, Encoding.UTF8, "application/json"));
    }

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
