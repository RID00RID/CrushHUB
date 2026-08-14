namespace CrushHUB.Services;

/// <summary>Роли системы. Значения совпадают с именами ролей в Identity.</summary>
public static class RoleNames
{
    public const string Admin = "admin";
    public const string Member = "member";

    public static readonly IReadOnlyList<string> All = [Admin, Member];

    public static string Display(string? role) => role switch
    {
        Admin => "Администратор",
        Member => "Участник",
        _ => "Без роли"
    };
}
