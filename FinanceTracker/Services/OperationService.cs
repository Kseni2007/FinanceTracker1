using FinanceTracker.Model;
using FinanceTracker.Store;

namespace FinanceTracker.Services;

/// <summary>
/// Бизнес-логика работы с операциями и категориями. Изменяющие методы оборачиваются
/// в обратимые команды и проходят через <see cref="ActionStack"/> (отмена/повтор).
/// </summary>
public sealed class OperationService
{
    private readonly FinanceDb _db;
    private readonly ActionStack _stack;

    public OperationService(FinanceDb db, ActionStack stack)
    {
        _db = db;
        _stack = stack;
    }

    public IReadOnlyList<Operation> All => _db.Operations.Rows;
    public IReadOnlyList<Category> Categories => _db.Categories.Rows;

    public IReadOnlyList<Operation> Query(FilterChain chain) => chain.Run(_db.Operations.Rows);

    public void Add(Operation op)
    {
        var cat = _db.Categories.ByKey(op.CategoryKey);
        if (cat is not null) { op.CategoryTitle = cat.Title; op.Kind = cat.Kind; }
        Operation? created = null;
        _stack.Push(new LambdaAction("Добавление операции",
            () => created = _db.Operations.Insert(op),
            () => { if (created is not null) _db.Operations.Delete(created.Key); }));
    }

    public void Update(Operation op)
    {
        var before = _db.Operations.ByKey(op.Key)?.Copy();
        if (before is null) return;
        var cat = _db.Categories.ByKey(op.CategoryKey);
        if (cat is not null) { op.CategoryTitle = cat.Title; op.Kind = cat.Kind; }
        _stack.Push(new LambdaAction("Изменение операции",
            () => _db.Operations.Apply(op),
            () => _db.Operations.Apply(before)));
    }

    public void Remove(int key)
    {
        var before = _db.Operations.ByKey(key)?.Copy();
        if (before is null) return;
        _stack.Push(new LambdaAction("Удаление операции",
            () => _db.Operations.Delete(key),
            () => _db.Operations.Insert(before)));
    }

    public void AddCategory(Category c) =>
        _stack.Push(new LambdaAction("Добавление категории",
            () => _db.Categories.Insert(c),
            () => _db.Categories.Delete(c.Key)));

    public void RemoveCategory(int key)
    {
        var before = _db.Categories.ByKey(key)?.Copy();
        if (before is null) return;
        _stack.Push(new LambdaAction("Удаление категории",
            () => _db.Categories.Delete(key),
            () => _db.Categories.Insert(before)));
    }
}
