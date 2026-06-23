using FinanceTracker.Model;
using FinanceTracker.Store;

namespace FinanceTracker.Services;

/// <summary>Сводная аналитика: баланс, итоги по типам и распределение расходов по категориям.</summary>
public sealed class Analytics
{
    private readonly FinanceDb _db;
    public Analytics(FinanceDb db) => _db = db;

    public decimal TotalIncome =>
        _db.Operations.Rows.Where(o => o.Kind == OperationKind.Income).Sum(o => o.Amount);

    public decimal TotalExpense =>
        _db.Operations.Rows.Where(o => o.Kind == OperationKind.Expense).Sum(o => o.Amount);

    public decimal Balance => TotalIncome - TotalExpense;

    /// <summary>Расходы по категориям: название → сумма (для диаграммы).</summary>
    public IReadOnlyList<(string Title, decimal Sum, string ColorHex)> ExpenseByCategory()
    {
        var byCat = _db.Operations.Rows
            .Where(o => o.Kind == OperationKind.Expense)
            .GroupBy(o => o.CategoryKey)
            .Select(g =>
            {
                var cat = _db.Categories.ByKey(g.Key);
                return (Title: cat?.Title ?? "—", Sum: g.Sum(o => o.Amount), ColorHex: cat?.ColorHex ?? "#999999");
            })
            .OrderByDescending(x => x.Sum)
            .ToList();
        return byCat;
    }
}
