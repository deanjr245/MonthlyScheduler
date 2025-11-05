using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Forms;

public class SavedSchedulesForm : Form
{
    private const string ConfirmDeleteScheduleFormat = "Are you sure you want to delete the schedule for {0} {1}?";
    private const string ConfirmDeleteTitle = "Confirm Delete";
    
    private readonly SchedulerDbContext _context;
    private readonly DataGridView _schedulesGrid;
    private readonly Button _btnLoad;
    private readonly Button _btnClose;
    private readonly Button _btnDelete;
    private readonly Button _btnManageFooterText;
    
    public event EventHandler<GeneratedSchedule>? ScheduleLoaded;

    public SavedSchedulesForm(SchedulerDbContext context)
    {
        _context = context;
        Text = "Saved Schedules";
        Size = new Size(800, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppStyling.LightBackground;
        
        // ===== Initialize all buttons first =====
        _btnManageFooterText = new Button
        {
            Text = "Manage PDF Footer Text",
            Width = 180
        };
        _btnManageFooterText.ApplyModernStyle();
        _btnManageFooterText.Click += BtnManageFooterText_Click;

        _btnDelete = new Button
        {
            Text = "Delete Schedule"
        };
        _btnDelete.ApplySecondaryStyle();
        _btnDelete.Click += BtnDelete_Click;

        _btnLoad = new Button
        {
            Text = "Load Schedule"
        };
        _btnLoad.ApplyModernStyle();
        _btnLoad.Click += BtnLoad_Click;

        _btnClose = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.Cancel
        };
        _btnClose.ApplySecondaryStyle();

        // Create grid
        _schedulesGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _schedulesGrid.ApplyModernStyle();
        _schedulesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 245, 255);
        _schedulesGrid.DefaultCellStyle.SelectionForeColor = AppStyling.DarkText;
        _schedulesGrid.ColumnHeadersDefaultCellStyle.BackColor = AppStyling.Primary;
        _schedulesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _schedulesGrid.CellClick += SchedulesGrid_CellClick;

