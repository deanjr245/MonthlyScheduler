using System.Data;
using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Services;
using MonthlyScheduler.Models;
using MonthlyScheduler.Exceptions;
using MonthlyScheduler.UI;
using static MonthlyScheduler.Utilities.AppStringConstants;

namespace MonthlyScheduler;

public partial class Form1 : Form
{
    private readonly SchedulerDbContext _context;
    private ComboBox monthSelect = null!;
    private ComboBox yearSelect = null!;
    private Button btnUpload = null!;
    private Button btnGenerateSchedule = null!;
    private Button btnViewSavedSchedules = null!;
    private Button btnViewMembers = null!;
    private Button btnExportMembers = null!;
    private Button btnAddMember = null!;
    private Button btnManageDutyTypes = null!;
    private Button btnManageFooterText = null!;
    private DataGridView scheduleGrid = null!;
    private int NumberOfYearsToShow = 10;

    public Form1(SchedulerDbContext context)
    {
        _context = context;
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
        yearSelect = new ComboBox();
        btnUpload = new Button();
        btnGenerateSchedule = new Button();
        btnViewSavedSchedules = new Button();
        btnViewMembers = new Button();
        btnExportMembers = new Button();
        btnAddMember = new Button();
        btnManageDutyTypes = new Button();
        btnManageFooterText = new Button();
        scheduleGrid = new DataGridView();

        // Configure month selection
        monthSelect.DropDownStyle = ComboBoxStyle.DropDownList;
        monthSelect.Dock = DockStyle.Fill;
        monthSelect.Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 1);
        monthSelect.Items.AddRange(System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[..^1]); // Exclude empty 13th month
        monthSelect.SelectedIndex = DateTime.Now.Month % 12; // Next month (wraps December to January)

