using FinanceTracker.Model;
using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Диалог настройки бюджета (лимита расходов по категории).</summary>
public sealed class BudgetForm : Form
{
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _limit = new() { DecimalPlaces = 2, Maximum = 100_000_000, ThousandsSeparator = true };
    private readonly ComboBox _period = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public Budget Value { get; } = new();

    public BudgetForm(IReadOnlyList<Category> expenseCategories)
    {
        Text = "Бюджет по категории";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(360, 180);

        foreach (var c in expenseCategories.Where(c => c.Kind == OperationKind.Expense))
            _category.Items.Add(c);
        if (_category.Items.Count > 0) _category.SelectedIndex = 0;
        _period.Items.AddRange(new object[] { "Неделя", "Месяц", "Год" });
        _period.SelectedIndex = 1;

        Controls.Add(new Label { Text = "Категория:", AutoSize = true, Location = new Point(18, 22) });
        _category.SetBounds(130, 18, 210, 26); Controls.Add(_category);
        Controls.Add(new Label { Text = "Лимит:", AutoSize = true, Location = new Point(18, 60) });
        _limit.SetBounds(130, 56, 210, 26); Controls.Add(_limit);
        Controls.Add(new Label { Text = "Период:", AutoSize = true, Location = new Point(18, 98) });
        _period.SetBounds(130, 94, 210, 26); Controls.Add(_period);

        var ok = new Button { Text = "Сохранить", DialogResult = DialogResult.OK, Location = new Point(160, 132), Size = new Size(90, 32) };
        var cancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(260, 132), Size = new Size(90, 32) };
        ok.Click += OnSave;
        Controls.AddRange(new Control[] { ok, cancel });
        AcceptButton = ok; CancelButton = cancel;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        Value.CategoryKey = _category.SelectedItem is Category c ? c.Key : 0;
        Value.Limit = _limit.Value;
        Value.Period = (BudgetPeriod)Math.Max(0, _period.SelectedIndex);
        var errors = Guard.Check(Value);
        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Проверка данных",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }
}
