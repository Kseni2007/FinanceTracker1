using FinanceTracker.Model;
using FinanceTracker.Store;

namespace FinanceTracker.Services;

/// <summary>Бизнес-логика бюджетов: учёт лимитов по категориям и контроль их превышения.</summary>
public sealed class BudgetService
{
    private readonly FinanceDb _db;
    private readonly ActionStack _stack;

    public BudgetService(FinanceDb db, ActionStack stack)
    {
        _db = db;
        _stack = stack;
    }

    public IReadOnlyList<Budget> All => _db.Budgets.Rows;

    public void Save(Budget b)
    {
        var cat = _db.Categories.ByKey(b.CategoryKey);
        if (cat is not null) b.CategoryTitle = cat.Title;

        if (b.Key == 0)
        {
            _stack.Push(new LambdaAction("Добавление бюджета",
                () => _db.Budgets.Insert(b),
                () => _db.Budgets.Delete(b.Key)));
        }
        else
        {
            var before = _db.Budgets.ByKey(b.Key)?.Copy();
            if (before is null) return;
            _stack.Push(new LambdaAction("Изменение бюджета",
                () => _db.Budgets.Apply(b),
                () => _db.Budgets.Apply(before)));
        }
    }

    public void Remove(int key)
    {
        var before = _db.Budgets.ByKey(key)?.Copy();
        if (before is null) return;
        _stack.Push(new LambdaAction("Удаление бюджета",
            () => _db.Budgets.Delete(key),
            () => _db.Budgets.Insert(before)));
    }

    /// <summary>Сумма расходов по категории бюджета за текущий период.</summary>
    public decimal SpentInPeriod(Budget b)
    {
        var since = b.Period switch
        {
            BudgetPeriod.Week => DateTime.Today.AddDays(-7),
            BudgetPeriod.Year => DateTime.Today.AddYears(-1),
            _ => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
        };
        return _db.Operations.Rows
            .Where(o => o.Kind == OperationKind.Expense && o.CategoryKey == b.CategoryKey && o.Moment.Date >= since)
            .Sum(o => o.Amount);
    }
}
