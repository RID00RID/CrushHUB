using System.Text.Json;
using CrushHUB.Domain.Entities;
using CrushHUB.Domain.Repositoryes.Abstract;
using CrushHUB.Models.Api;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

/// <summary>
/// Приём данных от игры. Аутентификация — ключ проекта в заголовке <c>X-Api-Key</c>,
/// сессия админки здесь не участвует.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("ingest/{projectId:int}")]
public class IngestController : ControllerBase
{
    public const string ApiKeyHeader = "X-Api-Key";

    private readonly IRepository<Project> _projects;
    private readonly IRepository<Crash> _crashes;
    private readonly IRepository<UserReport> _reports;
    private readonly ScreenshotStorage _screenshots;
    private readonly GameUserRegistry _gameUsers;

    public IngestController(IRepository<Project> projects, IRepository<Crash> crashes,
        IRepository<UserReport> reports, ScreenshotStorage screenshots, GameUserRegistry gameUsers)
    {
        _projects = projects;
        _crashes = crashes;
        _reports = reports;
        _screenshots = screenshots;
        _gameUsers = gameUsers;
    }

    /// <summary>Проверка ключа: удобно дёрнуть первым, чтобы убедиться в настройках.</summary>
    [HttpGet("ping")]
    public async Task<IActionResult> Ping(int projectId)
    {
        Project? project = await Authorize(projectId);

        if (project is null)
            return Unauthorized(new { error = "Неизвестный ключ приложения или чужой проект" });

        return Ok(new { projectId = project.Id, project = project.Name });
    }

    /// <summary>
    /// Игрок согласился делиться данными о системе — игра присылает конфигурацию.
    /// Повторный вызов с тем же SystemID обновляет её, дубликатов не будет.
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> PostUser(int projectId, [FromBody] JsonElement body)
    {
        Project? project = await Authorize(projectId);

        if (project is null)
            return Unauthorized(new { error = "Неизвестный ключ приложения или чужой проект" });

        GameUser? user = await _gameUsers.ResolveAsync(project.Id, null, body);

        if (user is null)
            return BadRequest(new { error = "В конфигурации нет SystemID" });

        return Ok(new { id = user.Id, systemId = user.SystemId });
    }

    [HttpPost("crashes")]
    public async Task<IActionResult> PostCrash(int projectId, [FromBody] CrashIngestRequest request)
    {
        Project? project = await Authorize(projectId);

        if (project is null)
            return Unauthorized(new { error = "Неизвестный ключ приложения или чужой проект" });

        GameUser? gameUser = await _gameUsers.ResolveAsync(project.Id, request.UserId, request.UserConfig);

        Crash crash = new()
        {
            ProjectId = project.Id,
            GameUserId = gameUser?.Id,
            Title = request.Title.Trim(),
            Callstack = request.Callstack,
            Version = request.Version,
            Platform = request.Platform,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            Status = CrashStatus.Open
        };

        await _crashes.AddAsync(crash);
        await _crashes.SaveChangesAsync();

        return Created($"/Home/Crashes/{project.Id}?crash={crash.Id}", new { id = crash.Id });
    }

    [HttpPost("reports")]
    public async Task<IActionResult> PostReport(int projectId, [FromBody] ReportIngestRequest request)
    {
        Project? project = await Authorize(projectId);

        if (project is null)
            return Unauthorized(new { error = "Неизвестный ключ приложения или чужой проект" });

        string? screenshotPath = null;

        if (!string.IsNullOrWhiteSpace(request.Screenshot))
        {
            ScreenshotSaveResult saved = await _screenshots.SaveAsync(project.Id, request.Screenshot);

            if (saved.Error is not null)
                return BadRequest(new { error = saved.Error });

            screenshotPath = saved.Path;
        }

        GameUser? gameUser = await _gameUsers.ResolveAsync(project.Id, request.UserId, request.UserConfig);

        UserReport report = new()
        {
            ProjectId = project.Id,
            GameUserId = gameUser?.Id,
            Category = request.Category.Trim(),
            Description = request.Description.Trim(),
            ScreenshotPath = screenshotPath,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
            Status = ReportStatus.Open
        };

        await _reports.AddAsync(report);
        await _reports.SaveChangesAsync();

        return Created($"/Home/Reports/{project.Id}?report={report.Id}", new { id = report.Id, screenshot = screenshotPath });
    }

    /// <summary>Ключ из заголовка должен принадлежать именно тому проекту, что указан в адресе.</summary>
    private async Task<Project?> Authorize(int projectId)
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var header))
            return null;

        string? key = header.ToString().Trim();

        if (string.IsNullOrEmpty(key))
            return null;

        List<Project> found = await _projects.FindAsync(p => p.ApiKey == key);

        return found.FirstOrDefault(p => p.Id == projectId);
    }
}
