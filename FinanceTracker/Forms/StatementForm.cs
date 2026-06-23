using System.IO;
using FinanceTracker.Model;
using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Окно формирования выписки операций (выбор формата и сохранение в файл).</summary>
public sealed class StatementForm : Form
{
    private readonly ComboBox _format = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly IReadOnlyList<Operation> _operations;

    public StatementForm(IReadOnlyList<Operation> operations)
    {
        _operations = operations;
        Text = "Формирование выписки";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(360, 160);

        _format.Items.AddRange(new object[] { "CSV (таблица)", "HTML (страница)", "Markdown (текст)" });
        _format.SelectedIndex = 0;

        Controls.Add(new Label { Text = "Формат:", AutoSize = true, Location = new Point(18, 24) });
        _format.SetBounds(120, 20, 220, 26); Controls.Add(_format);
        Controls.Add(new Label { Text = $"Операций к выгрузке: {_operations.Count}", AutoSize = true, Location = new Point(18, 64) });

        var save = new Button { Text = "Сохранить…", Location = new Point(120, 110), Size = new Size(110, 32) };
        var cancel = new Button { Text = "Закрыть", DialogResult = DialogResult.Cancel, Location = new Point(240, 110), Size = new Size(100, 32) };
        save.Click += OnSave;
        Controls.AddRange(new Control[] { save, cancel });
        CancelButton = cancel;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        var format = (StatementFormat)_format.SelectedIndex;
        var writer = StatementFactory.Create(format);
        using var dlg = new SaveFileDialog
        {
            FileName = "Выписка_" + DateTime.Today.ToString("yyyyMMdd") + writer.Extension,
            Filter = $"Файл выписки (*{writer.Extension})|*{writer.Extension}|Все файлы (*.*)|*.*"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        File.WriteAllText(dlg.FileName, writer.Build(_operations));
        Journal.Note("Сформирована выписка: " + dlg.FileName);
        MessageBox.Show("Выписка сохранена:\n" + dlg.FileName, "Готово",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }
}
