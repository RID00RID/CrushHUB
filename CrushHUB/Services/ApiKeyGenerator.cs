using System.Security.Cryptography;

namespace CrushHUB.Services;

/// <summary>Генератор ключей приложения вида <c>ch_live_…</c>.</summary>
public static class ApiKeyGenerator
{
    public const string Prefix = "ch_live_";

    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int BodyLength = 32;

    public static string Create()
    {
        Span<char> body = stackalloc char[BodyLength];

        for (int i = 0; i < BodyLength; i++)
            body[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return Prefix + new string(body);
    }
}
