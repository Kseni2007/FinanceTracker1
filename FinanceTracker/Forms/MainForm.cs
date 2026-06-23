using FinanceTracker.Model;
using FinanceTracker.Services;
using FinanceTracker.Store;

namespace FinanceTracker.Forms;

/// <summary>Главное окно приложения: вкладки «Операции», «Категории», «Бюджеты».</summary>
public sealed class MainForm : Form
{
    private readonly FinanceDb _db;
    private readonly ActionStack _actions;
    private readonly OperationService _opSvc;
    private readonly BudgetService _budSvc;
    private readonly Analytics _analytics;

    private readonly TabControl _tabs = new();
    private readonly DataGridView _gridOps = new(), _gridCats = new(), _gridBud = new();

    private readonly ToolStripMenuItem _undoItem, _redoItem;
    private readonly ToolStripStatusLabel _statusBar;

    // фильтры
    private readonly TextBox _search = new() { Width = 180, PlaceholderText = "Поиск…" };
    private readonly ComboBox _kindFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 100 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 100 };

    public MainForm(FinanceDb db, ActionStack actions, OperationService opSvc,
                    BudgetService budSvc, Analytics analytics, string user)
    {
        _db = db; _actions = actions; _opSvc = opSvc; _budSvc = budSvc; _analytics = analytics;

        Text = "Учёт личных финансов — " + user;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 580);

        // ---- menu ----
        var mFile = new ToolStripMenuItem("Файл");
        mFile.DropDownItems.Add("Выписка операций…", null, (_, _) => DoStatement());
        mFile.DropDownItems.Add(new ToolStripSeparator());
        mFile.DropDownItems.Add("Выход", null, (_, _) => Close());

        var mEdit = new ToolStripMenuItem("Правка");
        _undoItem = new ToolStripMenuItem("Отменить") { ShortcutKeys = Keys.Control | Keys.Z, Enabled = false };
        _redoItem = new ToolStripMenuItem("Повторить") { ShortcutKeys = Keys.Control | Keys.Y, Enabled = false };
        _undoItem.Click += (_, _) => _actions.Undo();
        _redoItem.Click += (_, _) => _actions.Redo();
        mEdit.DropDownItems.AddRange(new ToolStripItem[] { _undoItem, _redoItem });

        var mView = new ToolStripMenuItem("Вид");
        mView.DropDownItems.Add("Статистика расходов…", null, (_, _) => new ChartForm(_analytics).ShowDialog(this));

        var mHelp = new ToolStripMenuItem("Справка");
        mHelp.DropDownItems.Add("О программе…", null, (_, _) => new AboutForm().ShowDialog(this));

        var menu = new MenuStrip();
        menu.Items.AddRange(new ToolStripItem[] { mFile, mEdit, mView, mHelp });
        MainMenuStrip = menu;

        // ---- status ----
        var status = new StatusStrip();
        _statusBar = new ToolStripStatusLabel("Баланс: …");
        status.Items.Add(_statusBar);

        // ---- tabs ----
        _tabs.Dock = DockStyle.Fill;
        var tabOps = new TabPage("Операции");
        var tabCats = new TabPage("Категории");
        var tabBud = new TabPage("Бюджеты");

        // ops toolbar
        var panOps = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, AutoSize = true, WrapContents = false };
        _kindFilter.Items.AddRange(new object[] { "Все", "Доходы", "Расходы" });
        _kindFilter.SelectedIndex = 0;
        _from.Value = DateTime.Today.AddMonths(-1); _to.Value = DateTime.Today;
        var btnFilter = new Button { Text = "Применить", Height = 28, AutoSize = true };
        btnFilter.Click += (_, _) => RefreshOps();
        var btnAdd = new Button { Text = "Добавить", Height = 28, AutoSize = true };
        btnAdd.Click += (_, _) => DoAddOp();
        var btnEdit = new Button { Text = "Изменить", Height = 28, AutoSize = true };
        btnEdit.Click += (_, _) => DoEditOp();
        var btnDel = new Button { Text = "Удалить", Height = 28, AutoSize = true };
        btnDel.Click += (_, _) => DoDelOp();
        panOps.Controls.AddRange(new Control[] { _search, _kindFilter, _from, _to, btnFilter, btnAdd, btnEdit, btnDel });

        ConfigGrid(_gridOps);
        tabOps.Controls.Add(_gridOps); tabOps.Controls.Add(panOps);

        // cats toolbar
        var panCats = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, WrapContents = false };
        var btnAddCat = new Button { Text = "Добавить", Height = 28, AutoSize = true };
        btnAddCat.Click += (_, _) => DoAddCat();
        var btnDelCat = new Button { Text = "Удалить", Height = 28, AutoSize = true };
        btnDelCat.Click += (_, _) => DoDelCat();
        panCats.Controls.AddRange(new Control[] { btnAddCat, btnDelCat });
        ConfigGrid(_gridCats);
        tabCats.Controls.Add(_gridCats); tabCats.Controls.Add(panCats);

        // budget toolbar
        var panBud = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, WrapContents = false };
        var btnAddBud = new Button { Text = "Добавить", Height = 28, AutoSize = true };
        btnAddBud.Click += (_, _) => DoAddBud();
        var btnDelBud = new Button { Text = "Удалить", Height = 28, AutoSize = true };
        btnDelBud.Click += (_, _) => DoDelBud();
        panBud.Controls.AddRange(new Control[] { btnAddBud, btnDelBud });
        ConfigGrid(_gridBud);
        tabBud.Controls.Add(_gridBud); tabBud.Controls.Add(panBud);

        _tabs.TabPages.AddRange(new[] { tabOps, tabCats, tabBud });

        // Порядок стыковки (Dock) важен: заполняющая вкладочная панель добавляется
        // первой, затем нижняя строка состояния и верхнее меню — иначе ярлычки
        // вкладок «Операции/Категории/Бюджеты» оказываются под меню и не видны.
        Controls.Add(_tabs);
        Controls.Add(status);
        Controls.Add(menu);

        // events
        _db.Changed += (_, _) => RefreshAll();
        _actions.Changed += (_, _) => UpdateUndoRedo();

        RefreshAll();
    }

    private static void ConfigGrid(DataGridView g)
    {
        g.Dock = DockStyle.Fill;
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    // ---- ops ----
    private void RefreshOps()
    {
        var chain = new FilterChain();
        chain.With(new PeriodFilter(_from.Value, _to.Value));
        if (_kindFilter.SelectedIndex == 1) chain.With(new KindFilter(OperationKind.Income));
        else if (_kindFilter.SelectedIndex == 2) chain.With(new KindFilter(OperationKind.Expense));
        if (!string.IsNullOrWhiteSpace(_search.Text)) chain.With(new TextFilter(_search.Text));
        var data = _opSvc.Query(chain);
        _gridOps.DataSource = data.Select(o => new
        {
            Дата = o.Moment.ToString("dd.MM.yyyy"),
            Тип = o.Kind == OperationKind.Income ? "Доход" : "Расход",
            Категория = o.CategoryTitle,
            Сумма = o.Amount.ToString("N2"),
            Способ = o.Way.ToString(),
            Примечание = o.Note,
            Key = o.Key
        }).ToList();
        _gridOps.Columns["Key"]!.Visible = false;
    }

    private void DoAddOp()
    {
        using var f = new OperationForm(_opSvc.Categories);
        if (f.ShowDialog(this) == DialogResult.OK) _opSvc.Add(f.Value);
    }

    private void DoEditOp()
    {
        if (_gridOps.CurrentRow is null) return;
        var key = Convert.ToInt32(_gridOps.CurrentRow.Cells["Key"].Value);
        var op = _db.Operations.ByKey(key);
        if (op is null) return;
        using var f = new OperationForm(_opSvc.Categories, op);
        if (f.ShowDialog(this) == DialogResult.OK) _opSvc.Update(f.Value);
    }

    private void DoDelOp()
    {
        if (_gridOps.CurrentRow is null) return;
        var key = Convert.ToInt32(_gridOps.CurrentRow.Cells["Key"].Value);
        if (MessageBox.Show("Удалить операцию?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            _opSvc.Remove(key);
    }

    // ---- categories ----
    private void RefreshCats()
    {
        _gridCats.DataSource = _opSvc.Categories.Select(c => new
        {
            Название = c.Title,
            Тип = c.Kind == OperationKind.Income ? "Доход" : "Расход",
            Key = c.Key
        }).ToList();
        _gridCats.Columns["Key"]!.Visible = false;
    }

    private void DoAddCat()
    {
        using var f = new CategoryForm();
        if (f.ShowDialog(this) == DialogResult.OK) _opSvc.AddCategory(f.Value);
    }

    private void DoDelCat()
    {
        if (_gridCats.CurrentRow is null) return;
        var key = Convert.ToInt32(_gridCats.CurrentRow.Cells["Key"].Value);
        if (MessageBox.Show("Удалить категорию?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            _opSvc.RemoveCategory(key);
    }

    // ---- budget ----
    private void RefreshBudgets()
    {
        _gridBud.DataSource = _budSvc.All.Select(b =>
        {
            var spent = _budSvc.SpentInPeriod(b);
            return new
            {
                Категория = b.CategoryTitle,
                Лимит = b.Limit.ToString("N2"),
                Потрачено = spent.ToString("N2"),
                Остаток = (b.Limit - spent).ToString("N2"),
                Период = b.Period switch { BudgetPeriod.Week => "Неделя", BudgetPeriod.Year => "Год", _ => "Месяц" },
                Key = b.Key
            };
        }).ToList();
        _gridBud.Columns["Key"]!.Visible = false;
    }

    private void DoAddBud()
    {
        using var f = new BudgetForm(_opSvc.Categories);
        if (f.ShowDialog(this) == DialogResult.OK) _budSvc.Save(f.Value);
    }

    private void DoDelBud()
    {
        if (_gridBud.CurrentRow is null) return;
        var key = Convert.ToInt32(_gridBud.CurrentRow.Cells["Key"].Value);
        if (MessageBox.Show("Удалить бюджет?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            _budSvc.Remove(key);
    }

    // ---- refresh ----
    private void RefreshAll()
    {
        if (InvokeRequired) { Invoke(RefreshAll); return; }
        RefreshOps();
        RefreshCats();
        RefreshBudgets();
        _statusBar.Text = $"Доходы: {_analytics.TotalIncome:N0} ₽   Расходы: {_analytics.TotalExpense:N0} ₽   Баланс: {_analytics.Balance:N0} ₽";
    }

    private void DoStatement()
    {
        var ops = _db.Operations.Rows;
        using var f = new StatementForm(ops);
        f.ShowDialog(this);
    }

    private void UpdateUndoRedo()
    {
        if (InvokeRequired) { Invoke(UpdateUndoRedo); return; }
        _undoItem.Enabled = _actions.CanUndo;
        _undoItem.Text = _actions.CanUndo ? "Отменить: " + _actions.UndoCaption : "Отменить";
        _redoItem.Enabled = _actions.CanRedo;
        _redoItem.Text = _actions.CanRedo ? "Повторить: " + _actions.RedoCaption : "Повторить";
    }
}
