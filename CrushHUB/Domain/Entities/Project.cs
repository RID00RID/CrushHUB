namespace CrushHUB.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Площадка проекта, пока задаётся значением по умолчанию.</summary>
    public string Platform { get; set; } = "PC";

    /// <summary>Ключ приложения для SDK. Хранится как есть — его показывают в настройках проекта.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Вебхук канала Discord: пусто — уведомления выключены.</summary>
    public string? DiscordWebhookUrl { get; set; }

    public bool NotifyOnCrash { get; set; } = true;

    public bool NotifyOnReport { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
