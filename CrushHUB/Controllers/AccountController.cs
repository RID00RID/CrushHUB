using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace CrushHUB.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl)
    {
        await _signInManager.SignOutAsync();
        
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        
        if(!ModelState.IsValid)
            return View(model);

        string login = await ResolveUserName(model.Username!.Trim());

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(login, model.Password!, model.RememberMe, false);

        if(result.Succeeded)
            return Redirect(returnUrl ?? "/");

        ModelState.AddModelError(string.Empty, "неверный логин или пароль");
        return View(model);
    }
    
    /// <summary>
    /// Участников заводят по почте, а логином становится её часть до «собаки» —
    /// поэтому вход принимаем и по логину, и по почте.
    /// </summary>
    private async Task<string> ResolveUserName(string login)
    {
        if (!login.Contains('@'))
            return login;

        AppUser? byEmail = await _userManager.FindByEmailAsync(login);

        return byEmail?.UserName ?? login;
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}