        // ===== Create layout structure =====
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Button panel with left and right sections
        var buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 10, 0, 0)
        };
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        var leftPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        var rightPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        // ===== Add buttons to panels =====
        leftPanel.Controls.Add(_btnManageFooterText);
        rightPanel.Controls.AddRange(new Control[] { _btnClose, _btnLoad, _btnDelete });

        // ===== Assemble the layout hierarchy =====
        buttonPanel.Controls.Add(leftPanel, 0, 0);
        buttonPanel.Controls.Add(rightPanel, 1, 0);
        
        mainLayout.Controls.Add(_schedulesGrid, 0, 0);
        mainLayout.Controls.Add(buttonPanel, 0, 1);
        
        Controls.Add(mainLayout);

        // ===== Load initial data after form is loaded =====
        this.Load += LoadSchedules;
    }

    private async void LoadSchedules(object? sender, EventArgs e)
    {
        try
        {
            // First clear everything
            _schedulesGrid.DataSource = null;
            _schedulesGrid.Columns.Clear();

            var schedules = await _context.GeneratedSchedules
                .AsNoTracking()
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .ThenByDescending(s => s.GeneratedDate)
                .Select(s => new
                {
                    s.Id,
                    Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month),
                    s.Year,
                    Generated = s.GeneratedDate
                })
                .ToListAsync();

            // Set the data source
            _schedulesGrid.DataSource = schedules;

            // Hide ID column
            if (_schedulesGrid.Columns["Id"] is DataGridViewColumn idColumn)
            {
                idColumn.Visible = false;
            }

            // Add checkbox button column at the beginning
            var checkboxColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Selected",
                HeaderText = "",
                Width = 40,
                FalseValue = false,
                TrueValue = true
            };
            _schedulesGrid.Columns.Insert(0, checkboxColumn);

            // Add export button columns
            var csvBtn = new DataGridViewButtonColumn
            {
                Name = "ExportCSV",
                HeaderText = "Export CSV",
                Text = "CSV",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            csvBtn.DefaultCellStyle.BackColor = AppStyling.Success;
            csvBtn.DefaultCellStyle.ForeColor = Color.White;
            _schedulesGrid.Columns.Add(csvBtn);

            var pdfBtn = new DataGridViewButtonColumn
            {
                Name = "ExportPDF",
                HeaderText = "Export PDF",
                Text = "PDF",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            pdfBtn.DefaultCellStyle.BackColor = AppStyling.Danger;
            pdfBtn.DefaultCellStyle.ForeColor = Color.White;
            _schedulesGrid.Columns.Add(pdfBtn);

            // Configure grid column sizing
            if (_schedulesGrid.Columns["Selected"] is DataGridViewColumn selectedCol)
            {
                selectedCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                selectedCol.Width = 40;
            }
            
            if (_schedulesGrid.Columns["ExportCSV"] is DataGridViewColumn csvCol)
            {
                csvCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                csvCol.Width = 100;
            }
            
            if (_schedulesGrid.Columns["ExportPDF"] is DataGridViewColumn pdfCol)
            {
                pdfCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                pdfCol.Width = 100;
            }
            
            // Let the remaining columns (Month, Year, Generated) fill the space
            if (_schedulesGrid.Columns["Month"] is DataGridViewColumn monthCol)
                monthCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            if (_schedulesGrid.Columns["Year"] is DataGridViewColumn yearCol)
                yearCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            if (_schedulesGrid.Columns["Generated"] is DataGridViewColumn genCol)
                genCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            // Select the first row by default if any schedules exist
            if (_schedulesGrid.Rows.Count > 0)
            {
                UpdateSelectedRow(0);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading schedules: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        // Find the row with the checked checkbox button
        DataGridViewRow? selectedRow = null;
        foreach (DataGridViewRow row in _schedulesGrid.Rows)
        {
            if (row.Cells["Selected"].Value is bool isSelected && isSelected)
            {
                selectedRow = row;
                break;
            }
        }

        if (selectedRow == null)
        {
            MessageBox.Show("No schedule selected.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var scheduleId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
        var monthName = selectedRow.Cells["Month"].Value?.ToString() ?? "";
        var year = Convert.ToInt32(selectedRow.Cells["Year"].Value);

        if (MessageBox.Show(
            string.Format(ConfirmDeleteScheduleFormat, monthName, year),
            ConfirmDeleteTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            try
            {
                var scheduleToDelete = await _context.GeneratedSchedules
                    .FirstOrDefaultAsync(s => s.Id == scheduleId);

                if (scheduleToDelete != null)
                {
                    _context.GeneratedSchedules.Remove(scheduleToDelete);
                    await _context.SaveChangesAsync();
                    
                    // Reload the grid and auto-select the first row if one exists
                    LoadSchedules(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting schedule: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void UpdateSelectedRow(int rowIndex)
    {
        // Clear all checkbox buttons first
        foreach (DataGridViewRow row in _schedulesGrid.Rows)
        {
            row.Cells["Selected"].Value = false;
        }

        // Select this checkbox button
        _schedulesGrid.Rows[rowIndex].Cells["Selected"].Value = true;
        
        // Force the checkbox to refresh and display the checked state
        _schedulesGrid.RefreshEdit();
    }

    private async void SchedulesGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var columnName = _schedulesGrid.Columns[e.ColumnIndex].Name;
        
        // Handle row clicks (not on buttons or checkbox button)
        if (columnName != "ExportCSV" && columnName != "ExportPDF")
        {
            UpdateSelectedRow(e.RowIndex);
            return;
        }

        try
        {
            var row = _schedulesGrid.Rows[e.RowIndex];
            var scheduleId = Convert.ToInt32(row.Cells["Id"].Value);
            var month = row.Cells["Month"].Value?.ToString() ?? "";
            var year = Convert.ToInt32(row.Cells["Year"].Value);
            var monthNumber = DateTime.ParseExact(month, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;

            // Load the schedule data
            var scheduleLoader = new Services.ScheduleLoaderService(_context);
            var scheduleData = await scheduleLoader.LoadScheduleData(year, monthNumber);
            var title = $"{month} {year} Schedule";

            if (columnName == "ExportCSV")
            {
                using SaveFileDialog saveFileDialog = new()
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Export Schedule to CSV",
                    FileName = $"Schedule_{month}_{year}.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var exportService = new Services.ScheduleExportService();
                    exportService.ExportToCSV(scheduleData, saveFileDialog.FileName, title);
                    MessageBox.Show("Schedule exported successfully!", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (columnName == "ExportPDF")
            {
                using SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PDF Files|*.pdf",
                    Title = "Export Schedule to PDF",
                    FileName = $"Schedule_{month}_{year}.pdf"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var exportService = new Services.ScheduleExportService();
                    exportService.ExportToPDF(scheduleData, saveFileDialog.FileName, title);
                    MessageBox.Show("Schedule exported successfully!", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Open the PDF file with the default application
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception openEx)
                    {
                        // If opening fails, just show a message but don't crash
                        MessageBox.Show($"PDF saved but could not open automatically: {openEx.Message}", 
                            "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting schedule: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnManageFooterText_Click(object? sender, EventArgs e)
    {
        try
        {
            var manageFooterTextForm = new ManageFooterTextForm(_context);
            manageFooterTextForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error managing footer text: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnLoad_Click(object? sender, EventArgs e)
    {
        // Find the row with the checked checkbox button
        DataGridViewRow? selectedRow = null;
        foreach (DataGridViewRow row in _schedulesGrid.Rows)
        {
            if (row.Cells["Selected"].Value is bool isSelected && isSelected)
            {
                selectedRow = row;
                break;
            }
        }

        if (selectedRow == null)
        {
            MessageBox.Show("No schedule selected.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var scheduleId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            // NOW load the full schedule with all related data
            var fullSchedule = await _context.GeneratedSchedules
                .Include(s => s.DailySchedules)
                    .ThenInclude(d => d.Assignments)
                        .ThenInclude(a => a.Member)
                .Include(s => s.DailySchedules)
                    .ThenInclude(d => d.Assignments)
                        .ThenInclude(a => a.DutyType)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (fullSchedule != null)
            {
                // Raise the event so Form1 can handle the loading
                ScheduleLoaded?.Invoke(this, fullSchedule);
                Close();
            }
            else
            {
                MessageBox.Show("Schedule not found.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading schedule: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
