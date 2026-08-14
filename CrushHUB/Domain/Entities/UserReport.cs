namespace CrushHUB.Domain.Entities;

public enum ReportStatus
{
    Open = 0,
    Resolved = 1,
    WontFix = 2,
    Spam = 3
}

/// <summary>Обращение, отправленное игроком через форму в игре.</summary>
public class UserReport : BaseEntity
{
    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    /// <summary>Машина, с которой пришло обращение. Пусто, если игра не прислала SystemID.</summary>
    public int? GameUserId { get; set; }

    public GameUser? GameUser { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Путь к приложенному скриншоту относительно корня сайта.</summary>
    public string? ScreenshotPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ReportStatus Status { get; set; } = ReportStatus.Open;
}
