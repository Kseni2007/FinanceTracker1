using System.IO;
using System.Text;

namespace FinanceTracker.Services;

/// <summary>Журнал событий приложения: дозапись строк в файл Logs/finance.log.</summary>
public static class Journal
{
    private static readonly string LogFolder =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

    public static void Note(string message)
    {
        try
        {
            Directory.CreateDirectory(LogFolder);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{message}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(LogFolder, "finance.log"), line, Encoding.UTF8);
        }
        catch
        {
            // журнал не должен мешать работе приложения
        }
    }
}
