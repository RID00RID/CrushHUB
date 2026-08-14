using CrushHUB.Domain.Entities;

namespace CrushHUB.Models;

/// <summary>Порядок списка обращений.</summary>
public enum ReportSort
{
    Date = 0,
    Status = 1
}

public class ReportsViewModel
{
    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    /// <summary>Выбранная категория: null — «Все».</summary>
    public string? Category { get; init; }

    /// <summary>Выбранный статус: null — «Все».</summary>
    public ReportStatus? Status { get; init; }

    public ReportSort Sort { get; init; }

    /// <summary>Категории, которые реально встречаются в обращениях проекта.</summary>
    public IReadOnlyList<string> Categories { get; init; } = [];

    public IReadOnlyList<ReportListItemViewModel> Reports { get; init; } = [];
}

public class ReportListItemViewModel
{
    public int Id { get; init; }

    public string Category { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public ReportStatus Status { get; init; }

    /// <summary>Идентификатор машины игрока, пусто — если обращение пришло без него.</summary>
    public string? SystemId { get; init; }

    public int? GameUserId { get; init; }

    public DateTime CreatedAt { get; init; }
}

public class ReportDetailViewModel : ReportListItemViewModel
{
    public int ProjectId { get; init; }

    public string? ScreenshotPath { get; init; }
}

/// <summary>Подписи, порядок и цветовой тон статусов обращения.</summary>
public static class ReportStatusView
{
    public static readonly IReadOnlyList<ReportStatus> All =
        [ReportStatus.Open, ReportStatus.Resolved, ReportStatus.WontFix, ReportStatus.Spam];

    public static string Display(ReportStatus status) => status switch
    {
        ReportStatus.Open => "Открыто",
        ReportStatus.Resolved => "Решено",
        ReportStatus.WontFix => "Не будет исправлено",
        ReportStatus.Spam => "Спам",
        _ => status.ToString()
    };

    public static string Tone(ReportStatus status) => status switch
    {
        ReportStatus.Open => "warning",
        ReportStatus.Resolved => "success",
        ReportStatus.WontFix => "muted",
        ReportStatus.Spam => "danger",
        _ => "muted"
    };
}

public static class ReportSortView
{
    public static readonly IReadOnlyList<ReportSort> All = [ReportSort.Date, ReportSort.Status];

    public static string Display(ReportSort sort) => sort switch
    {
        ReportSort.Status => "По статусу",
        _ => "По дате"
    };
}
