using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CrushHUB.Models.Api;

/// <summary>Тело запроса на приём обращения игрока.</summary>
public class ReportIngestRequest
{
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>Идентификатор машины игрока. Можно не слать, если он есть в userConfig.SystemID.</summary>
    [StringLength(100)]
    public string? UserId { get; set; }

    /// <summary>Когда игрок отправил обращение. Если не прислали — возьмём время получения.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>Скриншот в base64 (можно с префиксом data:image/png;base64,). PNG или JPEG, до 5 МБ.</summary>
    public string? Screenshot { get; set; }

    /// <summary>Конфигурация машины игрока: JSON-объект, сохраняем как есть.</summary>
    public JsonElement? UserConfig { get; set; }
}
