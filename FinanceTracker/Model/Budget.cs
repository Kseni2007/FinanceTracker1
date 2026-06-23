namespace FinanceTracker.Model;

/// <summary>Лимит расходов по категории на период (бюджет).</summary>
public sealed class Budget : IRecord
{
    public int Key { get; set; }
    public int CategoryKey { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Month;

    /// <summary>Доля израсходованного лимита (0..1+) при заданной сумме трат.</summary>
    public double Usage(decimal spent) =>
        Limit <= 0 ? 0 : (double)(spent / Limit);

    public bool IsExceeded(decimal spent) => spent > Limit;

    public Budget Copy() => new()
    {
        Key = Key,
        CategoryKey = CategoryKey,
        CategoryTitle = CategoryTitle,
        Limit = Limit,
        Period = Period
    };
}
