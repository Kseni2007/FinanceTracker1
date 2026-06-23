namespace FinanceTracker.Model;

/// <summary>Категория доходов или расходов (например, «Продукты», «Зарплата»).</summary>
public sealed class Category : IRecord
{
    public int Key { get; set; }
    public string Title { get; set; } = string.Empty;
    public OperationKind Kind { get; set; } = OperationKind.Expense;
    public string ColorHex { get; set; } = "#4C8BF5";

    public override string ToString() => Title;

    public Category Copy() => new()
    {
        Key = Key,
        Title = Title,
        Kind = Kind,
        ColorHex = ColorHex
    };
}
