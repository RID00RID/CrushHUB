using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class HomeController
{
    /// <summary>Порядок статусов при сортировке «По статусу»: сначала то, что требует внимания.</summary>
    private static readonly List<ReportStatus> StatusOrder = [.. ReportStatusView.All];

    [HttpGet]
    public async Task<IActionResult> Reports(int id, string? category, ReportStatus? status, ReportSort sort, int? report)
    {
        Project? project = await LoadProjectPage(id, AppTab.Reports);

        if (project is null)
            return RedirectToAction(nameof(Index));

        if (report is not null)
            return await ReportDetail(project, report.Value);

        List<UserReport> all = await _reports.FindAsync(r => r.ProjectId == project.Id);
        Dictionary<int, string> systemIds = await LoadSystemIds(project.Id);

        IEnumerable<UserReport> filtered = all;

        if (!string.IsNullOrEmpty(category))
            filtered = filtered.Where(r => r.Category == category);

        if (status is not null)
            filtered = filtered.Where(r => r.Status == status);

        filtered = sort == ReportSort.Status
            ? filtered.OrderBy(r => StatusOrder.IndexOf(r.Status)).ThenByDescending(r => r.CreatedAt)
            : filtered.OrderByDescending(r => r.CreatedAt);

        return View(new ReportsViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Category = category,
            Status = status,
            Sort = sort,
            Categories = all.Select(r => r.Category).Distinct().OrderBy(c => c).ToList(),
            Reports = filtered.Select(r => ToReportListItem(r, systemIds)).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetReportStatus(int id, int reportId, ReportStatus status)
    {
        UserReport? report = await _reports.GetByIdAsync(reportId);

        if (report is null || report.ProjectId != id)
            return RedirectToAction(nameof(Reports), new { id });

        report.Status = status;

        _reports.Update(report);
        await _reports.SaveChangesAsync();

        return RedirectToAction(nameof(Reports), new { id, report = reportId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteReport(int id, int reportId)
    {
        UserReport? report = await _reports.GetByIdAsync(reportId);

        if (report is not null && report.ProjectId == id)
        {
            _reports.Delete(report);
            await _reports.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Reports), new { id });
    }

    private async Task<IActionResult> ReportDetail(Project project, int reportId)
    {
        UserReport? report = await _reports.GetByIdAsync(reportId);

        if (report is null || report.ProjectId != project.Id)
            return RedirectToAction(nameof(Reports), new { id = project.Id });

        GameUser? gameUser = report.GameUserId is null ? null : await _gameUsers.GetByIdAsync(report.GameUserId.Value);

        return View("ReportDetail", new ReportDetailViewModel
        {
            Id = report.Id,
            ProjectId = project.Id,
            Category = report.Category,
            Description = report.Description,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            SystemId = gameUser?.SystemId,
            GameUserId = gameUser?.Id,
            ScreenshotPath = report.ScreenshotPath
        });
    }

    private static ReportListItemViewModel ToReportListItem(UserReport report, IReadOnlyDictionary<int, string> systemIds) => new()
    {
        Id = report.Id,
        Category = report.Category,
        Description = report.Description,
        Status = report.Status,
        SystemId = report.GameUserId is not null && systemIds.TryGetValue(report.GameUserId.Value, out string? systemId)
            ? systemId
            : null,
        GameUserId = report.GameUserId,
        CreatedAt = report.CreatedAt
    };
}
