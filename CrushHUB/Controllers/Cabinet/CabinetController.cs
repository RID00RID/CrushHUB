using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

/// <summary>
/// Общая часть личного кабинета: каркас с сайдбаром. Вкладки вынесены в отдельные
/// части этого же контроллера (Profile.cs, Security.cs, Users.cs). Кабинет открыт
/// любому вошедшему; администраторская область — только «Пользователи».
/// </summary>
[Authorize]
public partial class CabinetController : Controller
{
    public const string ProfileTab = "profile";
    public const string SecurityTab = "security";
    public const string UsersTab = "users";

    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;

    public CabinetController(UserManager<AppUser> users, SignInManager<AppUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Profile));

    /// <summary>Готовит каркас: активную вкладку и карточку аккаунта в сайдбаре.</summary>
    private async Task<AppUser?> LoadShell(string tab)
    {
        AppUser? user = await _users.GetUserAsync(User);

        if (user is null)
            return null;

        ViewData["CabinetTab"] = tab;
        ViewData["Account"] = new AccountCardViewModel
        {
            Name = DisplayNameOf(user),
            Email = user.Email ?? string.Empty,
            IsAdmin = User.IsInRole(RoleNames.Admin)
        };

        return user;
    }

    private static string DisplayNameOf(AppUser user) =>
        string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName;
}
