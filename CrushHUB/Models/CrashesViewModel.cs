using CrushHUB.Domain.Entities;

namespace CrushHUB.Models;

/// <summary>Порядок списка крашей по дате.</summary>
public enum CrashSort
{
    DateDesc = 0,
    DateAsc = 1
}

public class CrashesViewModel
{
    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Выбранный фильтр: null — «Все».</summary>
    public CrashStatus? Status { get; init; }

    public CrashSort Sort { get; init; }

    public IReadOnlyList<CrashListItemViewModel> Crashes { get; init; } = [];
}

/// <summary>Подписи вариантов сортировки.</summary>
public static class CrashSortView
{
    public static readonly IReadOnlyList<CrashSort> All = [CrashSort.DateDesc, CrashSort.DateAsc];

    public static string Display(CrashSort sort) => sort switch
    {
        CrashSort.DateAsc => "Сначала старые",
        _ => "Сначала новые"
    };
}

public class CrashListItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public CrashStatus Status { get; init; }

    public string? Version { get; init; }

    public string? Platform { get; init; }

    /// <summary>Идентификатор машины игрока, пусто — если краш пришёл без него.</summary>
    public string? SystemId { get; init; }

    public int? GameUserId { get; init; }

    public DateTime OccurredAt { get; init; }
}

public class CrashDetailViewModel : CrashListItemViewModel
{
    public int ProjectId { get; init; }

    public string? Callstack { get; init; }
}

/// <summary>Подписи и css-классы статусов краша — чтобы не размазывать их по разметке.</summary>
public static class CrashStatusView
{
    public static readonly IReadOnlyList<CrashStatus> All = [CrashStatus.Open, CrashStatus.Resolved];

    public static string Display(CrashStatus status) => status switch
    {
        CrashStatus.Open => "Открыт",
        CrashStatus.Resolved => "Решён",
        _ => status.ToString()
    };

    public static string Tone(CrashStatus status) => status switch
    {
        CrashStatus.Resolved => "success",
        _ => "warning"
    };
}
