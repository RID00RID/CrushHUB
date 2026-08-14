namespace CrushHUB.Domain.Entities;

/// <summary>
/// Пользователь игры — это машина, с которой пришли данные. Опознаётся по SystemID,
/// конфигурация хранится здесь один раз, а краши и обращения ссылаются на неё.
/// </summary>
public class GameUser : BaseEntity
{
    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    /// <summary>Уникальный идентификатор машины, присылает игра.</summary>
    public string SystemId { get; set; } = string.Empty;

    public string? OsName { get; set; }

    public string? OsVersion { get; set; }

    public string? Cpu { get; set; }

    public string? Gpu { get; set; }

    public int? MemoryMb { get; set; }

    /// <summary>Последняя присланная конфигурация целиком — чтобы не терять незнакомые поля.</summary>
    public string? ConfigJson { get; set; }

    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
