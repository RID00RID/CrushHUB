using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class HomeController
{
    [HttpGet]
    public async Task<IActionResult> Users(int id, string? search, int? user)
    {
        Project? project = await LoadProjectPage(id, AppTab.Users);

        if (project is null)
            return RedirectToAction(nameof(Index));

        if (user is not null)
            return await GameUserDetail(project, user.Value);

        List<GameUser> users = await _gameUsers.FindAsync(u => u.ProjectId == project.Id);
        List<Crash> crashes = await _crashes.FindAsync(c => c.ProjectId == project.Id);
        List<UserReport> reports = await _reports.FindAsync(r => r.ProjectId == project.Id);

        IEnumerable<GameUser> found = users;

        if (!string.IsNullOrWhiteSpace(search))
            found = found.Where(u => u.SystemId.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

        return View(new GameUsersViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Search = search,
            Users = found
                .Select(u => new GameUserListItemViewModel
                {
                    Id = u.Id,
                    SystemId = u.SystemId,
                    CrashCount = crashes.Count(c => c.GameUserId == u.Id),
                    ReportCount = reports.Count(r => r.GameUserId == u.Id)
                })
                .OrderByDescending(u => u.CrashCount + u.ReportCount)
                .ThenBy(u => u.SystemId)
                .ToList()
        });
    }

    private async Task<IActionResult> GameUserDetail(Project project, int gameUserId)
    {
        GameUser? user = await _gameUsers.GetByIdAsync(gameUserId);

        if (user is null || user.ProjectId != project.Id)
            return RedirectToAction(nameof(Users), new { id = project.Id });

        List<Crash> crashes = await _crashes.FindAsync(c => c.GameUserId == user.Id);
        List<UserReport> reports = await _reports.FindAsync(r => r.GameUserId == user.Id);

        Dictionary<int, string> systemIds = new() { [user.Id] = user.SystemId };

        return View("GameUserDetail", new GameUserDetailViewModel
        {
            ProjectId = project.Id,
            Id = user.Id,
            SystemId = user.SystemId,
            FirstSeenAt = user.FirstSeenAt,
            LastSeenAt = user.LastSeenAt,
            Config = UserConfigViewModel.Parse(user.ConfigJson),
            Crashes = crashes
                .OrderByDescending(c => c.OccurredAt)
                .Select(c => ToListItem(c, systemIds))
                .ToList(),
            Reports = reports
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => ToReportListItem(r, systemIds))
                .ToList()
        });
    }
}
