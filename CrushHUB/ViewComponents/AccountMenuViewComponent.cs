using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.ViewComponents;

/// <summary>Аватар с выпадающим меню в шапке. У участника вместо него простое «Выйти».</summary>
public class AccountMenuViewComponent : ViewComponent
{
    private readonly UserManager<AppUser> _users;

    public AccountMenuViewComponent(UserManager<AppUser> users)
    {
        _users = users;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        AppUser? user = await _users.GetUserAsync(HttpContext.User);

        return View(new AccountCardViewModel
        {
            Name = string.IsNullOrWhiteSpace(user?.DisplayName) ? user?.UserName ?? string.Empty : user.DisplayName,
            Email = user?.Email ?? string.Empty,
            IsAdmin = HttpContext.User.IsInRole(RoleNames.Admin)
        });
    }
}
