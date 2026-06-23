using System.Globalization;
using System.Text;
using FinanceTracker.Model;

namespace FinanceTracker.Services;

public enum StatementFormat { Csv, Html, Markdown }

/// <summary>
/// Паттерн «Фабричный метод»: интерфейс <see cref="IStatementWriter"/> задаёт единый
/// способ построения выписки операций, а <see cref="StatementFactory"/> по выбранному
/// формату создаёт нужный построитель.
/// </summary>
public interface IStatementWriter
{
    string Extension { get; }
    string Build(IEnumerable<Operation> operations);
}

public sealed class CsvStatement : IStatementWriter
{
    public string Extension => ".csv";
    public string Build(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Дата;Тип;Категория;Сумма;Способ;Примечание");
        foreach (var o in operations)
            sb.AppendLine(string.Join(';',
                o.Moment.ToString("dd.MM.yyyy"),
                o.Kind == OperationKind.Income ? "Доход" : "Расход",
                o.CategoryTitle,
                o.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                o.Way,
                o.Note.Replace(';', ' ')));
        return sb.ToString();
    }
}

public sealed class HtmlStatement : IStatementWriter
{
    public string Extension => ".html";
    public string Build(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'><title>Выписка операций</title></head><body>");
        sb.AppendLine("<h2>Выписка операций</h2><table border='1' cellspacing='0' cellpadding='4'>");
        sb.AppendLine("<tr><th>Дата</th><th>Тип</th><th>Категория</th><th>Сумма</th><th>Примечание</th></tr>");
        foreach (var o in operations)
            sb.AppendLine($"<tr><td>{o.Moment:dd.MM.yyyy}</td>" +
                          $"<td>{(o.Kind == OperationKind.Income ? "Доход" : "Расход")}</td>" +
                          $"<td>{o.CategoryTitle}</td><td align='right'>{o.Amount:0.00}</td><td>{o.Note}</td></tr>");
        sb.AppendLine("</table></body></html>");
        return sb.ToString();
    }
}

public sealed class MarkdownStatement : IStatementWriter
{
    public string Extension => ".md";
    public string Build(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Выписка операций\n");
        sb.AppendLine("| Дата | Тип | Категория | Сумма | Примечание |");
        sb.AppendLine("|------|-----|-----------|------:|------------|");
        foreach (var o in operations)
            sb.AppendLine($"| {o.Moment:dd.MM.yyyy} | {(o.Kind == OperationKind.Income ? "Доход" : "Расход")} " +
                          $"| {o.CategoryTitle} | {o.Amount:0.00} | {o.Note} |");
        return sb.ToString();
    }
}

public static class StatementFactory
{
    public static IStatementWriter Create(StatementFormat format) => format switch
    {
        StatementFormat.Csv => new CsvStatement(),
        StatementFormat.Html => new HtmlStatement(),
        StatementFormat.Markdown => new MarkdownStatement(),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
