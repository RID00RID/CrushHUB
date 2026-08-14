using CrushHUB.Domain.Entities;
using CrushHUB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace CrushHUB.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
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

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Username!, model.Password!, model.RememberMe, false);
        
        if(result.Succeeded)
            return Redirect(returnUrl ?? "/");
        
        ModelState.AddModelError(string.Empty, "неверный логин или пароль");
        return View(model);
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