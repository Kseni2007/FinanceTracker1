namespace FinanceTracker.Forms;

/// <summary>Окно «О программе».</summary>
public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "О программе";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(420, 200);

        var text = new Label
        {
            AutoSize = false,
            Location = new Point(20, 20),
            Size = new Size(380, 130),
            Text = "Учёт личных финансов\n\n" +
                   "Настольное приложение для учёта доходов и расходов,\n" +
                   "планирования бюджета и анализа трат по категориям.\n\n" +
                   "C# / Windows Forms (.NET 9).\n" +
                   "Производственная практика ПП.02.01."
        };
        var ok = new Button { Text = "ОК", DialogResult = DialogResult.OK, Location = new Point(315, 155), Size = new Size(85, 32) };
        Controls.AddRange(new Control[] { text, ok });
        AcceptButton = ok;
    }
}
