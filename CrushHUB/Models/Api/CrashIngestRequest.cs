using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CrushHUB.Models.Api;

/// <summary>Тело запроса на приём краша от игры.</summary>
public class CrashIngestRequest
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Callstack { get; set; }

    [StringLength(50)]
    public string? Version { get; set; }

    [StringLength(50)]
    public string? Platform { get; set; }

    /// <summary>Идентификатор машины игрока. Можно не слать, если он есть в userConfig.SystemID.</summary>
    [StringLength(100)]
    public string? UserId { get; set; }

    /// <summary>Когда случился краш. Если не прислали — возьмём время получения.</summary>
    public DateTime? OccurredAt { get; set; }

    /// <summary>Конфигурация машины игрока: JSON-объект, сохраняем как есть.</summary>
    public JsonElement? UserConfig { get; set; }
}
