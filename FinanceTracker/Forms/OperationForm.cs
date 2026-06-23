using FinanceTracker.Model;
using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Диалог добавления и редактирования операции.</summary>
public sealed class OperationForm : Form
{
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short };
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _amount = new() { DecimalPlaces = 2, Maximum = 100_000_000, ThousandsSeparator = true };
    private readonly ComboBox _way = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _note = new();

    private readonly IReadOnlyList<Category> _categories;
    public Operation Value { get; }

    public OperationForm(IReadOnlyList<Category> categories, Operation? source = null)
    {
        _categories = categories;
        Value = source?.Copy() ?? new Operation();

        Text = source is null ? "Новая операция" : "Изменение операции";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(380, 290);

        _kind.Items.AddRange(new object[] { "Доход", "Расход" });
        _way.Items.AddRange(new object[] { "Наличные", "Карта", "Перевод", "Другое" });

        int y = 18;
        AddRow("Дата:", _date, ref y);
        AddRow("Тип:", _kind, ref y);
        AddRow("Категория:", _category, ref y);
        AddRow("Сумма:", _amount, ref y);
        AddRow("Способ:", _way, ref y);
        AddRow("Примечание:", _note, ref y);

        _kind.SelectedIndexChanged += (_, _) => ReloadCategories();

        var ok = new Button { Text = "Сохранить", DialogResult = DialogResult.OK, Location = new Point(180, y + 10), Size = new Size(90, 32) };
        var cancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(280, y + 10), Size = new Size(90, 32) };
        ok.Click += OnSave;
        Controls.AddRange(new Control[] { ok, cancel });
        AcceptButton = ok; CancelButton = cancel;

        LoadValue();
    }

    private void AddRow(string caption, Control input, ref int y)
    {
        Controls.Add(new Label { Text = caption, AutoSize = true, Location = new Point(18, y + 4) });
        input.SetBounds(140, y, 220, 26);
        Controls.Add(input);
        y += 38;
    }

    private void ReloadCategories()
    {
        var kind = _kind.SelectedIndex == 0 ? OperationKind.Income : OperationKind.Expense;
        _category.Items.Clear();
        foreach (var c in _categories.Where(c => c.Kind == kind))
            _category.Items.Add(c);
        if (_category.Items.Count > 0) _category.SelectedIndex = 0;
    }

    private void LoadValue()
    {
        _date.Value = Value.Moment == default ? DateTime.Today : Value.Moment;
        _kind.SelectedIndex = Value.Kind == OperationKind.Income ? 0 : 1;
        ReloadCategories();
        for (int i = 0; i < _category.Items.Count; i++)
            if (((Category)_category.Items[i]!).Key == Value.CategoryKey) _category.SelectedIndex = i;
        _amount.Value = Value.Amount;
        _way.SelectedIndex = (int)Value.Way;
        _note.Text = Value.Note;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        Value.Moment = _date.Value.Date;
        Value.Kind = _kind.SelectedIndex == 0 ? OperationKind.Income : OperationKind.Expense;
        Value.CategoryKey = _category.SelectedItem is Category c ? c.Key : 0;
        Value.Amount = _amount.Value;
        Value.Way = (PaymentWay)Math.Max(0, _way.SelectedIndex);
        Value.Note = _note.Text.Trim();

        var errors = Guard.Check(Value);
        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Проверка данных",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }
}
