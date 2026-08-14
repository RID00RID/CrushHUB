using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class HomeController
{
    private const string NotificationsSavedKey = "NotificationsSaved";
    private const string TestResultKey = "DiscordTestResult";

    [HttpGet]
    public async Task<IActionResult> Settings(int id)
    {
        Project? project = await LoadProjectPage(id, AppTab.Settings);

        return project is null ? RedirectToAction(nameof(Index)) : View(BuildSettingsViewModel(project));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameProject(int id, ProjectSettingsViewModel settings)
    {
        Project? project = await LoadProjectPage(id, AppTab.Settings);

        if (project is null)
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
        {
            ProjectSettingsViewModel invalid = BuildSettingsViewModel(project);
            invalid.Name = settings.Name;

            return View(nameof(Settings), invalid);
        }

        project.Name = settings.Name!.Trim();

        _projects.Update(project);
        await _projects.SaveChangesAsync();

        return RedirectToAction(nameof(Settings), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateKey(int id)
    {
        Project? project = await _projects.GetByIdAsync(id);

        if (project is null)
            return RedirectToAction(nameof(Index));

        project.ApiKey = ApiKeyGenerator.Create();

        _projects.Update(project);
        await _projects.SaveChangesAsync();

        return RedirectToAction(nameof(Settings), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        Project? project = await _projects.GetByIdAsync(id);

        if (project is null)
            return RedirectToAction(nameof(Index));

        _projects.Delete(project);
        await _projects.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> SaveNotifications(int id, ProjectSettingsViewModel settings)
    {
        Project? project = await LoadProjectPage(id, AppTab.Settings);

        if (project is null)
            return RedirectToAction(nameof(Index));

        // Из этой формы приходят только поля уведомлений, остальное не трогаем.
        ModelState.Remove(nameof(ProjectSettingsViewModel.Name));

        if (!ModelState.IsValid)
        {
            ProjectSettingsViewModel invalid = BuildSettingsViewModel(project);
            invalid.DiscordWebhookUrl = settings.DiscordWebhookUrl;
            invalid.NotifyOnCrash = settings.NotifyOnCrash;
            invalid.NotifyOnReport = settings.NotifyOnReport;

            return View(nameof(Settings), invalid);
        }

        project.DiscordWebhookUrl = string.IsNullOrWhiteSpace(settings.DiscordWebhookUrl)
            ? null
            : settings.DiscordWebhookUrl.Trim();
        project.NotifyOnCrash = settings.NotifyOnCrash;
        project.NotifyOnReport = settings.NotifyOnReport;

        _projects.Update(project);
        await _projects.SaveChangesAsync();

        TempData[NotificationsSavedKey] = true;

        return RedirectToAction(nameof(Settings), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> TestDiscord(int id)
    {
        Project? project = await _projects.GetByIdAsync(id);

        if (project is null)
            return RedirectToAction(nameof(Index));

        TempData[TestResultKey] = string.IsNullOrWhiteSpace(project.DiscordWebhookUrl)
            ? "Сначала сохраните вебхук"
            : await _discord.SendTestAsync(project.DiscordWebhookUrl, project.Name) is { } error
                ? $"Не отправилось: {error}"
                : "Отправлено — проверьте канал";

        return RedirectToAction(nameof(Settings), new { id });
    }

    private ProjectSettingsViewModel BuildSettingsViewModel(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        ApiKey = project.ApiKey,
        ServerUrl = $"{Request.Scheme}://{Request.Host}/ingest/{project.Id}",
        DiscordWebhookUrl = project.DiscordWebhookUrl,
        NotifyOnCrash = project.NotifyOnCrash,
        NotifyOnReport = project.NotifyOnReport,
        TestResult = TempData[TestResultKey] as string,
        NotificationsSaved = TempData[NotificationsSavedKey] is true
    };
}
