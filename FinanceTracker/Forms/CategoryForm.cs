using FinanceTracker.Model;
using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Диалог создания категории доходов/расходов.</summary>
public sealed class CategoryForm : Form
{
    private readonly TextBox _title = new();
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public Category Value { get; } = new();

    public CategoryForm()
    {
        Text = "Новая категория";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(340, 150);

        _kind.Items.AddRange(new object[] { "Доход", "Расход" });
        _kind.SelectedIndex = 1;

        Controls.Add(new Label { Text = "Название:", AutoSize = true, Location = new Point(18, 22) });
        _title.SetBounds(120, 18, 200, 26);
        Controls.Add(_title);

        Controls.Add(new Label { Text = "Тип:", AutoSize = true, Location = new Point(18, 60) });
        _kind.SetBounds(120, 56, 200, 26);
        Controls.Add(_kind);

        var ok = new Button { Text = "Сохранить", DialogResult = DialogResult.OK, Location = new Point(140, 100), Size = new Size(90, 32) };
        var cancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(240, 100), Size = new Size(90, 32) };
        ok.Click += OnSave;
        Controls.AddRange(new Control[] { ok, cancel });
        AcceptButton = ok; CancelButton = cancel;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        Value.Title = _title.Text.Trim();
        Value.Kind = _kind.SelectedIndex == 0 ? OperationKind.Income : OperationKind.Expense;
        var errors = Guard.Check(Value);
        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Проверка данных",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }
}
