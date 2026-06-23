using FinanceTracker.Forms;
using FinanceTracker.Services;
using FinanceTracker.Store;

namespace FinanceTracker;

/// <summary>Точка входа приложения «Учёт личных финансов».</summary>
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Journal.Note("Необработанное исключение: " + e.ExceptionObject);
            MessageBox.Show("Произошла непредвиденная ошибка: " + e.ExceptionObject,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        Application.ThreadException += (_, e) =>
        {
            Journal.Note("Исключение потока UI: " + e.Exception.Message);
            MessageBox.Show("Произошла ошибка: " + e.Exception.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        var db = FinanceDb.Current;
        db.Prepare();

        var pinSvc = new PinService(db);
        while (true)
        {
            using var pin = new PinForm(pinSvc);
            if (pin.ShowDialog() != DialogResult.OK)
                return;

            var actions = new ActionStack();
            var opSvc = new OperationService(db, actions);
            var budSvc = new BudgetService(db, actions);
            var analytics = new Analytics(db);

            var result = DialogResult.None;
            using (var main = new MainForm(db, actions, opSvc, budSvc, analytics, pinSvc.Active!.Owner))
            {
                result = main.ShowDialog();
            }

            pinSvc.Leave();
            if (result != DialogResult.Retry)
                return;
        }
    }
}
