namespace CrushHUB.Services;

public class AppConfig
{
    public Database Database { get; set; } = new Database();

    public Notifications Notifications { get; set; } = new Notifications();
}

public class Notifications
{
    /// <summary>
    /// Прокси для исходящих уведомлений: http://host:port или socks5://host:port.
    /// Нужен там, где мессенджер недоступен напрямую — например, Discord с российского хостинга.
    /// Пусто — ходим напрямую.
    /// </summary>
    public string? ProxyUrl { get; set; }
}

public class Database
{
    public string? ConnectionString { get; set; }
}
