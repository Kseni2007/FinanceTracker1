namespace FinanceTracker.Model;

/// <summary>Профиль пользователя приложения с защитой PIN-кодом.</summary>
public sealed class Profile : IRecord
{
    public int Key { get; set; }
    public string Owner { get; set; } = "Пользователь";
    public string PinHash { get; set; } = string.Empty;
    public string Currency { get; set; } = "₽";

    public Profile Copy() => new()
    {
        Key = Key,
        Owner = Owner,
        PinHash = PinHash,
        Currency = Currency
    };
}
