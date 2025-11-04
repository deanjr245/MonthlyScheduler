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
    private GeneratedSchedule? _selectedSchedule;

    public GeneratedSchedule? SelectedSchedule => _selectedSchedule;

    public SavedSchedulesForm(SchedulerDbContext context)
    {
        _context = context;
        Text = "Saved Schedules";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppStyling.LightBackground;
        
        // Add a delete button and manage its state
        _btnDelete = new Button
        {
            Text = "Delete Schedule",
            Width = 120,
            Enabled = false
        };
        _btnDelete.ApplySecondaryStyle();
        _btnDelete.Click += BtnDelete_Click;

        // Create main layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        // Create grid
        _schedulesGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _schedulesGrid.ApplyModernStyle();
        
        // Configure grid selection and styling
        _schedulesGrid.RowHeadersVisible = false;
        _schedulesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _schedulesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 245, 255);
        _schedulesGrid.DefaultCellStyle.SelectionForeColor = AppStyling.DarkText;
        _schedulesGrid.ColumnHeadersDefaultCellStyle.BackColor = AppStyling.Primary;
        _schedulesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

        // Create button panel
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnClose = new Button
        {
            Text = "Close",
            Width = 100,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.ApplySecondaryStyle();

        _btnLoad = new Button
        {
            Text = "Load Schedule",
            Width = 120,
            Enabled = false,
            DialogResult = DialogResult.OK
        };
        _btnLoad.ApplyModernStyle();

        buttonPanel.Controls.AddRange(new Control[] { _btnClose, _btnLoad, _btnDelete });

        // Add controls to layout
        mainLayout.Controls.Add(_schedulesGrid, 0, 0);
        mainLayout.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainLayout);

        // Add cell click handler for export buttons
        _schedulesGrid.CellClick += SchedulesGrid_CellClick;

        // Load data
        _ = LoadSchedules();
    }

    private async Task LoadSchedules()
    {
        try
        {
            // First clear everything
            _schedulesGrid.DataSource = null;
            _schedulesGrid.Columns.Clear();

            // Add checkbox column before setting data source
            var selectionColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Selected",
                HeaderText = "",
                Width = 30,
                ReadOnly = true
            };
            _schedulesGrid.Columns.Add(selectionColumn);

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

            // Now set the data source and hook up the selection changed event
            _schedulesGrid.SelectionChanged -= SchedulesGrid_SelectionChanged; // Remove old handler if exists
            _schedulesGrid.DataSource = schedules;
            _schedulesGrid.SelectionChanged += SchedulesGrid_SelectionChanged; // Add new handler

            // Add export button columns
            if (!_schedulesGrid.Columns.Contains("ExportCSV"))
            {
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
            }

            if (!_schedulesGrid.Columns.Contains("ExportPDF"))
            {
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
            }

            // Hide ID column
            if (_schedulesGrid.Columns["Id"] is DataGridViewColumn idColumn)
            {
                idColumn.Visible = false;
            }

            // Add checkbox column if it doesn't exist
            if (!_schedulesGrid.Columns.Contains("Selected"))
            {
                var checkboxColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "Selected",
                    HeaderText = "",
                    Width = 30,
                    ReadOnly = true
                };
                _schedulesGrid.Columns.Insert(0, checkboxColumn);
                
                // Initialize checkbox values to false
                foreach (DataGridViewRow row in _schedulesGrid.Rows)
                {
                    row.Cells["Selected"].Value = false;
                }
            }

            // Configure grid appearance
            foreach (DataGridViewColumn col in _schedulesGrid.Columns)
            {
                if (col.Name != "Id")
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            
            // Clear selection state
            _selectedSchedule = null;
            _btnLoad.Enabled = false;
            _btnDelete.Enabled = false;
            _schedulesGrid.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading schedules: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void SchedulesGrid_SelectionChanged(object? sender, EventArgs e)
    {
        try
        {
            _selectedSchedule = null;
            _btnLoad.Enabled = false;
            _btnDelete.Enabled = false;

            // Clear all checkboxes first using LINQ
            _schedulesGrid.Rows.Cast<DataGridViewRow>().ToList()
                .ForEach(row => row.Cells["Selected"].Value = false);

            if (_schedulesGrid.SelectedRows.Count > 0)
            {
                var row = _schedulesGrid.SelectedRows[0];
                if (row?.Cells["Id"]?.Value != null)
                {
                    var scheduleId = Convert.ToInt32(row.Cells["Id"].Value);
                
                _selectedSchedule = await _context.GeneratedSchedules
                    .Include(s => s.DailySchedules)
                        .ThenInclude(d => d.Assignments)
                            .ThenInclude(a => a.Member)
                    .Include(s => s.DailySchedules)
                        .ThenInclude(d => d.Assignments)
                            .ThenInclude(a => a.DutyType)
                    .AsNoTracking()  // Prevent tracking issues
                    .FirstOrDefaultAsync(s => s.Id == scheduleId);

                if (_selectedSchedule != null)
                {
                    // Update checkboxes using LINQ
                    _schedulesGrid.Rows.Cast<DataGridViewRow>()
                        .Where(gridRow => gridRow.Cells["Selected"]?.Value != null)
                        .ToList()
                        .ForEach(gridRow => gridRow.Cells["Selected"].Value = (gridRow.Index == row.Index));
                }

                    _btnLoad.Enabled = _selectedSchedule != null;
                    _btnDelete.Enabled = _selectedSchedule != null;
                }
            }
            else
            {
                _selectedSchedule = null;
                _btnLoad.Enabled = false;
                _btnDelete.Enabled = false;
                // Clear checkbox when no selection using LINQ
                _schedulesGrid.Rows.Cast<DataGridViewRow>().ToList()
                    .ForEach(gridRow => gridRow.Cells["Selected"].Value = false);
            }
        }
        catch (Exception ex)
        {
            _selectedSchedule = null;
            _btnLoad.Enabled = false;
            _btnDelete.Enabled = false;
            MessageBox.Show($"Error selecting schedule: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_selectedSchedule == null) return;

        var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_selectedSchedule.Month);
        if (MessageBox.Show(
            string.Format(ConfirmDeleteScheduleFormat, monthName, _selectedSchedule.Year),
            ConfirmDeleteTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            try
            {
                var scheduleToDelete = await _context.GeneratedSchedules
                    .FirstOrDefaultAsync(s => s.Id == _selectedSchedule.Id);

                if (scheduleToDelete != null)
                {
                    _context.GeneratedSchedules.Remove(scheduleToDelete);
                    await _context.SaveChangesAsync();
                    
                    // Clear selection state
                    _selectedSchedule = null;
                    _btnLoad.Enabled = false;
                    _btnDelete.Enabled = false;
                    
                    // Reload the grid and auto-select the first row if one exists
                    await LoadSchedules();
                    if (_schedulesGrid.Rows.Count > 0)
                    {
                        _schedulesGrid.Rows[0].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting schedule: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void SchedulesGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var columnName = _schedulesGrid.Columns[e.ColumnIndex].Name;
        
        if (columnName != "ExportCSV" && columnName != "ExportPDF")
            return;

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
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting schedule: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
