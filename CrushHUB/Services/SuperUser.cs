using CrushHUB.Domain.Entities;

namespace CrushHUB.Services;

/// <summary>
/// Базовый администратор из сида. Его учётную запись правит только он сам,
/// роль не меняется ни им, ни кем-либо ещё, удалить его нельзя — иначе систему
/// можно оставить без единственного гарантированного администратора.
/// </summary>
public static class SuperUser
{
    public const string Id = "4B00D67B-169D-459D-8BE0-5A1F9575F247";

    public static bool Is(AppUser user) => Is(user.Id);

    public static bool Is(string? userId) => string.Equals(userId, Id, StringComparison.OrdinalIgnoreCase);
}
