using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class HomeController
{
    [HttpGet]
    public async Task<IActionResult> Crashes(int id, CrashStatus? status, CrashSort sort, int? crash)
    {
        Project? project = await LoadProjectPage(id, AppTab.Crashes);

        if (project is null)
            return RedirectToAction(nameof(Index));

        if (crash is not null)
            return await CrashDetail(project, crash.Value);

        List<Crash> crashes = await _crashes.FindAsync(c => c.ProjectId == project.Id);
        Dictionary<int, string> systemIds = await LoadSystemIds(project.Id);

        if (status is not null)
            crashes = crashes.Where(c => c.Status == status).ToList();

        IEnumerable<Crash> ordered = sort == CrashSort.DateAsc
            ? crashes.OrderBy(c => c.OccurredAt)
            : crashes.OrderByDescending(c => c.OccurredAt);

        return View(new CrashesViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = status,
            Sort = sort,
            Crashes = ordered.Select(c => ToListItem(c, systemIds)).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCrashStatus(int id, int crashId, CrashStatus status)
    {
        Crash? crash = await _crashes.GetByIdAsync(crashId);

        if (crash is null || crash.ProjectId != id)
            return RedirectToAction(nameof(Crashes), new { id });

        crash.Status = status;

        _crashes.Update(crash);
        await _crashes.SaveChangesAsync();

        return RedirectToAction(nameof(Crashes), new { id, crash = crashId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteCrash(int id, int crashId)
    {
        Crash? crash = await _crashes.GetByIdAsync(crashId);

        if (crash is not null && crash.ProjectId == id)
        {
            _crashes.Delete(crash);
            await _crashes.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Crashes), new { id });
    }

    private async Task<IActionResult> CrashDetail(Project project, int crashId)
    {
        Crash? crash = await _crashes.GetByIdAsync(crashId);

        if (crash is null || crash.ProjectId != project.Id)
            return RedirectToAction(nameof(Crashes), new { id = project.Id });

        GameUser? gameUser = crash.GameUserId is null ? null : await _gameUsers.GetByIdAsync(crash.GameUserId.Value);

        return View("CrashDetail", new CrashDetailViewModel
        {
            Id = crash.Id,
            ProjectId = project.Id,
            Title = crash.Title,
            Status = crash.Status,
            Version = crash.Version,
            Platform = crash.Platform,
            SystemId = gameUser?.SystemId,
            GameUserId = gameUser?.Id,
            OccurredAt = crash.OccurredAt,
            Callstack = crash.Callstack
        });
    }

    private static CrashListItemViewModel ToListItem(Crash crash, IReadOnlyDictionary<int, string> systemIds) => new()
    {
        Id = crash.Id,
        Title = crash.Title,
        Status = crash.Status,
        Version = crash.Version,
        Platform = crash.Platform,
        SystemId = crash.GameUserId is not null && systemIds.TryGetValue(crash.GameUserId.Value, out string? systemId)
            ? systemId
            : null,
        GameUserId = crash.GameUserId,
        OccurredAt = crash.OccurredAt
    };
}
