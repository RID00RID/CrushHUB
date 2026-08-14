using System.ComponentModel.DataAnnotations;

namespace CrushHUB.Models;

public class ProjectSettingsViewModel
{
    public int Id { get; init; }

    [Required(ErrorMessage = "Укажите название проекта")]
    [StringLength(100, ErrorMessage = "Название не длиннее 100 символов")]
    [Display(Name = "Название проекта")]
    public string? Name { get; set; }

    /// <summary>Адрес, который прописывают в плагине, чтобы он знал, куда слать краши.</summary>
    public string ServerUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    [Url(ErrorMessage = "Похоже, это не адрес вебхука")]
    [StringLength(300)]
    [Display(Name = "Вебхук Discord")]
    public string? DiscordWebhookUrl { get; set; }

    [Display(Name = "Краши")]
    public bool NotifyOnCrash { get; set; } = true;

    [Display(Name = "Обращения игроков")]
    public bool NotifyOnReport { get; set; } = true;

    /// <summary>Результат проверки вебхука: пусто — проверку не запускали.</summary>
    public string? TestResult { get; init; }

    public bool NotificationsSaved { get; init; }
}
