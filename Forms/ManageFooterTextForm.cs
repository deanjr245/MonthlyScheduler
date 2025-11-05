using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Forms;

public class ManageFooterTextForm : Form
{
    private readonly SchedulerDbContext _context;
    private TextBox _worshipTextBox = null!;
    private TextBox _avTextBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;

    public ManageFooterTextForm(SchedulerDbContext context)
    {
        _context = context;
        InitializeComponents();
        LoadFooterTexts();
    }

    private void InitializeComponents()
    {
        Text = "Manage PDF Footer Text";
        Size = new Size(600, 350);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppStyling.LightBackground;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = AppStyling.LightBackground
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Worship label
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); // Worship textbox
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // AV label
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); // AV textbox
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons

        // Worship label
        var worshipLabel = new Label
        {
            Text = "Worship Assignments Footer Text:",
            AutoSize = true,
            Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 1, FontStyle.Bold),
            ForeColor = AppStyling.DarkText,
            Margin = new Padding(0, 0, 0, 5)
        };

        // Worship textbox
        _worshipTextBox = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size),
            Margin = new Padding(0, 0, 0, 15)
        };

        // AV label
        var avLabel = new Label
        {
            Text = "Audio-Visual Assignments Footer Text:",
            AutoSize = true,
            Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 1, FontStyle.Bold),
            ForeColor = AppStyling.DarkText,
            Margin = new Padding(0, 0, 0, 5)
        };

        // AV textbox
        _avTextBox = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size),
            Margin = new Padding(0, 0, 0, 15)
        };

        // Buttons panel
        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Margin = new Padding(0)
        };

        _saveButton = new Button
        {
            Text = "Save",
            Width = 100,
            Height = 35,
            Margin = new Padding(0, 0, 10, 0)
        };
        _saveButton.Click += SaveButton_Click;
        _saveButton.ApplyModernStyle();

        _cancelButton = new Button
        {
            Text = "Cancel",
            Width = 100,
            Height = 35,
            Margin = new Padding(0)
        };
        _cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        _cancelButton.ApplySecondaryStyle();

        buttonsPanel.Controls.Add(_cancelButton);
        buttonsPanel.Controls.Add(_saveButton);

        // Add controls to layout
        mainLayout.Controls.Add(worshipLabel, 0, 0);
        mainLayout.Controls.Add(_worshipTextBox, 0, 1);
        mainLayout.Controls.Add(avLabel, 0, 2);
        mainLayout.Controls.Add(_avTextBox, 0, 3);
        mainLayout.Controls.Add(buttonsPanel, 0, 4);

        Controls.Add(mainLayout);
    }

    private async void LoadFooterTexts()
    {
        try
        {
            var worshipFooter = await _context.CategoryFooterTexts
                .FirstOrDefaultAsync(f => f.Category == DutyCategory.Worship);
            var avFooter = await _context.CategoryFooterTexts
                .FirstOrDefaultAsync(f => f.Category == DutyCategory.AudioVisual);

            _worshipTextBox.Text = worshipFooter?.FooterText ?? string.Empty;
            _avTextBox.Text = avFooter?.FooterText ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading footer text: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Update or create Worship footer
            var worshipFooter = await _context.CategoryFooterTexts
                .FirstOrDefaultAsync(f => f.Category == DutyCategory.Worship);

            if (worshipFooter == null)
            {
                worshipFooter = new CategoryFooterText
                {
                    Category = DutyCategory.Worship,
                    FooterText = _worshipTextBox.Text
                };
                _context.CategoryFooterTexts.Add(worshipFooter);
            }
            else
            {
                worshipFooter.FooterText = _worshipTextBox.Text;
            }

            // Update or create AV footer
            var avFooter = await _context.CategoryFooterTexts
                .FirstOrDefaultAsync(f => f.Category == DutyCategory.AudioVisual);

            if (avFooter == null)
            {
                avFooter = new CategoryFooterText
                {
                    Category = DutyCategory.AudioVisual,
                    FooterText = _avTextBox.Text
                };
                _context.CategoryFooterTexts.Add(avFooter);
            }
            else
            {
                avFooter.FooterText = _avTextBox.Text;
            }

            await _context.SaveChangesAsync();

            MessageBox.Show("Footer text saved successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving footer text: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
