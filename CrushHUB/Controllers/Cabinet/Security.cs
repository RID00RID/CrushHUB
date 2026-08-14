using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class CabinetController
{
    private const string PasswordChangedKey = "PasswordChanged";

    [HttpGet]
    public async Task<IActionResult> Security()
    {
        if (await LoadShell(SecurityTab) is null)
            return RedirectToAction("Login", "Account");

        return View(new ChangePasswordViewModel { Saved = TempData[PasswordChangedKey] is true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel password)
    {
        AppUser? user = await LoadShell(SecurityTab);

        if (user is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(nameof(Security), password);

        IdentityResult result = await _users.ChangePasswordAsync(user, password.CurrentPassword!, password.NewPassword!);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                // Неверный текущий пароль показываем у его же поля, остальное — у нового.
                string field = error.Code == nameof(IdentityErrorDescriber.PasswordMismatch)
                    ? nameof(ChangePasswordViewModel.CurrentPassword)
                    : nameof(ChangePasswordViewModel.NewPassword);

                ModelState.AddModelError(field, Translate(error));
            }

            return View(nameof(Security), password);
        }

        // Смена пароля обновляет security stamp — без этого текущая кука станет недействительной.
        await _signIn.RefreshSignInAsync(user);

        TempData[PasswordChangedKey] = true;

        return RedirectToAction(nameof(Security));
    }

    private static string Translate(IdentityError error) => error.Code switch
    {
        nameof(IdentityErrorDescriber.PasswordMismatch) => "Текущий пароль неверный",
        nameof(IdentityErrorDescriber.PasswordTooShort) => "Пароль слишком короткий",
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) => "В пароле нужна хотя бы одна цифра",
        nameof(IdentityErrorDescriber.PasswordRequiresLower) => "В пароле нужна хотя бы одна строчная буква",
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) => "В пароле нужна хотя бы одна заглавная буква",
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => "В пароле нужен спецсимвол",
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => "В пароле слишком мало разных символов",
        _ => error.Description
    };
}
