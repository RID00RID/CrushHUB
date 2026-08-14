namespace CrushHUB.Domain.Entities;

public enum CrashStatus
{
    Open = 0,
    Resolved = 1
}

/// <summary>Краш, присланный игрой через API.</summary>
public class Crash : BaseEntity
{
    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    /// <summary>Машина, с которой пришёл краш. Пусто, если игра не прислала SystemID.</summary>
    public int? GameUserId { get; set; }

    public GameUser? GameUser { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Callstack { get; set; }

    public string? Version { get; set; }

    public string? Platform { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public CrashStatus Status { get; set; } = CrashStatus.Open;
}
