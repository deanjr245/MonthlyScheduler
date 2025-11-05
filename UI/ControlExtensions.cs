using System.Drawing.Drawing2D;

namespace MonthlyScheduler.UI;

public static class ControlExtensions
{
    public static void ApplyModernStyle(this Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = AppStyling.Primary;
        button.ForeColor = Color.White;
        button.Font = AppStyling.Font;
        button.Padding = new Padding(5, 0, 5, 0);
        button.Cursor = Cursors.Hand;
        button.MinimumSize = new Size(130, 35);
    }

    public static void ApplySecondaryStyle(this Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = AppStyling.Secondary;
        button.ForeColor = Color.White;
        button.Font = AppStyling.Font;
        button.Padding = new Padding(5, 0, 5, 0);
        button.Cursor = Cursors.Hand;
        button.MinimumSize = new Size(130, 35);
    }

    public static void ApplyModernStyle(this DataGridView grid)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        grid.RowsDefaultCellStyle.BackColor = Color.White;
        grid.BackgroundColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 245, 255);
        grid.DefaultCellStyle.SelectionForeColor = AppStyling.DarkText;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.RowsDefaultCellStyle.Padding = new Padding(3);
        grid.DefaultCellStyle.Font = AppStyling.Font;
        grid.ColumnHeadersDefaultCellStyle.Font = AppStyling.FontBold;
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppStyling.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = AppStyling.Border;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToOrderColumns = true;
        grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        grid.MultiSelect = false;
        grid.RowTemplate.Height = 35;
        grid.ColumnHeadersHeight = 35;
    }

    public static void ApplyModernStyle(this ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = AppStyling.Font;
        comboBox.BackColor = Color.White;
        comboBox.ForeColor = Color.FromArgb(33, 37, 41);
        comboBox.Height = 35;
    }

    public static void ApplyModernStyle(this NumericUpDown numericUpDown)
    {
        numericUpDown.Font = AppStyling.Font;
        numericUpDown.BackColor = Color.White;
        numericUpDown.ForeColor = Color.FromArgb(33, 37, 41);
        numericUpDown.Height = 35;
    }

    public static void ApplyModernStyle(this Label label)
    {
        label.Font = AppStyling.Font;
        label.ForeColor = AppStyling.DarkText;
    }

    public static void ApplyModernStyle(this Panel panel)
    {
        panel.BackColor = AppStyling.DarkBackground;
        panel.BorderStyle = BorderStyle.None;
    }
}