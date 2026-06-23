namespace FinanceTracker.Model;

/// <summary>Финансовая операция — доход или расход за конкретную дату.</summary>
public sealed class Operation : IRecord
{
    public int Key { get; set; }
    public DateTime Moment { get; set; } = DateTime.Today;
    public OperationKind Kind { get; set; } = OperationKind.Expense;
    public int CategoryKey { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentWay Way { get; set; } = PaymentWay.Card;
    public string Note { get; set; } = string.Empty;

    /// <summary>Знаковая сумма: расход берётся со знаком «минус».</summary>
    public decimal Signed => Kind == OperationKind.Income ? Amount : -Amount;

    public override string ToString() =>
        $"{Moment:dd.MM.yyyy} {CategoryTitle} {Signed:+0.00;-0.00} ₽";

    /// <summary>Снимок состояния операции (используется для отмены изменений).</summary>
    public Operation Copy() => new()
    {
        Key = Key,
        Moment = Moment,
        Kind = Kind,
        CategoryKey = CategoryKey,
        CategoryTitle = CategoryTitle,
        Amount = Amount,
        Way = Way,
        Note = Note
    };
}
