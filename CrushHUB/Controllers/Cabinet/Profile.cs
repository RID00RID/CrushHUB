using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrushHUB.Controllers;

public partial class CabinetController
{
    private const string ProfileSavedKey = "ProfileSaved";

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        AppUser? user = await LoadShell(ProfileTab);

        if (user is null)
            return RedirectToAction("Login", "Account");

        return View(new ProfileViewModel
        {
            Name = DisplayNameOf(user),
            Email = user.Email,
            Bio = user.Bio,
            Saved = TempData[ProfileSavedKey] is true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(ProfileViewModel profile)
    {
        AppUser? user = await LoadShell(ProfileTab);

        if (user is null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(nameof(Profile), profile);

        user.DisplayName = profile.Name!.Trim();
        user.Bio = string.IsNullOrWhiteSpace(profile.Bio) ? null : profile.Bio.Trim();

        IdentityResult email = await _users.SetEmailAsync(user, profile.Email!.Trim());

        if (!email.Succeeded)
        {
            AddErrors(email);
            return View(nameof(Profile), profile);
        }

        IdentityResult saved = await _users.UpdateAsync(user);

        if (!saved.Succeeded)
        {
            AddErrors(saved);
            return View(nameof(Profile), profile);
        }

        TempData[ProfileSavedKey] = true;

        return RedirectToAction(nameof(Profile));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
