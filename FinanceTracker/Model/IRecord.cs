namespace FinanceTracker.Model;

/// <summary>Базовый контракт записи хранилища: каждая запись имеет числовой ключ.</summary>
public interface IRecord
{
    int Key { get; set; }
}
