namespace CrushHUB.Models;

/// <summary>Вкладка верхней панели. Порядок в <see cref="All"/> совпадает с макетом.</summary>
public record AppTab(string Key, string Label, string Action)
{
    public static readonly AppTab Dashboard = new("dashboard", "Dashboard", "Dashboard");
    public static readonly AppTab Crashes = new("crashList", "Crash Report", "Crashes");
    public static readonly AppTab Reports = new("reportList", "User Report", "Reports");
    public static readonly AppTab Users = new("userList", "Users", "Users");
    public static readonly AppTab Settings = new("settings", "Настройки", "Settings");

    public static readonly IReadOnlyList<AppTab> All = [Dashboard, Crashes, Reports, Users, Settings];
}
