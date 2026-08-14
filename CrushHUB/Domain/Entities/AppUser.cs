using Microsoft.AspNetCore.Identity;

namespace CrushHUB.Domain.Entities;

/// <summary>Пользователь системы: Identity плюс поля профиля.</summary>
public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public string? Bio { get; set; }
}
