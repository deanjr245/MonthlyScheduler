using System.Data;
using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Services;
using MonthlyScheduler.Models;
using MonthlyScheduler.Utilities;
using MonthlyScheduler.Exceptions;
using MonthlyScheduler.UI;

namespace MonthlyScheduler;

public partial class Form1 : Form
{
    private ComboBox monthSelect = null!;
    private NumericUpDown yearSelect = null!;
    private Button btnUpload = null!;
    private Button btnGenerateSchedule = null!;
    private Button btnViewSavedSchedules = null!;
    private Button btnViewMembers = null!;
    private Button btnExportMembers = null!;
    private Button btnAddMember = null!;
    private Button btnManageDutyTypes = null!;
    private DataGridView scheduleGrid = null!;
    private Label lblScheduleTitle = null!;

    public Form1()
    {
        InitializeComponent();
        InitializeControls();
        SetupLayout();
        scheduleGrid.CellClick += ScheduleGrid_CellClick;
        scheduleGrid.CellDoubleClick += ScheduleGrid_CellDoubleClick;
    }

    private void InitializeControls()
    {
        // Initialize all controls
        monthSelect = new ComboBox();
        yearSelect = new NumericUpDown();
        btnUpload = new Button();
        btnGenerateSchedule = new Button();
        btnViewSavedSchedules = new Button();
        btnViewMembers = new Button();
        btnExportMembers = new Button();
        btnAddMember = new Button();
        btnManageDutyTypes = new Button();
        scheduleGrid = new DataGridView();

        // Configure month selection
        monthSelect.DropDownStyle = ComboBoxStyle.DropDownList;
        monthSelect.Width = 180;
        monthSelect.Items.AddRange(System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[..^1]); // Exclude empty 13th month
        monthSelect.SelectedIndex = DateTime.Now.Month - 1;


        // Configure year selection
        yearSelect.Width = 180;
        yearSelect.Minimum = 2024;
        yearSelect.Maximum = 2030;
        yearSelect.Value = DateTime.Now.Year;


        // Configure buttons
        btnUpload.Text = "Upload Spreadsheet";
        btnUpload.Width = 180;
        btnUpload.Click += BtnUpload_Click;
        btnUpload.ApplySecondaryStyle();

        btnGenerateSchedule.Text = "Generate Schedule";
        btnGenerateSchedule.Width = 180;
        btnGenerateSchedule.Click += BtnGenerateSchedule_Click;
        btnGenerateSchedule.ApplyModernStyle();

        btnViewSavedSchedules.Text = "View Saved Schedules";
        btnViewSavedSchedules.Width = 180;
        btnViewSavedSchedules.Click += BtnViewSavedSchedules_Click;
        btnViewSavedSchedules.ApplyModernStyle();

        btnExportMembers.Text = "Export Members - CSV";
        btnExportMembers.Width = 180;
        btnExportMembers.Click += BtnExportMembers_Click;
        btnExportMembers.ApplyModernStyle();

        btnViewMembers.Text = "View Members";
        btnViewMembers.Width = 180;
        btnViewMembers.Click += BtnViewMembers_Click;
        btnViewMembers.ApplyModernStyle();

        btnAddMember.Text = "Add New Member";
        btnAddMember.Width = 180;
        btnAddMember.Click += BtnAddMember_Click;
        btnAddMember.ApplyModernStyle();

        btnManageDutyTypes.Text = "Manage Duty Types";
        btnManageDutyTypes.Width = 180;
        btnManageDutyTypes.Click += BtnManageDutyTypes_Click;
        btnManageDutyTypes.ApplyModernStyle();

        // Configure schedule title
        lblScheduleTitle = new Label
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 0, 0, 10),
            Visible = false
        };
        lblScheduleTitle.ApplyModernStyle();
        lblScheduleTitle.Font = new Font(lblScheduleTitle.Font.FontFamily, 16, FontStyle.Bold);

        // Configure main grid
        scheduleGrid.Dock = DockStyle.Fill;
        scheduleGrid.ApplyModernStyle();
        scheduleGrid.ScrollBars = ScrollBars.Both;
        scheduleGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        scheduleGrid.RowTemplate.Height = 25;
        scheduleGrid.BorderStyle = BorderStyle.None;
        scheduleGrid.DefaultCellStyle.Padding = new Padding(0);

        // Setup hover effect once
        var hoverColor = Color.FromArgb(240, 245, 255);
        scheduleGrid.RowTemplate.DefaultCellStyle.BackColor = Color.White;
        scheduleGrid.CellMouseEnter += (s, e) => {
            if (e.RowIndex >= 0)
            {
                scheduleGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = hoverColor;
            }
        };
        scheduleGrid.CellMouseLeave += (s, e) => {
            if (e.RowIndex >= 0)
            {
                scheduleGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        };
    }

    private void SetupLayout()
    {
        // Configure form
        Text = "Monthly Scheduler";
        Size = new Size(1024, 768);
        WindowState = FormWindowState.Maximized;

        // Set form background color


        // Create main table layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(20),
            BackColor = AppStyling.WindowBackground
        };

        // Left panel setup
        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 12,  // Reduced from 14 after removing export buttons
            Padding = new Padding(15),
            Width = 220,
            BackColor = AppStyling.WindowBackground
        };
        // Set up all row styles
        leftPanel.RowStyles.Clear();
        
        // Row 0: Logo (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        
        // Rows 1-3: Controls (AutoSize)
        for (int i = 1; i <= 3; i++)
        {
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        
        // Row 4: Spacer between Schedule and Member buttons (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        
        // Rows 5-7: Member controls (AutoSize)
        for (int i = 5; i <= 7; i++)
        {
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        
        // Row 8: Spacer between Member and Duty Type buttons (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        
        // Row 9: Duty Type button (AutoSize)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        
        // Row 10: Spacer (Percent - fills remaining space)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        
        // Row 11: Upload button (AutoSize)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));


        // Logo panel
        var logoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 20),
            BackColor = AppStyling.WindowBackground
        };

        var logoPictureBox = new PictureBox
        {
            Image = Image.FromFile("../../../Assets/RiceRoadLogo.png"),
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill
        };
        logoPanel.Controls.Add(logoPictureBox);

        // Month/Year selection panel
        var monthYearPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20),
            BackColor = AppStyling.WindowBackground
        };

        var monthLabel = new Label { Text = "Select Month:", AutoSize = true };
        monthLabel.ApplyModernStyle();
        var yearLabel = new Label { Text = "Select Year:", AutoSize = true };
        yearLabel.ApplyModernStyle();

        monthYearPanel.Controls.AddRange(
        [
            monthLabel,
            monthSelect,
            yearLabel,
            yearSelect
        ]);

        // Set row styles for the logo
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Height for logo

        // Add controls to left panel with specific positioning
        leftPanel.Controls.Add(logoPanel, 0, 0);
        leftPanel.Controls.Add(monthYearPanel, 0, 1);
        leftPanel.Controls.Add(btnGenerateSchedule, 0, 2);
        leftPanel.Controls.Add(btnViewSavedSchedules, 0, 3);
        // Row 4: spacing
        leftPanel.Controls.Add(btnViewMembers, 0, 5);
        leftPanel.Controls.Add(btnExportMembers, 0, 6);
        leftPanel.Controls.Add(btnAddMember, 0, 7);
        // Row 8: spacing
        leftPanel.Controls.Add(btnManageDutyTypes, 0, 9);

        // Add spacer panel that fills remaining space
        var spacerPanel = new Panel { Dock = DockStyle.Fill };
        leftPanel.Controls.Add(spacerPanel, 0, 10);
        
        // Add upload button at the very bottom
        leftPanel.Controls.Add(btnUpload, 0, 11);

        // Right panel for schedule title and grid
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = AppStyling.WindowBackground,
            Padding = new Padding(0),
            Margin = new Padding(0),
            ColumnCount = 1,
            RowCount = 2,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };

        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Title
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid

        // Configure control properties
        lblScheduleTitle.Margin = new Padding(1, 1, 1, 0);
        scheduleGrid.Margin = new Padding(0);

        // Add controls
        rightPanel.Controls.Add(lblScheduleTitle, 0, 0);
        rightPanel.Controls.Add(scheduleGrid, 0, 1);

        // Configure and add panels to main layout with fixed left panel width
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        // Add main layout to form
        Controls.Add(mainLayout);
    }

    private async void BtnUpload_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new()
        {
            Filter = "Excel Files|*.csv|All Files|*.*",
            Title = "Select People Data File"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                using var context = new SchedulerDbContext();
                var importService = new ExcelImportService(context);
                try
                {
                    await importService.ImportMembersFromExcel(openFileDialog.FileName);
                }
                catch (ImportResultException ex)
                {
                    MessageBox.Show(ex.Message, "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnGenerateSchedule_Click(object? sender, EventArgs e)
    {
        try
        {
            var selectedMonth = monthSelect.SelectedIndex + 1; // Adding 1 because SelectedIndex is 0-based
            var selectedYear = (int)yearSelect.Value;

            using var context = new SchedulerDbContext();
            var scheduleService = new ScheduleService(context);
            var (schedule, storedSchedule) = await scheduleService.GenerateMonthlySchedule(selectedYear, selectedMonth);

            await ConfigureGrid(selectedYear, selectedMonth, context);

            MessageBox.Show("Schedule generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearAndSetupGrid(bool forMembers = false)
    {
        scheduleGrid.SuspendLayout();
        try
        {
            scheduleGrid.DataSource = null;
            scheduleGrid.Columns.Clear();
            scheduleGrid.ApplyModernStyle();

            // Hide title for member view, show for schedule view
            lblScheduleTitle.Visible = !forMembers;

            // Remove selection highlighting but enable hover effect
            scheduleGrid.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
            scheduleGrid.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
            
            // Set hover style for all rows
            scheduleGrid.RowsDefaultCellStyle.BackColor = Color.White;
            scheduleGrid.RowsDefaultCellStyle.ForeColor = AppStyling.Text;

            if (forMembers)
            {
                // Set grid options specific to member view
                scheduleGrid.AllowUserToAddRows = false;
                scheduleGrid.AllowUserToDeleteRows = false;
                scheduleGrid.ReadOnly = true;
                scheduleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                scheduleGrid.MultiSelect = false;
            }
            else
            {
                // Reset to schedule view settings
                scheduleGrid.AllowUserToAddRows = false;
                scheduleGrid.AllowUserToDeleteRows = false;
                scheduleGrid.ReadOnly = false;
                scheduleGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                scheduleGrid.MultiSelect = false;
            }
        }
        finally
        {
            scheduleGrid.ResumeLayout();
        }
    }

    private async Task GetMembersView()
    {
        try
        {
            using var context = new SchedulerDbContext();

            // Load all data in parallel
            var members = context.Members
                .Include(m => m.AvailableDuties)
                .OrderBy(m => m.ExcludeFromScheduling)
                .ThenBy(m => m.LastName)
                .ThenBy(m => m.FirstName);

            var duties = context.DutyTypes
                .OrderBy(dt => dt.Category)
                .ThenBy(dt => dt.Name);

            // Use DataTable instead of dynamic types - much faster
            var dataTable = new DataTable();
            dataTable.Columns.Add("Last Name", typeof(string));
            dataTable.Columns.Add("First Name", typeof(string));
            dataTable.Columns.Add("Form Received", typeof(string));
            
            foreach (var duty in duties)
            {
                dataTable.Columns.Add(duty.Name, typeof(string));
            }
            
            dataTable.Columns.Add("Excluded", typeof(string));

            // Pre-build a lookup for member duties for O(1) access
            var memberDutyLookup = members.ToDictionary(
                m => m.Id,
                m => new HashSet<int>(m.AvailableDuties.Select(d => d.DutyTypeId))
            );

            // Populate rows - much faster than reflection
            dataTable.BeginLoadData(); // Disable constraints and notifications during load
            foreach (var member in members)
            {
                var row = dataTable.NewRow();
                row["Last Name"] = member.LastName;
                row["First Name"] = member.FirstName;
                row["Form Received"] = member.HasSubmittedForm ? "Yes" : "No";
                
                var dutyIds = memberDutyLookup[member.Id];
                foreach (var duty in duties)
                {
                    row[duty.Name] = dutyIds.Contains(duty.Id) ? "Yes" : "";
                }
                
                row["Excluded"] = member.ExcludeFromScheduling ? "Yes" : "No";
                dataTable.Rows.Add(row);
            }
            dataTable.EndLoadData();

            ClearAndSetupGrid(forMembers: true);

            scheduleGrid.SuspendLayout();
            scheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            try
            {
                // Bind data first
                scheduleGrid.DataSource = dataTable;

                // Add Edit and Delete button columns at the FRONT by inserting
                var editBtn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Standard,
                    Width = 80,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                editBtn.DefaultCellStyle.BackColor = AppStyling.Info;
                editBtn.DefaultCellStyle.ForeColor = Color.White;
                editBtn.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
                editBtn.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
                scheduleGrid.Columns.Insert(0, editBtn);

                var deleteBtn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Standard,
                    Width = 80,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                deleteBtn.DefaultCellStyle.BackColor = AppStyling.Danger;
                deleteBtn.DefaultCellStyle.ForeColor = Color.White;
                deleteBtn.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
                deleteBtn.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
                scheduleGrid.Columns.Insert(1, deleteBtn);

                // Set fixed widths for name/form/excluded columns
                if (scheduleGrid.Columns["Last Name"] is DataGridViewColumn lastNameCol)
                {
                    lastNameCol.Width = 120;
                    lastNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns["First Name"] is DataGridViewColumn firstNameCol)
                {
                    firstNameCol.Width = 120;
                    firstNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns["Form Received"] is DataGridViewColumn formCol)
                {
                    formCol.Width = 100;
                    formCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns["Excluded"] is DataGridViewColumn excludedCol)
                {
                    excludedCol.Width = 80;
                    excludedCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }

                // All duty columns use Fill mode to distribute remaining space
                foreach (DataGridViewColumn col in scheduleGrid.Columns)
                {
                    if (col.Name != "Edit" && col.Name != "Delete" && 
                        col.Name != "Last Name" && col.Name != "First Name" && 
                        col.Name != "Form Received" && col.Name != "Excluded")
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    col.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
                    col.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
                }
            }
            finally
            {
                scheduleGrid.ResumeLayout();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refreshing members view: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnViewMembers_Click(object? sender, EventArgs e)
    {
        try
        {
            await GetMembersView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAddMember_Click(object? sender, EventArgs e)
    {
        try
        {
            using var context = new SchedulerDbContext();
            var memberForm = new Forms.MemberForm(context);
            
            if (memberForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh the member view if it's currently displayed
                if (scheduleGrid.DataSource is DataTable)
                {
                    await GetMembersView();
                }
                MessageBox.Show("Member added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding member: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ScheduleGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        // Ignore header clicks
        if (e.RowIndex < 0)
            return;

        // Only handle clicks in member view - check if Edit/Delete columns exist
        if (!scheduleGrid.Columns.Contains("Edit") || !scheduleGrid.Columns.Contains("Delete"))
            return;

        // Check if First Name and Last Name columns exist (member view)
        if (!scheduleGrid.Columns.Contains("First Name") || !scheduleGrid.Columns.Contains("Last Name"))
            return;

        var row = scheduleGrid.Rows[e.RowIndex];
        var firstName = row.Cells["First Name"]?.Value?.ToString() ?? string.Empty;
        var lastName = row.Cells["Last Name"]?.Value?.ToString() ?? string.Empty;

        using var context = new SchedulerDbContext();

        // Find the member in the database
        var member = await context.Members
            .Include(m => m.AvailableDuties)
            .FirstOrDefaultAsync(m => m.FirstName == firstName && m.LastName == lastName);

        if (member == null)
        {
            MessageBox.Show("Member not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Handle Edit button click
        if (e.ColumnIndex >= 0 && scheduleGrid.Columns[e.ColumnIndex].Name == "Edit")
        {
            var memberForm = new Forms.MemberForm(context, member);
            if (memberForm.ShowDialog() == DialogResult.OK)
            {
                await GetMembersView();
            }
        }
        // Handle Delete button click
        else if (e.ColumnIndex >= 0 && scheduleGrid.Columns[e.ColumnIndex].Name == "Delete")
        {
            if (MessageBox.Show(
                $"Are you sure you want to delete {member.FullName}?", 
                "Confirm Delete", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                context.Members.Remove(member);
                await context.SaveChangesAsync();
                await GetMembersView();
            }
        }
    }

    private async void BtnExportMembers_Click(object? sender, EventArgs e)
    {
        using SaveFileDialog saveFileDialog = new()
        {
            Filter = "CSV Files|*.csv",
            Title = "Export Members to CSV",
            FileName = $"Members_{DateTime.Now:yyyyMMdd}.csv"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                using var context = new SchedulerDbContext();
                var exportService = new MemberExportService();
                
                await exportService.ExportMembersToCSV(context, saveFileDialog.FileName);
                MessageBox.Show("Members exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnManageDutyTypes_Click(object? sender, EventArgs e)
    {
        try
        {
            using var context = new SchedulerDbContext();
            var manageDutyTypesForm = new Forms.ManageDutyTypesForm(context);
            manageDutyTypesForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error managing duty types: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ScheduleGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        try
        {
            // Ignore header clicks and clicks on non-assignment cells
            if (e.RowIndex < 0 || e.ColumnIndex <= 1) return;

            // Get the duty type first
            var gridRow = scheduleGrid.Rows[e.RowIndex];
            var serviceText = gridRow.Cells["Service"].Value?.ToString();
            var dutyTypeName = gridRow.Cells["Duty"].Value?.ToString();

            if (string.IsNullOrEmpty(serviceText) || string.IsNullOrEmpty(dutyTypeName)) return;

            using var dbContext = new SchedulerDbContext();
            var dutyTypeToEdit = await dbContext.DutyTypes.FirstOrDefaultAsync(dt => dt.Name == dutyTypeName);
            if (dutyTypeToEdit == null) return;

            // Only allow editing manually scheduled duties
            if (!dutyTypeToEdit.ManuallyScheduled)
            {
                MessageBox.Show("This duty is automatically scheduled. To manually assign it, first mark it as 'Include but don't schedule' in the duty type settings.",
                    "Cannot Edit Assignment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check if this is a text input assignment
            if (dutyTypeToEdit.ManualAssignmentType == Models.ManualAssignmentType.TextInput)
            {
                var currentValue = gridRow.Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;
                
                // Show simple input dialog
                var inputForm = new Form
                {
                    Text = $"Edit {dutyTypeName}",
                    Size = new Size(400, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var inputBox = new TextBox
                {
                    Text = currentValue,
                    Width = 350,
                    Location = new Point(20, 20)
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 60),
                    Width = 80
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 60),
                    Width = 80
                };

                inputForm.Controls.AddRange(new Control[] { inputBox, okButton, cancelButton });
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    gridRow.Cells[e.ColumnIndex].Value = inputBox.Text;
                    // Note: Text assignments don't save to database - they're for display only
                }
                return;
            }

            if (string.IsNullOrEmpty(serviceText) || string.IsNullOrEmpty(dutyTypeName)) return;

            using var context = new SchedulerDbContext();
            var dutyType = await context.DutyTypes.FirstOrDefaultAsync(dt => dt.Name == dutyTypeName);
            if (dutyType == null) return;

            var serviceType = serviceText.EndsWith("Morning") ? ServiceType.Sunday_AM :
                            serviceText.EndsWith("Evening") ? ServiceType.Sunday_PM :
                            ServiceType.Wednesday;

            // If this is a Last-Sunday-only duty, restrict editing to that date on Sunday PM
            bool isLastSundayOnly = (!string.IsNullOrWhiteSpace(dutyType.Name) && dutyType.Name.Equals("Monthly Song Service Leader", StringComparison.OrdinalIgnoreCase))
                                    || (!string.IsNullOrWhiteSpace(dutyType.Description) && dutyType.Description.Contains("[LastSundayOnly]", StringComparison.OrdinalIgnoreCase));
            if (isLastSundayOnly)
            {
                if (serviceType != ServiceType.Sunday_PM)
                {
                    MessageBox.Show("This duty only occurs on the last Sunday evening of the month.",
                        "Cannot Edit Assignment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Parse column date with current year and month from the schedule
                var columnName = scheduleGrid.Columns[e.ColumnIndex].Name;
                var year = (int)yearSelect.Value;
                var month = monthSelect.SelectedIndex + 1;
                var selectedDate = DateTime.Parse($"{columnName} {year}");
                
                // Calculate last Sunday of the month
                var firstDayOfNextMonth = new DateTime(year, month, 1).AddMonths(1);
                var lastSunday = firstDayOfNextMonth.AddDays(-1);
                while (lastSunday.DayOfWeek != DayOfWeek.Sunday)
                {
                    lastSunday = lastSunday.AddDays(-1);
                }

                if (selectedDate.Date != lastSunday.Date)
                {
                    MessageBox.Show("This duty can only be assigned on the last Sunday evening of the month.",
                        "Cannot Edit Assignment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            var assignmentForm = new Forms.AssignmentEditForm(context, dutyType, serviceType);
            if (assignmentForm.ShowDialog() == DialogResult.OK && assignmentForm.SelectedMember != null)
            {
                var columnName = scheduleGrid.Columns[e.ColumnIndex].Name;
                var year = (int)yearSelect.Value;
                var selectedDate = DateTime.Parse($"{columnName} {year}");

                // Update the grid cell
                scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = assignmentForm.SelectedMember.FullName;

                // Save changes to database
                var schedule = await context.GeneratedSchedules
                    .Include(s => s.DailySchedules)
                        .ThenInclude(d => d.Assignments)
                    .FirstOrDefaultAsync(s => 
                        s.Year == selectedDate.Year && 
                        s.Month == selectedDate.Month);

                if (schedule == null)
                {
                    // Create new schedule if none exists
                    schedule = new GeneratedSchedule
                    {
                        Year = selectedDate.Year,
                        Month = selectedDate.Month,
                        GeneratedDate = DateTime.Now
                    };
                    await context.GeneratedSchedules.AddAsync(schedule);
                }

                var dailySchedule = schedule.DailySchedules
                    .FirstOrDefault(d => d.Date.Date == selectedDate.Date) 
                    ?? new DailySchedule
                    {
                        Date = selectedDate,
                        DayOfWeek = selectedDate.DayOfWeek
                    };

                if (!schedule.DailySchedules.Contains(dailySchedule))
                {
                    schedule.DailySchedules.Add(dailySchedule);
                }

                // Remove any existing assignment for this duty/service
                var existingAssignment = dailySchedule.Assignments
                    .FirstOrDefault(a => 
                        a.DutyTypeId == dutyType.Id && 
                        a.ServiceType == serviceType);

                if (existingAssignment != null)
                {
                    dailySchedule.Assignments.Remove(existingAssignment);
                }

                // Add new assignment
                dailySchedule.Assignments.Add(new ScheduleAssignment
                {
                    Member = assignmentForm.SelectedMember,
                    DutyType = dutyType,
                    ServiceType = serviceType
                });

                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating assignment: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnViewSavedSchedules_Click(object? sender, EventArgs e)
    {
        try
        {
            using var context = new SchedulerDbContext();
            var savedSchedulesForm = new Forms.SavedSchedulesForm(context);
            if (savedSchedulesForm.ShowDialog() == DialogResult.OK && savedSchedulesForm.SelectedSchedule != null)
            {
                var schedule = savedSchedulesForm.SelectedSchedule;

                // Set the month/year selectors to match the loaded schedule
                monthSelect.SelectedIndex = schedule.Month - 1;
                yearSelect.Value = schedule.Year;

                await ConfigureGrid(schedule.Year, schedule.Month, context);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error viewing saved schedules: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task ConfigureGrid(int year, int month, SchedulerDbContext context)
    {
        // Clear and setup grids for schedule view
        ClearAndSetupGrid(forMembers: false);

        // Update titles
        lblScheduleTitle.Text = $"{monthSelect.Text} {year}";
        lblScheduleTitle.Visible = true;

        // Load schedule data
        var scheduleLoader = new ScheduleLoaderService(context);
        var scheduleData = await scheduleLoader.LoadScheduleData(year, month);

        // Suspend layout for performance
        scheduleGrid.SuspendLayout();
        try
        {
            // Set data source
            scheduleGrid.DataSource = scheduleData;

            // Configure columns
            scheduleGrid.RowTemplate.Height = 25;

            if (scheduleGrid.Columns["Service"] is DataGridViewColumn serviceCol)
            {
                serviceCol.Width = 180;
                serviceCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            if (scheduleGrid.Columns["Duty"] is DataGridViewColumn dutyCol)
            {
                dutyCol.Width = 200;
                dutyCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            foreach (DataGridViewColumn col in scheduleGrid.Columns)
            {
                if (col.Name != "Service" && col.Name != "Duty")
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }
        finally
        {
            scheduleGrid.ResumeLayout();
        }
    }
}
