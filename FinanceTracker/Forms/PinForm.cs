using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Окно входа по PIN-коду.</summary>
public sealed class PinForm : Form
{
    private readonly PinService _pin;
    private readonly TextBox _box = new() { PasswordChar = '•', MaxLength = 8, TextAlign = HorizontalAlignment.Center };
    private readonly Label _hint = new() { ForeColor = Color.Firebrick, AutoSize = true };

    public PinForm(PinService pin)
    {
        _pin = pin;
        Text = "Учёт личных финансов — вход";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(320, 200);

        var title = new Label { Text = "Введите PIN-код", AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20) };
        _box.SetBounds(20, 60, 280, 30);
        _box.Font = new Font("Segoe UI", 14F);
        _hint.Location = new Point(20, 95);

        var ok = new Button { Text = "Войти", DialogResult = DialogResult.OK, Location = new Point(20, 130), Size = new Size(135, 36) };
        var cancel = new Button { Text = "Выход", DialogResult = DialogResult.Cancel, Location = new Point(165, 130), Size = new Size(135, 36) };
        ok.Click += OnOk;

        Controls.AddRange(new Control[] { title, _box, _hint, ok, cancel });
        AcceptButton = ok; CancelButton = cancel;

        var demo = new Label { Text = "Демо-код: 0000", AutoSize = true, ForeColor = Color.Gray, Location = new Point(20, 175) };
        Controls.Add(demo);
    }

    private void OnOk(object? sender, EventArgs e)
    {
        if (_pin.Enter(_box.Text))
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            _hint.Text = "Неверный PIN-код";
            _box.SelectAll();
            _box.Focus();
            DialogResult = DialogResult.None;
        }
    }
}