        // Configure year selection
        yearSelect.DropDownStyle = ComboBoxStyle.DropDownList;
        yearSelect.Dock = DockStyle.Fill;
        yearSelect.Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 1);
        var startYear = DateTime.Now.Year - 1;
        for (int i = 0; i <= NumberOfYearsToShow; i++)
        {
            yearSelect.Items.Add(startYear + i);
        }
        yearSelect.SelectedItem = DateTime.Now.Year;

        // Configure buttons
        btnUpload.Text = ButtonUploadText;
        btnUpload.Dock = DockStyle.Fill;
        btnUpload.Click += BtnUpload_Click;
        btnUpload.ApplySecondaryStyle();

        btnGenerateSchedule.Text = ButtonGenerateText;
        btnGenerateSchedule.Dock = DockStyle.Fill;
        btnGenerateSchedule.Click += BtnGenerateSchedule_Click;
        btnGenerateSchedule.ApplyModernStyle();

        btnViewSavedSchedules.Text = ButtonViewSchedulesText;
        btnViewSavedSchedules.Dock = DockStyle.Fill;
        btnViewSavedSchedules.Click += BtnViewSavedSchedules_Click;
        btnViewSavedSchedules.ApplyModernStyle();

        btnExportMembers.Text = ButtonExportMembersText;
        btnExportMembers.Dock = DockStyle.Fill;
        btnExportMembers.Click += BtnExportMembers_Click;
        btnExportMembers.ApplyModernStyle();

        btnViewMembers.Text = ButtonViewMembersText;
        btnViewMembers.Dock = DockStyle.Fill;
        btnViewMembers.Click += BtnViewMembers_Click;
        btnViewMembers.ApplyModernStyle();

        btnAddMember.Text = ButtonAddMemberText;
        btnAddMember.Dock = DockStyle.Fill;
        btnAddMember.Click += BtnAddMember_Click;
        btnAddMember.ApplyModernStyle();

        btnManageDutyTypes.Text = ButtonManageDutyTypesText;
        btnManageDutyTypes.Dock = DockStyle.Fill;
        btnManageDutyTypes.Click += BtnManageDutyTypes_Click;
        btnManageDutyTypes.ApplyModernStyle();

        btnManageFooterText.Text = ButtonManageFooterTextText;
        btnManageFooterText.Dock = DockStyle.Fill;
        btnManageFooterText.Click += BtnManageFooterText_Click;
        btnManageFooterText.ApplyModernStyle();

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
        Text = FormTitleMainScheduler;
        Size = new Size(1024, 768);
        WindowState = FormWindowState.Maximized;

        // Create main table layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(20),
            BackColor = AppStyling.DarkBackground
        };

        // Left panel setup
        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 13,
            Padding = new Padding(15),
            Width = 220,
            BackColor = AppStyling.DarkBackground
        };
        // Set up all row styles
        leftPanel.RowStyles.Clear();
        
        // Row 0: Logo (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        
        // Rows 1-3: Controls (AutoSize)
        Enumerable.Range(0, 3).ToList().ForEach(_ => leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)));
        
        // Row 4: Spacer between Schedule and Member buttons (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        
        // Rows 5-7: Member controls (AutoSize)
        Enumerable.Range(0, 3).ToList().ForEach(_ => leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)));
        
        // Row 8: Spacer between Member and Duty Type buttons (Absolute)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        
        // Rows 9-10: Duty Type and Footer Text buttons (AutoSize)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        
        // Row 11: Spacer (Percent - fills remaining space)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        
        // Row 12: Upload button (AutoSize)
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));


        // Logo panel
        var logoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 20),
            BackColor = AppStyling.DarkBackground
        };

        var logoPictureBox = new PictureBox
        {
            Image = Image.FromFile("../../../Assets/RiceRoadLogo.png"),
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            BackColor = AppStyling.LightBackground
        };
        logoPanel.Controls.Add(logoPictureBox);

        // Month/Year selection panel
        var monthYearPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0, 0, 0, 20),
            BackColor = AppStyling.DarkBackground,
            AutoSize = true
        };

        monthYearPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Month label
        monthYearPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Month select
        monthYearPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Year label
        monthYearPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Year select

        var monthLabel = new Label { Text = LabelSelectMonth, AutoSize = true, Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 2), ForeColor = AppStyling.LightText, Margin = new Padding(0, 0, 0, 5) };
        var yearLabel = new Label { Text = LabelSelectYear, AutoSize = true, Font = new Font(AppStyling.Font.FontFamily, AppStyling.Font.Size + 2), ForeColor = AppStyling.LightText, Margin = new Padding(0, 10, 0, 5) };

        monthYearPanel.Controls.Add(monthLabel, 0, 0);
        monthYearPanel.Controls.Add(monthSelect, 0, 1);
        monthYearPanel.Controls.Add(yearLabel, 0, 2);
        monthYearPanel.Controls.Add(yearSelect, 0, 3);

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
        leftPanel.Controls.Add(btnManageFooterText, 0, 10);

        // Add spacer panel that fills remaining space
        var spacerPanel = new Panel { Dock = DockStyle.Fill };
        leftPanel.Controls.Add(spacerPanel, 0, 11);
        
        // Add upload button at the very bottom
        leftPanel.Controls.Add(btnUpload, 0, 12);

        // Right panel for schedule title and grid
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = AppStyling.DarkBackground,
            Padding = new Padding(0),
            Margin = new Padding(0),
            ColumnCount = 1,
            RowCount = 2,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };

        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Title
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid

        // Configure control properties
        scheduleGrid.Margin = new Padding(0);

        // Add grid to right panel
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
        const string CsvFilter = "Excel Files|*.csv|All Files|*.*";
        const string SelectFileTitle = "Select People Data File";
        const string ImportCompleteTitle = "Import Complete";
        const string ErrorTitle = "Error";
        const string ImportErrorMessage = "Error importing data: {0}";
        const string WarningTitle = "Warning";
        const string WarningMessage = "Only use this if you know what you're doing! Will override database!";
        
        // Show warning confirmation dialog
        var confirmResult = MessageBox.Show(
            WarningMessage,
            WarningTitle,
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning);

        // If user cancels, exit without proceeding
        if (confirmResult != DialogResult.OK)
        {
            return;
        }

        using OpenFileDialog openFileDialog = new()
        {
            Filter = CsvFilter,
            Title = SelectFileTitle
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var importService = new ExcelImportService(_context);
                try
                {
                    await importService.ImportMembersFromExcel(openFileDialog.FileName);
                }
                catch (ImportResultException ex)
                {
                    MessageBox.Show(ex.Message, ImportCompleteTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ImportErrorMessage, ex.Message), ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnGenerateSchedule_Click(object? sender, EventArgs e)
    {
        try
        {
            var selectedMonth = monthSelect.SelectedIndex + 1; // Adding 1 because SelectedIndex is 0-based
            var selectedYear = (int)yearSelect.SelectedItem!;

            var scheduleService = new ScheduleService(_context);
            _ = await scheduleService.GenerateMonthlySchedule(selectedYear, selectedMonth);

            await ConfigureGrid(selectedYear, selectedMonth, _context);

            MessageBox.Show(ScheduleGeneratedSuccess, SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorGeneratingScheduleFormat, ex.Message), ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearAndSetupGrid(bool forMembers = false)
    {
        // Clear any existing title labels from the right panel
        var mainLayout = (TableLayoutPanel)Controls[0];
        var rightPanel = (TableLayoutPanel)mainLayout.Controls[1];
        
        // Remove all labels from position (0, 0) in the right panel
        var controlsToRemove = rightPanel.Controls.Cast<Control>()
            .Where(c => c is Label && rightPanel.GetRow(c) == 0)
            .ToList();
        
        foreach (var control in controlsToRemove)
        {
            rightPanel.Controls.Remove(control);
            control.Dispose();
        }

        scheduleGrid.SuspendLayout();
        try
        {
            scheduleGrid.DataSource = null;
            scheduleGrid.Columns.Clear();
            scheduleGrid.ApplyModernStyle();

            // Remove selection highlighting but enable hover effect
            scheduleGrid.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
            scheduleGrid.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
            
            // Set hover style for all rows
            scheduleGrid.RowsDefaultCellStyle.BackColor = Color.White;
            scheduleGrid.RowsDefaultCellStyle.ForeColor = AppStyling.DarkText;

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
            // Load all data asynchronously
            var members = _context.Members
                .Include(m => m.AvailableDuties)
                .OrderBy(m => m.ExcludeFromScheduling)
                .ThenBy(m => m.LastName)
                .ThenBy(m => m.FirstName);

            var duties = _context.DutyTypes
                .OrderBy(dt => dt.Category)
                .ThenBy(dt => dt.Name);

            // Use DataTable instead of dynamic types - much faster
            var dataTable = new DataTable();
            dataTable.Columns.Add(ColumnLastNameText, typeof(string));
            dataTable.Columns.Add(ColumnFirstNameText, typeof(string));
            dataTable.Columns.Add(ColumnFormReceivedText, typeof(string));
            
            foreach (var duty in duties)
            {
                dataTable.Columns.Add(duty.Name, typeof(string));
            }
            
            dataTable.Columns.Add(ColumnExcludedText, typeof(string));

            // Pre-build a lookup for member duties for O(1) access
            var memberDutyLookup = members.ToDictionary(
                m => m.Id,
                m => new HashSet<int>(m.AvailableDuties.Select(d => d.DutyTypeId))
            );

            // Populate rows - much faster than reflection
            dataTable.BeginLoadData(); // Disable constraints and notifications during load
            
            // Process members using optimized approach
            foreach(var member in members)
            {
                var row = dataTable.NewRow();
                row[ColumnLastNameText] = member.LastName;
                row[ColumnFirstNameText] = member.FirstName;
                row[ColumnFormReceivedText] = member.HasSubmittedForm ? YesText : NoText;

                var dutyIds = memberDutyLookup[member.Id];
                foreach (var duty in duties)
                {
                    row[duty.Name] = dutyIds.Contains(duty.Id) ? YesText : string.Empty;
                }
                
                row[ColumnExcludedText] = member.ExcludeFromScheduling ? YesText : NoText;
                dataTable.Rows.Add(row);
            };
            
            dataTable.EndLoadData();

            ClearAndSetupGrid(forMembers: true);

            scheduleGrid.SuspendLayout();
            scheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            
            try
            {
                // Bind data first
                scheduleGrid.DataSource = dataTable;

                // Increase column header height to prevent text cutoff
                scheduleGrid.ColumnHeadersHeight = 65;
                scheduleGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                // Add Edit and Delete button columns at the FRONT by inserting
                var editBtn = new DataGridViewButtonColumn
                {
                    Name = ColumnEditText,
                    Text = ColumnEditText,
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
                    Name = ColumnDeleteText,
                    Text = ColumnDeleteText,
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
                if (scheduleGrid.Columns[ColumnLastNameText] is DataGridViewColumn lastNameCol)
                {
                    lastNameCol.Width = 120;
                    lastNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns[ColumnFirstNameText] is DataGridViewColumn firstNameCol)
                {
                    firstNameCol.Width = 120;
                    firstNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns[ColumnFormReceivedText] is DataGridViewColumn formCol)
                {
                    formCol.Width = 100;
                    formCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                if (scheduleGrid.Columns[ColumnExcludedText] is DataGridViewColumn excludedCol)
                {
                    excludedCol.Width = 80;
                    excludedCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }

                // All duty columns use Fill mode to distribute remaining space - using LINQ
                var fixedColumns = new HashSet<string> { ColumnEditText, ColumnDeleteText, ColumnLastNameText, ColumnFirstNameText, ColumnFormReceivedText, ColumnExcludedText };
                
                scheduleGrid.Columns.Cast<DataGridViewColumn>().ToList().ForEach(col =>
                {
                    if (!fixedColumns.Contains(col.Name))
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    col.DefaultCellStyle.SelectionBackColor = scheduleGrid.DefaultCellStyle.BackColor;
                    col.DefaultCellStyle.SelectionForeColor = scheduleGrid.DefaultCellStyle.ForeColor;
                });
            }
            finally
            {
                scheduleGrid.ResumeLayout();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorRefreshingMembersFormat, ex.Message), ErrorTitle, 
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
            MessageBox.Show(string.Format(ErrorLoadingMembersFormat, ex.Message), ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAddMember_Click(object? sender, EventArgs e)
    {
        try
        {
            var memberForm = new Forms.MemberForm(_context);
            
            if (memberForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh the member view if it's currently displayed
                if (scheduleGrid.DataSource is DataTable)
                {
                    await GetMembersView();
                }
                MessageBox.Show(MemberAddedSuccess, SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorAddingMemberFormat, ex.Message), ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ScheduleGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        try
        {
            // Ignore header clicks
            if (e.RowIndex < 0)
                return;

            // Only handle clicks in member view - check if Edit/Delete columns exist
            if (!scheduleGrid.Columns.Contains(ColumnEditText) || !scheduleGrid.Columns.Contains(ColumnDeleteText))
                return;

            // Check if First Name and Last Name columns exist (member view)
            if (!scheduleGrid.Columns.Contains(ColumnFirstNameText) || !scheduleGrid.Columns.Contains(ColumnLastNameText))
                return;

            var row = scheduleGrid.Rows[e.RowIndex];
            var firstName = row.Cells[ColumnFirstNameText]?.Value?.ToString() ?? string.Empty;
            var lastName = row.Cells[ColumnLastNameText]?.Value?.ToString() ?? string.Empty;

            // Find the member in the database
            var member = await _context.Members
                .Include(m => m.AvailableDuties)
                .FirstOrDefaultAsync(m => m.FirstName == firstName && m.LastName == lastName);

            if (member == null)
            {
                MessageBox.Show(MemberNotFoundError, ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle Edit button click
            if (e.ColumnIndex >= 0 && scheduleGrid.Columns[e.ColumnIndex].Name == ColumnEditText)
            {
                var memberForm = new Forms.MemberForm(_context, member);
                if (memberForm.ShowDialog() == DialogResult.OK)
                {
                    await GetMembersView();
                }
            }
            // Handle Delete button click
            else if (e.ColumnIndex >= 0 && scheduleGrid.Columns[e.ColumnIndex].Name == ColumnDeleteText)
            {
                if (MessageBox.Show(
                    string.Format(ConfirmDeleteMemberFormat, member.FullName), 
                    ConfirmDeleteTitle, 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        _context.Members.Remove(member);
                        await _context.SaveChangesAsync();
                        await GetMembersView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(ErrorDeletingMemberFormat, ex.Message), ErrorTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorLoadingMembersFormat, ex.Message), ErrorTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var exportService = new MemberExportService();
                
                await exportService.ExportMembersToCSV(_context, saveFileDialog.FileName);
                MessageBox.Show(MembersExportedSuccess, ExportCompleteTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ErrorExportingMembersFormat, ex.Message), ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnManageDutyTypes_Click(object? sender, EventArgs e)
    {
        try
        {
            var manageDutyTypesForm = new Forms.ManageDutyTypesForm(_context);
            manageDutyTypesForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorManagingDutyTypesFormat, ex.Message), ErrorTitle, 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnManageFooterText_Click(object? sender, EventArgs e)
    {
        try
        {
            var manageFooterTextForm = new Forms.ManageFooterTextForm(_context);
            manageFooterTextForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error managing footer text: {ex.Message}", ErrorTitle,
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
            var serviceText = gridRow.Cells[ColumnServiceText].Value?.ToString();
            var dutyTypeName = gridRow.Cells[ColumnDutyText].Value?.ToString();

            if (string.IsNullOrEmpty(serviceText) || string.IsNullOrEmpty(dutyTypeName)) return;

            var dutyTypeToEdit = await _context.DutyTypes.FirstOrDefaultAsync(dt => dt.Name == dutyTypeName);
            if (dutyTypeToEdit == null) return;

            // Only allow editing manually scheduled duties
            if (!dutyTypeToEdit.ManuallyScheduled)
            {
                MessageBox.Show(CannotEditAutoScheduledDuty,
                    CannotEditAssignmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            var dutyType = await _context.DutyTypes.FirstOrDefaultAsync(dt => dt.Name == dutyTypeName);
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
                    MessageBox.Show(LastSundayOnlyOccursMessage,
                        CannotEditAssignmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Parse column date with current year and month from the schedule
                var columnName = scheduleGrid.Columns[e.ColumnIndex].Name;
                var year = (int)yearSelect.SelectedItem!;
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
                    MessageBox.Show(LastSundayOnlyAssignMessage,
                        CannotEditAssignmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            var assignmentForm = new Forms.AssignmentEditForm(_context, dutyType, serviceType);
            if (assignmentForm.ShowDialog() == DialogResult.OK && assignmentForm.SelectedMember != null)
            {
                var columnName = scheduleGrid.Columns[e.ColumnIndex].Name;
                var year = (int)yearSelect.SelectedItem!;
                var selectedDate = DateTime.Parse($"{columnName} {year}");

                // Update the grid cell
                scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = assignmentForm.SelectedMember.FullName;

                // Save changes to database
                var schedule = await _context.GeneratedSchedules
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
                    await _context.GeneratedSchedules.AddAsync(schedule);
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

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorUpdatingAssignmentFormat, ex.Message), ErrorTitle, 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnViewSavedSchedules_Click(object? sender, EventArgs e)
    {
        try
        {
            var savedSchedulesForm = new Forms.SavedSchedulesForm(_context);
            if (savedSchedulesForm.ShowDialog() == DialogResult.OK && savedSchedulesForm.SelectedSchedule != null)
            {
                var schedule = savedSchedulesForm.SelectedSchedule;

                // Set the month/year selectors to match the loaded schedule
                monthSelect.SelectedIndex = schedule.Month - 1;
                yearSelect.SelectedItem = schedule.Year;

                await ConfigureGrid(schedule.Year, schedule.Month, _context);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(ErrorViewingSavedSchedulesFormat, ex.Message), ErrorTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task ConfigureGrid(int year, int month, SchedulerDbContext context)
    {
        // Clear and setup grids for schedule view
        ClearAndSetupGrid(forMembers: false);

        var lblScheduleTitle = new Label
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 0, 0, 10),
            Margin = new Padding(1, 1, 1, 0),
            Font = new Font(AppStyling.Font.FontFamily, 16, FontStyle.Bold)
        };
        
        // Find the right panel and add the label to it
        var mainLayout = (TableLayoutPanel)Controls[0];
        var rightPanel = (TableLayoutPanel)mainLayout.Controls[1];
        rightPanel.Controls.Add(lblScheduleTitle, 0, 0);

        // Update titles
        lblScheduleTitle.Text = $"{monthSelect.Text} {year}";
        lblScheduleTitle.ForeColor = AppStyling.LightText;
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

            // Configure fixed-width columns
            var fixedWidthColumns = new Dictionary<string, int>
            {
                [ColumnServiceText] = 180,
                [ColumnDutyText] = 200
            };

            fixedWidthColumns
                .Where(kvp => scheduleGrid.Columns[kvp.Key] is DataGridViewColumn)
                .ToList()
                .ForEach(kvp =>
                {
                    var col = scheduleGrid.Columns[kvp.Key]!;
                    col.Width = kvp.Value;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                });

            // Configure fill columns using LINQ
            scheduleGrid.Columns.Cast<DataGridViewColumn>()
                .Where(col => !fixedWidthColumns.ContainsKey(col.Name))
                .ToList()
                .ForEach(col => col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill);
        }
        finally
        {
            scheduleGrid.ResumeLayout();
        }
    }
}
