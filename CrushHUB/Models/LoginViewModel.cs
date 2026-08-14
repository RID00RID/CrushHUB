using System.ComponentModel.DataAnnotations;

namespace CrushHUB.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите логин или почту")]
    [Display(Name = "Логин или почта")]
    public string? Username { get; set; }
    [Required]
    
    [UIHint("password")]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
    
    public bool RememberMe { get; set; }
}