using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrushHUB.Controllers;

public partial class AdminController
{
    private const string UsersErrorKey = "UsersError";

    [HttpGet]
    public async Task<IActionResult> Users(bool create = false)
    {
        if (await LoadShell(UsersTab) is null)
            return RedirectToAction("Login", "Account");

        return View(await BuildUsersViewModel(create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel create)
    {
        if (await LoadShell(UsersTab) is null)
            return RedirectToAction("Login", "Account");

        if (!RoleNames.All.Contains(create.Role))
            ModelState.AddModelError("Create.Role", "Неизвестная роль");

        if (!ModelState.IsValid)
            return View(nameof(Users), await BuildUsersViewModel(true, create));

        string email = create.Email!.Trim();

        if (await _users.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError("Create.Email", "Пользователь с такой почтой уже есть");
            return View(nameof(Users), await BuildUsersViewModel(true, create));
        }

        AppUser user = new()
        {
            UserName = await PickUserName(email),
            DisplayName = create.Name!.Trim(),
            Email = email,
            EmailConfirmed = true
        };

        IdentityResult result = await _users.CreateAsync(user, create.Password!);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("Create.Password", error.Description);

            return View(nameof(Users), await BuildUsersViewModel(true, create));
        }

        await _users.AddToRoleAsync(user, create.Role);

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        if (!RoleNames.All.Contains(role))
            return RedirectToAction(nameof(Users));

        AppUser? user = await _users.FindByIdAsync(userId);

        if (user is null)
            return RedirectToAction(nameof(Users));

        if (IsCurrentUser(user))
        {
            TempData[UsersErrorKey] = "Нельзя менять собственную роль";
            return RedirectToAction(nameof(Users));
        }

        IList<string> current = await _users.GetRolesAsync(user);

        if (current.Count > 0)
            await _users.RemoveFromRolesAsync(user, current);

        await _users.AddToRoleAsync(user, role);

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        AppUser? user = await _users.FindByIdAsync(userId);

        if (user is null)
            return RedirectToAction(nameof(Users));

        if (IsCurrentUser(user))
        {
            TempData[UsersErrorKey] = "Нельзя удалить собственную учётную запись";
            return RedirectToAction(nameof(Users));
        }

        await _users.DeleteAsync(user);

        return RedirectToAction(nameof(Users));
    }

    /// <summary>Логин берём из локальной части почты, при совпадении — почта целиком.</summary>
    private async Task<string> PickUserName(string email)
    {
        string candidate = email.Split('@')[0];

        return await _users.FindByNameAsync(candidate) is null ? candidate : email;
    }

    private bool IsCurrentUser(AppUser user) => user.Id == _users.GetUserId(User);

    private async Task<UsersViewModel> BuildUsersViewModel(bool isCreating, CreateUserViewModel? create = null)
    {
        List<AppUser> users = await _users.Users.OrderBy(u => u.UserName).ToListAsync();
        List<MemberViewModel> members = [];

        foreach (AppUser user in users)
        {
            IList<string> roles = await _users.GetRolesAsync(user);

            members.Add(new MemberViewModel
            {
                Id = user.Id,
                Name = DisplayNameOf(user),
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault(),
                IsCurrentUser = IsCurrentUser(user)
            });
        }

        return new UsersViewModel
        {
            Members = members,
            Create = create ?? new CreateUserViewModel(),
            IsCreating = isCreating,
            Error = TempData[UsersErrorKey] as string
        };
    }
}
