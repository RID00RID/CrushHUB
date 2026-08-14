using System.Globalization;
using System.Text.Json;
using CrushHUB.Domain.Entities;
using CrushHUB.Domain.Repositoryes.Abstract;
using CrushHUB.Models;

namespace CrushHUB.Services;

/// <summary>
/// Находит или заводит машину игрока по SystemID и обновляет её конфигурацию.
/// Конфигурация хранится один раз на машину, а не копией в каждом краше.
/// </summary>
public class GameUserRegistry
{
    private readonly IRepository<GameUser> _users;

    public GameUserRegistry(IRepository<GameUser> users)
    {
        _users = users;
    }

    public async Task<GameUser?> ResolveAsync(int projectId, string? fallbackSystemId, JsonElement? config)
    {
        string? configJson = config is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined }
            ? config.Value.GetRawText()
            : null;

        UserConfigViewModel? parsed = UserConfigViewModel.Parse(configJson);

        string? systemId = Clean(parsed?.SystemId) ?? Clean(fallbackSystemId);

        if (systemId is null)
            return null;

        GameUser? user = (await _users.FindAsync(u => u.ProjectId == projectId && u.SystemId == systemId))
            .FirstOrDefault();

        if (user is null)
        {
            user = new GameUser { ProjectId = projectId, SystemId = systemId };
            Apply(user, parsed, configJson);

            await _users.AddAsync(user);
        }
        else
        {
            // Конфигурацию перетираем только если её прислали — иначе просто отмечаем визит.
            if (configJson is not null)
                Apply(user, parsed, configJson);

            user.LastSeenAt = DateTime.UtcNow;

            _users.Update(user);
        }

        await _users.SaveChangesAsync();

        return user;
    }

    private static void Apply(GameUser user, UserConfigViewModel? config, string? configJson)
    {
        user.LastSeenAt = DateTime.UtcNow;

        if (config is null)
            return;

        user.OsName = config.Os?.Name;
        user.OsVersion = config.Os?.Version;
        user.Cpu = config.Cpu;
        user.Gpu = config.Gpu;
        user.MemoryMb = int.TryParse(config.Memory, NumberStyles.Integer, CultureInfo.InvariantCulture, out int mb)
            ? mb
            : null;
        user.ConfigJson = configJson;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
