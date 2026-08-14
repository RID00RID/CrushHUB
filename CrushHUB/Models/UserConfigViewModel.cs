using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrushHUB.Models;

/// <summary>
/// Конфигурация машины игрока. Игра присылает JSON — храним его как есть и разбираем
/// по известным полям только для показа, чтобы незнакомые поля не терялись.
/// </summary>
public class UserConfigViewModel
{
    private static readonly JsonSerializerOptions ParseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    /// <summary>Уникальный идентификатор машины игрока.</summary>
    public string? SystemId { get; set; }

    public OsInfo? Os { get; set; }

    public string? Cpu { get; set; }

    public string? Gpu { get; set; }

    /// <summary>Объём оперативной памяти в мегабайтах, приходит строкой.</summary>
    public string? Memory { get; set; }

    /// <summary>Исходный JSON — показываем под спойлером, чтобы ничего не потерять.</summary>
    [JsonIgnore]
    public string Raw { get; set; } = string.Empty;

    /// <summary>Разбирает как «голый» объект конфигурации, так и обёртку {"UserConfig": {...}}.</summary>
    public static UserConfigViewModel? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        UserConfigViewModel? parsed;

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement.ValueKind == JsonValueKind.Object
                               && document.RootElement.TryGetProperty("UserConfig", out JsonElement wrapped)
                ? wrapped
                : document.RootElement;

            parsed = root.Deserialize<UserConfigViewModel>(ParseOptions);
        }
        catch (JsonException)
        {
            // Прислали не тот формат — покажем хотя бы исходный текст.
            return new UserConfigViewModel { Raw = json };
        }

        parsed ??= new UserConfigViewModel();
        parsed.Raw = Prettify(json);

        return parsed;
    }

    public string? MemoryDisplay
    {
        get
        {
            if (!int.TryParse(Memory, NumberStyles.Integer, CultureInfo.InvariantCulture, out int megabytes) || megabytes <= 0)
                return Memory;

            return $"{Math.Round(megabytes / 1024.0)} ГБ";
        }
    }

    public bool HasAnything =>
        SystemId is not null || Os is not null || Cpu is not null || Gpu is not null || Memory is not null;

    // Кириллицу не экранируем: этот JSON читает человек, а в HTML его всё равно экранирует Razor.
    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string Prettify(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, PrettyOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}

public class OsInfo
{
    public string? Name { get; set; }

    public string? Version { get; set; }
}
