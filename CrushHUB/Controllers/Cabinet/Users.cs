using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using CrushHUB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrushHUB.Controllers;

public partial class CabinetController
{
    private const string UsersErrorKey = "UsersError";

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Users(bool create = false, string? edit = null)
    {
        if (await LoadShell(UsersTab) is null)
            return RedirectToAction("Login", "Account");

        EditUserViewModel? editing = null;

        if (!string.IsNullOrEmpty(edit) && await _users.FindByIdAsync(edit) is { } target)
            editing = ToEditModel(target);

        return View(await BuildUsersViewModel(create, edit: editing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> EditUser(EditUserViewModel edit)
    {
        if (await LoadShell(UsersTab) is null)
            return RedirectToAction("Login", "Account");

        AppUser? user = await _users.FindByIdAsync(edit.Id);

        if (user is null)
            return RedirectToAction(nameof(Users));

        if (!ModelState.IsValid)
            return View(nameof(Users), await BuildUsersViewModel(false, edit: Merge(user, edit)));

        user.DisplayName = edit.Name!.Trim();

        IdentityResult saved = await _users.UpdateAsync(user);

        if (!saved.Succeeded)
        {
            AddErrors(saved, nameof(EditUserViewModel.Name), edit);
            return View(nameof(Users), await BuildUsersViewModel(false, edit: Merge(user, edit)));
        }

        if (!string.IsNullOrEmpty(edit.NewPassword))
        {
            // Текущий пароль администратор не знает, поэтому меняем через токен сброса.
            string token = await _users.GeneratePasswordResetTokenAsync(user);
            IdentityResult reset = await _users.ResetPasswordAsync(user, token, edit.NewPassword);

            if (!reset.Succeeded)
            {
                AddErrors(reset, nameof(EditUserViewModel.NewPassword), edit);
                return View(nameof(Users), await BuildUsersViewModel(false, edit: Merge(user, edit)));
            }

            // Себе сменили пароль — обновляем куку, иначе вылетим из системы.
            if (IsCurrentUser(user))
                await _signIn.RefreshSignInAsync(user);
        }

        return RedirectToAction(nameof(Users));
    }

    private void AddErrors(IdentityResult result, string field, EditUserViewModel edit)
    {
        foreach (IdentityError error in result.Errors)
            ModelState.AddModelError($"Edit.{field}", error.Description);
    }

    private static EditUserViewModel ToEditModel(AppUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Name = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName
    };

    /// <summary>Возвращает введённое админом поверх опознавательных полей из базы.</summary>
    private static EditUserViewModel Merge(AppUser user, EditUserViewModel edit) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Name = edit.Name,
        NewPassword = edit.NewPassword
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateUser(CreateUserViewModel create)
    {
        if (await LoadShell(UsersTab) is null)
            return RedirectToAction("Login", "Account");

        if (!RoleNames.All.Contains(create.Role))
            ModelState.AddModelError("Create.Role", "Неизвестная роль");

        if (!ModelState.IsValid)
            return View(nameof(Users), await BuildUsersViewModel(true, create));

        string email = create.Email!.Trim();
        string userName = create.UserName!.Trim();

        if (await _users.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError("Create.Email", "Пользователь с такой почтой уже есть");
            return View(nameof(Users), await BuildUsersViewModel(true, create));
        }

        if (await _users.FindByNameAsync(userName) is not null)
        {
            ModelState.AddModelError("Create.UserName", "Такой логин уже занят");
            return View(nameof(Users), await BuildUsersViewModel(true, create));
        }

        AppUser user = new()
        {
            UserName = userName,
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
    [Authorize(Roles = RoleNames.Admin)]
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
    [Authorize(Roles = RoleNames.Admin)]
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

    private bool IsCurrentUser(AppUser user) => user.Id == _users.GetUserId(User);

    private async Task<UsersViewModel> BuildUsersViewModel(bool isCreating, CreateUserViewModel? create = null,
        EditUserViewModel? edit = null)
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
                UserName = user.UserName ?? string.Empty,
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
            Edit = edit,
            Error = TempData[UsersErrorKey] as string
        };
    }
}
