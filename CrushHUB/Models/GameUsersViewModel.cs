namespace CrushHUB.Models;

public class GameUsersViewModel
{
    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public string? Search { get; init; }

    public IReadOnlyList<GameUserListItemViewModel> Users { get; init; } = [];
}

public class GameUserListItemViewModel
{
    public int Id { get; init; }

    public string SystemId { get; init; } = string.Empty;

    public int CrashCount { get; init; }

    public int ReportCount { get; init; }
}

public class GameUserDetailViewModel
{
    public int ProjectId { get; init; }

    public int Id { get; init; }

    public string SystemId { get; init; } = string.Empty;

    public DateTime FirstSeenAt { get; init; }

    public DateTime LastSeenAt { get; init; }

    public UserConfigViewModel? Config { get; init; }

    public IReadOnlyList<CrashListItemViewModel> Crashes { get; init; } = [];

    public IReadOnlyList<ReportListItemViewModel> Reports { get; init; } = [];
}
