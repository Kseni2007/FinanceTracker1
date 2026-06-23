using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FinanceTracker.Store;

/// <summary>
/// Низкоуровневое файловое хранилище: сериализация коллекций в формат JSON
/// средствами System.Text.Json. Файлы располагаются в подпапке Data.
/// </summary>
public static class JsonStore
{
    private static readonly string Folder =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string PathOf(string name) => Path.Combine(Folder, name + ".json");

    public static List<T> Load<T>(string name)
    {
        var path = PathOf(name);
        if (!File.Exists(path))
            return new List<T>();
        var json = File.ReadAllText(path, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
    }

    public static void Save<T>(string name, IEnumerable<T> items)
    {
        Directory.CreateDirectory(Folder);
        var json = JsonSerializer.Serialize(items.ToList(), Options);
        File.WriteAllText(PathOf(name), json, Encoding.UTF8);
    }
}
