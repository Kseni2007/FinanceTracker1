using System.Globalization;
using FinanceTracker.Services;

namespace FinanceTracker.Forms;

/// <summary>Окно статистики: круговая диаграмма расходов по категориям средствами GDI+.</summary>
public sealed class ChartForm : Form
{
    private readonly Analytics _analytics;

    public ChartForm(Analytics analytics)
    {
        _analytics = analytics;
        Text = "Статистика расходов по категориям";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(640, 420);
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var data = _analytics.ExpenseByCategory();
        var total = data.Sum(d => d.Sum);
        if (total <= 0)
        {
            g.DrawString("Нет данных о расходах", new Font("Segoe UI", 12F), Brushes.Gray, 20, 20);
            return;
        }

        var pie = new Rectangle(30, 40, 340, 340);
        float start = 0f;
        int legendY = 50;
        foreach (var (title, sum, colorHex) in data)
        {
            var sweep = (float)(sum / total) * 360f;
            using var brush = new SolidBrush(Parse(colorHex));
            g.FillPie(brush, pie, start, sweep);
            start += sweep;

            g.FillRectangle(brush, 400, legendY, 16, 16);
            var pct = (double)(sum / total) * 100.0;
            g.DrawString($"{title} — {sum:N0} ₽ ({pct:0.#}%)",
                new Font("Segoe UI", 10F), Brushes.Black, 424, legendY);
            legendY += 26;
        }

        g.DrawString($"Всего расходов: {total:N0} ₽", new Font("Segoe UI", 11F, FontStyle.Bold),
            Brushes.Black, 400, legendY + 10);
    }

    private static Color Parse(string hex)
    {
        try { return ColorTranslator.FromHtml(hex); }
        catch { return Color.SteelBlue; }
    }
}
