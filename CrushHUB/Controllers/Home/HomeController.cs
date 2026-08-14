using CrushHUB.Domain.Entities;
using CrushHUB.Domain.Repositoryes.Abstract;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

/// <summary>
/// Общая часть: выбор и создание проектов. Страницы выбранного проекта вынесены
/// в отдельные части этого же контроллера (Dashboard.cs, Crashes.cs и далее).
/// </summary>
[Authorize]
public partial class HomeController : Controller
{
    private const string GeneratedKeyTempDataKey = "GeneratedKey";

    private readonly IRepository<Project> _projects;
    private readonly IRepository<Crash> _crashes;
    private readonly IRepository<UserReport> _reports;
    private readonly IRepository<GameUser> _gameUsers;

    public HomeController(IRepository<Project> projects, IRepository<Crash> crashes,
        IRepository<UserReport> reports, IRepository<GameUser> gameUsers)
    {
        _projects = projects;
        _crashes = crashes;
        _reports = reports;
        _gameUsers = gameUsers;
    }

    /// <summary>SystemID машин проекта по их ключу — чтобы не ходить в базу на каждую строку списка.</summary>
    private async Task<Dictionary<int, string>> LoadSystemIds(int projectId)
    {
        List<GameUser> users = await _gameUsers.FindAsync(u => u.ProjectId == projectId);

        return users.ToDictionary(u => u.Id, u => u.SystemId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool create = false)
    {
        string? generatedKey = TempData[GeneratedKeyTempDataKey] as string;

        return View(await BuildProjectsViewModel(create || generatedKey is not null, generatedKey));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateProject(CreateProjectViewModel create)
    {
        if (!ModelState.IsValid)
            return View(nameof(Index), await BuildProjectsViewModel(true, null, create));

        Project project = new()
        {
            Name = create.Name!.Trim(),
            ApiKey = ApiKeyGenerator.Create()
        };

        await _projects.AddAsync(project);
        await _projects.SaveChangesAsync();

        TempData[GeneratedKeyTempDataKey] = project.ApiKey;

        return RedirectToAction(nameof(Index));
    }

    private async Task<ProjectsViewModel> BuildProjectsViewModel(bool isCreating, string? generatedKey,
        CreateProjectViewModel? create = null)
    {
        List<Project> projects = await _projects.GetAllAsync();

        return new ProjectsViewModel
        {
            Projects = projects
                .OrderBy(p => p.Id)
                .Select(p => new ProjectCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Platform = p.Platform
                })
                .ToList(),
            Create = create ?? new CreateProjectViewModel(),
            IsCreating = isCreating && User.IsInRole(RoleNames.Admin),
            GeneratedKey = generatedKey
        };
    }

    /// <summary>Страница выбранного проекта без своей модели: каркас и пустое содержимое.</summary>
    private async Task<IActionResult> ProjectPage(int id, AppTab tab)
    {
        Project? project = await LoadProjectPage(id, tab);

        return project is null ? RedirectToAction(nameof(Index)) : View(tab.Action);
    }

    /// <summary>Готовит каркас страницы проекта: имя в шапке и активную вкладку.</summary>
    private async Task<Project?> LoadProjectPage(int id, AppTab tab)
    {
        Project? project = await _projects.GetByIdAsync(id);

        if (project is null)
            return null;

        ViewData["ProjectId"] = project.Id;
        ViewData["ProjectName"] = project.Name;
        ViewData["ActiveTab"] = tab.Key;
        ViewData["Title"] = tab.Label;

        return project;
    }
}
