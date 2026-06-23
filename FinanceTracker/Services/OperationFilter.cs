using FinanceTracker.Model;

namespace FinanceTracker.Services;

/// <summary>
/// Паттерн «Стратегия»: каждое правило отбора операций оформлено отдельным
/// классом, а <see cref="FilterChain"/> последовательно применяет выбранные правила.
/// </summary>
public interface IOperationFilter
{
    IEnumerable<Operation> Pass(IEnumerable<Operation> source);
}

/// <summary>Отбор по диапазону дат.</summary>
public sealed class PeriodFilter : IOperationFilter
{
    private readonly DateTime _from, _to;
    public PeriodFilter(DateTime from, DateTime to) { _from = from.Date; _to = to.Date; }
    public IEnumerable<Operation> Pass(IEnumerable<Operation> source) =>
        source.Where(o => o.Moment.Date >= _from && o.Moment.Date <= _to);
}

/// <summary>Отбор по типу операции (доход/расход).</summary>
public sealed class KindFilter : IOperationFilter
{
    private readonly OperationKind _kind;
    public KindFilter(OperationKind kind) => _kind = kind;
    public IEnumerable<Operation> Pass(IEnumerable<Operation> source) =>
        source.Where(o => o.Kind == _kind);
}

/// <summary>Отбор по категории.</summary>
public sealed class CategoryFilter : IOperationFilter
{
    private readonly int _categoryKey;
    public CategoryFilter(int categoryKey) => _categoryKey = categoryKey;
    public IEnumerable<Operation> Pass(IEnumerable<Operation> source) =>
        source.Where(o => o.CategoryKey == _categoryKey);
}

/// <summary>Отбор по подстроке в примечании или названии категории.</summary>
public sealed class TextFilter : IOperationFilter
{
    private readonly string _text;
    public TextFilter(string text) => _text = text.Trim();
    public IEnumerable<Operation> Pass(IEnumerable<Operation> source)
    {
        if (string.IsNullOrEmpty(_text)) return source;
        return source.Where(o =>
            o.Note.Contains(_text, StringComparison.OrdinalIgnoreCase) ||
            o.CategoryTitle.Contains(_text, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>Конвейер отбора: накапливает правила и применяет их по очереди.</summary>
public sealed class FilterChain
{
    private readonly List<IOperationFilter> _filters = new();

    public FilterChain With(IOperationFilter? filter)
    {
        if (filter is not null) _filters.Add(filter);
        return this;
    }

    public IReadOnlyList<Operation> Run(IEnumerable<Operation> source)
    {
        var current = source;
        foreach (var f in _filters)
            current = f.Pass(current);
        return current.OrderByDescending(o => o.Moment).ToList();
    }
}
