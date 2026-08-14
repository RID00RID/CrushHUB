using System.ComponentModel.DataAnnotations;

namespace CrushHUB.Models;

/// <summary>Карточка текущего пользователя: аватар в шапке и блок в сайдбаре админки.</summary>
public class AccountCardViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }

    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[..1].ToUpper();
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100, ErrorMessage = "Имя не длиннее 100 символов")]
    [Display(Name = "Имя")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Укажите почту")]
    [EmailAddress(ErrorMessage = "Похоже, это не почта")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(1000, ErrorMessage = "Не длиннее 1000 символов")]
    [Display(Name = "О себе")]
    public string? Bio { get; set; }

    public bool Saved { get; init; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введите текущий пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string? CurrentPassword { get; set; }

    [Required(ErrorMessage = "Введите новый пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль не короче 6 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Повторите новый пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Повторите пароль")]
    public string? ConfirmPassword { get; set; }

    public bool Saved { get; init; }
}

public class UsersViewModel
{
    public IReadOnlyList<MemberViewModel> Members { get; init; } = [];

    public CreateUserViewModel Create { get; init; } = new();

    /// <summary>Развёрнута ли карточка «Новый пользователь».</summary>
    public bool IsCreating { get; init; }

    public string? Error { get; init; }
}

public class MemberViewModel
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>Логин для входа — он же может отличаться от почты.</summary>
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Role { get; init; }

    /// <summary>Себя нельзя ни удалить, ни разжаловать — иначе можно закрыть себе доступ.</summary>
    public bool IsCurrentUser { get; init; }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100, ErrorMessage = "Имя не длиннее 100 символов")]
    [Display(Name = "Имя")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Укажите логин")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин от 3 до 50 символов")]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Логин: латиница, цифры, точка, дефис и подчёркивание")]
    [Display(Name = "Логин")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Укажите почту")]
    [EmailAddress(ErrorMessage = "Похоже, это не почта")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Задайте пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль не короче 6 символов")]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Required]
    [Display(Name = "Роль")]
    public string Role { get; set; } = Services.RoleNames.Member;
}
