using System.ComponentModel.DataAnnotations;

namespace CrushHUB.Models;

public class ProjectsViewModel
{
    public IReadOnlyList<ProjectCardViewModel> Projects { get; init; } = [];

    public CreateProjectViewModel Create { get; init; } = new();

    /// <summary>Развёрнута ли форма создания проекта.</summary>
    public bool IsCreating { get; init; }

    /// <summary>Ключ только что созданного проекта — показывается один раз после создания.</summary>
    public string? GeneratedKey { get; init; }
}

public class ProjectCardViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Platform { get; init; } = string.Empty;

    public int CrashCount { get; init; }

    public int OpenReportCount { get; init; }
}

public class CreateProjectViewModel
{
    [Required(ErrorMessage = "Укажите название проекта")]
    [StringLength(100, ErrorMessage = "Название не длиннее 100 символов")]
    [Display(Name = "Название проекта")]
    public string? Name { get; set; }
}
