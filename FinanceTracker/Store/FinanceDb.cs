using FinanceTracker.Model;
using FinanceTracker.Services;

namespace FinanceTracker.Store;

/// <summary>
/// Единое хранилище приложения (паттерн «Одиночка»). Объединяет таблицы профилей,
/// категорий, операций и бюджетов и ретранслирует их изменения единым событием
/// <see cref="Changed"/>, на которое подписан интерфейс.
/// </summary>
public sealed class FinanceDb
{
    private static readonly Lazy<FinanceDb> _lazy = new(() => new FinanceDb());
    public static FinanceDb Current => _lazy.Value;

    public Table<Profile> Profiles { get; }
    public Table<Category> Categories { get; }
    public Table<Operation> Operations { get; }
    public Table<Budget> Budgets { get; }

    /// <summary>Сводное событие изменения данных (Observer).</summary>
    public event EventHandler? Changed;

    private FinanceDb()
    {
        Profiles = new Table<Profile>("profiles");
        Categories = new Table<Category>("categories");
        Operations = new Table<Operation>("operations");
        Budgets = new Table<Budget>("budgets");

        foreach (var t in new IRelay[] { Wrap(Profiles), Wrap(Categories), Wrap(Operations), Wrap(Budgets) })
            t.Subscribe(() => Changed?.Invoke(this, EventArgs.Empty));
    }

    private interface IRelay { void Subscribe(Action handler); }

    private sealed class Relay<T> : IRelay where T : class, IRecord
    {
        private readonly Table<T> _table;
        public Relay(Table<T> table) => _table = table;
        public void Subscribe(Action handler) => _table.Mutated += (_, _) => handler();
    }

    private static IRelay Wrap<T>(Table<T> table) where T : class, IRecord => new Relay<T>(table);

    /// <summary>Первичное наполнение демонстрационными данными.</summary>
    public void Prepare()
    {
        Profiles.SeedWhenEmpty(new[]
        {
            new Profile { Owner = "Ксения", PinHash = Hashing.Pin("0000"), Currency = "₽" }
        });

        Categories.SeedWhenEmpty(new[]
        {
            new Category { Title = "Зарплата", Kind = OperationKind.Income, ColorHex = "#43A047" },
            new Category { Title = "Подработка", Kind = OperationKind.Income, ColorHex = "#26A69A" },
            new Category { Title = "Продукты", Kind = OperationKind.Expense, ColorHex = "#EF5350" },
            new Category { Title = "Транспорт", Kind = OperationKind.Expense, ColorHex = "#FFA726" },
            new Category { Title = "Развлечения", Kind = OperationKind.Expense, ColorHex = "#AB47BC" },
            new Category { Title = "Жильё", Kind = OperationKind.Expense, ColorHex = "#5C6BC0" }
        });

        if (Operations.Rows.Count == 0)
        {
            int Cat(string title) => Categories.Rows.First(c => c.Title == title).Key;
            string Title(string title) => Categories.Rows.First(c => c.Title == title).Title;

            Operation Make(int daysAgo, OperationKind kind, string category, decimal amount, PaymentWay way, string note) =>
                new()
                {
                    Moment = DateTime.Today.AddDays(-daysAgo),
                    Kind = kind,
                    CategoryKey = Cat(category),
                    CategoryTitle = Title(category),
                    Amount = amount,
                    Way = way,
                    Note = note
                };

            Operations.SeedWhenEmpty(new[]
            {
                // доходы
                Make(30, OperationKind.Income,  "Зарплата",    58000m, PaymentWay.Transfer, "Зарплата за месяц"),
                Make(15, OperationKind.Income,  "Зарплата",    25000m, PaymentWay.Transfer, "Аванс"),
                Make(9,  OperationKind.Income,  "Подработка",   7500m, PaymentWay.Card,     "Фриланс-проект"),

                // расходы — Жильё
                Make(26, OperationKind.Expense, "Жильё",       18000m, PaymentWay.Transfer, "Аренда квартиры"),
                Make(24, OperationKind.Expense, "Жильё",        4200m, PaymentWay.Transfer, "Коммунальные услуги"),

                // расходы — Продукты
                Make(28, OperationKind.Expense, "Продукты",     2450m, PaymentWay.Card,     "Гипермаркет"),
                Make(21, OperationKind.Expense, "Продукты",     1320m, PaymentWay.Card,     "Супермаркет"),
                Make(12, OperationKind.Expense, "Продукты",      890m, PaymentWay.Cash,     "Овощи и фрукты"),
                Make(5,  OperationKind.Expense, "Продукты",     3100m, PaymentWay.Card,     "Закупка на неделю"),
                Make(1,  OperationKind.Expense, "Продукты",     1240m, PaymentWay.Card,     "Продукты"),

                // расходы — Транспорт
                Make(27, OperationKind.Expense, "Транспорт",    2000m, PaymentWay.Card,     "Проездной"),
                Make(14, OperationKind.Expense, "Транспорт",     650m, PaymentWay.Cash,     "Такси"),
                Make(3,  OperationKind.Expense, "Транспорт",     430m, PaymentWay.Card,     "Метро"),

                // расходы — Развлечения
                Make(20, OperationKind.Expense, "Развлечения",  1200m, PaymentWay.Card,     "Кино"),
                Make(8,  OperationKind.Expense, "Развлечения",  2300m, PaymentWay.Card,     "Кафе с друзьями")
            });
        }

        Budgets.SeedWhenEmpty(new[]
        {
            new Budget { CategoryKey = Categories.Rows.First(c => c.Title == "Продукты").Key,
                CategoryTitle = "Продукты", Limit = 15000m, Period = BudgetPeriod.Month },
            new Budget { CategoryKey = Categories.Rows.First(c => c.Title == "Транспорт").Key,
                CategoryTitle = "Транспорт", Limit = 4000m, Period = BudgetPeriod.Month },
            new Budget { CategoryKey = Categories.Rows.First(c => c.Title == "Развлечения").Key,
                CategoryTitle = "Развлечения", Limit = 5000m, Period = BudgetPeriod.Month }
        });
    }
}
