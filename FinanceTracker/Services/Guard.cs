using FinanceTracker.Model;

namespace FinanceTracker.Services;

/// <summary>Проверка вводимых данных. Возвращает список сообщений об ошибках.</summary>
public static class Guard
{
    public static List<string> Check(Operation o)
    {
        var errors = new List<string>();
        if (o.Amount <= 0)
            errors.Add("Сумма операции должна быть больше нуля.");
        if (o.CategoryKey <= 0)
            errors.Add("Не выбрана категория.");
        if (o.Moment.Date > DateTime.Today)
            errors.Add("Дата операции не может быть в будущем.");
        return errors;
    }

    public static List<string> Check(Category c)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(c.Title))
            errors.Add("Не указано название категории.");
        return errors;
    }

    public static List<string> Check(Budget b)
    {
        var errors = new List<string>();
        if (b.CategoryKey <= 0)
            errors.Add("Не выбрана категория бюджета.");
        if (b.Limit <= 0)
            errors.Add("Лимит должен быть больше нуля.");
        return errors;
    }
}
