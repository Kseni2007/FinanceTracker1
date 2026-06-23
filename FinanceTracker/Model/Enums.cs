namespace FinanceTracker.Model;

/// <summary>Тип финансовой операции.</summary>
public enum OperationKind
{
    Income,   // доход
    Expense   // расход
}

/// <summary>Период повторения для бюджета/лимита.</summary>
public enum BudgetPeriod
{
    Week,
    Month,
    Year
}

/// <summary>Способ оплаты операции.</summary>
public enum PaymentWay
{
    Cash,
    Card,
    Transfer,
    Other
}
