using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

/// <summary>
/// Общая часть раздела администрирования: каркас с сайдбаром. Сами вкладки
/// вынесены в отдельные части этого же контроллера (Profile.cs, Users.cs).
/// Раздел целиком доступен только администраторам.
/// </summary>
[Authorize(Roles = RoleNames.Admin)]
public partial class AdminController : Controller
{
    public const string ProfileTab = "profile";
    public const string UsersTab = "users";

    private readonly UserManager<AppUser> _users;

    public AdminController(UserManager<AppUser> users)
    {
        _users = users;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Profile));

    /// <summary>Готовит каркас: активную вкладку и карточку аккаунта в сайдбаре.</summary>
    private async Task<AppUser?> LoadShell(string tab)
    {
        AppUser? user = await _users.GetUserAsync(User);

        if (user is null)
            return null;

        ViewData["AdminTab"] = tab;
        ViewData["Account"] = new AccountCardViewModel
        {
            Name = DisplayNameOf(user),
            Email = user.Email ?? string.Empty,
            IsAdmin = true
        };

        return user;
    }

    private static string DisplayNameOf(AppUser user) =>
        string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName;
}
