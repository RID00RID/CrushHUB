using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class HomeController
{
    private const int DashboardDays = 7;

    [HttpGet]
    public async Task<IActionResult> Dashboard(int id)
    {
        Project? project = await LoadProjectPage(id, AppTab.Dashboard);

        if (project is null)
            return RedirectToAction(nameof(Index));

        List<Crash> crashes = await _crashes.FindAsync(c => c.ProjectId == project.Id);
        List<UserReport> reports = await _reports.FindAsync(r => r.ProjectId == project.Id);

        DateTime today = DateTime.UtcNow.Date;
        List<DateTime> days = Enumerable.Range(0, DashboardDays)
            .Select(offset => today.AddDays(offset - DashboardDays + 1))
            .ToList();

        return View(new DashboardViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            CrashCount = crashes.Count,
            OpenReportCount = reports.Count(r => r.Status == ReportStatus.Open),
            Crashes = ChartViewModel.Build("Краши по времени (7 дней)", "danger", days,
                CountByDay(crashes.Select(c => c.OccurredAt))),
            Reports = ChartViewModel.Build("Обращения по времени (7 дней)", "warning", days,
                CountByDay(reports.Select(r => r.CreatedAt)))
        });
    }

    private static Dictionary<DateTime, int> CountByDay(IEnumerable<DateTime> moments) =>
        moments.GroupBy(m => m.Date).ToDictionary(group => group.Key, group => group.Count());
}
