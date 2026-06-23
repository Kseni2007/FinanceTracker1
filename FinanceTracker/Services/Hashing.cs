using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Services;

/// <summary>Хеширование PIN-кода алгоритмом SHA-256.</summary>
public static class Hashing
{
    public static string Pin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("ft::" + pin));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Matches(string pin, string hash) =>
        string.Equals(Pin(pin), hash, StringComparison.OrdinalIgnoreCase);
}
