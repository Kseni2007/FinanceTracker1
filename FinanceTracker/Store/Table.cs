using FinanceTracker.Model;

namespace FinanceTracker.Store;

/// <summary>
/// Обобщённая таблица записей одного типа. Хранит данные в памяти, сохраняет их
/// в JSON и оповещает подписчиков об изменениях через событие <see cref="Mutated"/>
/// (паттерн «Наблюдатель»).
/// </summary>
public sealed class Table<T> where T : class, IRecord
{
    private readonly string _name;
    private readonly List<T> _rows;

    /// <summary>Событие изменения таблицы (Observer).</summary>
    public event EventHandler? Mutated;

    public Table(string name)
    {
        _name = name;
        _rows = JsonStore.Load<T>(name);
    }

    public IReadOnlyList<T> Rows => _rows;

    public T? ByKey(int key) => _rows.FirstOrDefault(r => r.Key == key);

    private int NextKey() => _rows.Count == 0 ? 1 : _rows.Max(r => r.Key) + 1;

    public T Insert(T row)
    {
        row.Key = NextKey();
        _rows.Add(row);
        Commit();
        return row;
    }

    public void Apply(T row)
    {
        var i = _rows.FindIndex(r => r.Key == row.Key);
        if (i >= 0) _rows[i] = row;
        Commit();
    }

    public void Delete(int key)
    {
        _rows.RemoveAll(r => r.Key == key);
        Commit();
    }

    public void Fill(IEnumerable<T> rows)
    {
        _rows.Clear();
        _rows.AddRange(rows);
        Commit();
    }

    public void SeedWhenEmpty(IEnumerable<T> rows)
    {
        if (_rows.Count > 0) return;
        _rows.AddRange(rows);
        foreach (var r in _rows.Where(r => r.Key == 0))
            r.Key = NextKey();
        Commit();
    }

    private void Commit()
    {
        JsonStore.Save(_name, _rows);
        Mutated?.Invoke(this, EventArgs.Empty);
    }
}
