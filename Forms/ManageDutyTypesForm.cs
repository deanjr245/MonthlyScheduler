using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;

namespace MonthlyScheduler.Forms;

public partial class ManageDutyTypesForm : Form
{
    private readonly SchedulerDbContext _context;
    private DataGridView dutyGrid = null!;
    private Button addButton = null!;
    private readonly List<DutyType> _dutyTypes = new();

    public ManageDutyTypesForm(SchedulerDbContext context)
    {
        _context = context;
        InitializeComponent();
        InitializeControls();
        SetupLayout();
        _ = LoadDutyTypes();
    }

    private void InitializeComponent()
    {
        Text = "Manage Duty Types";
        Size = new Size(1200, 750);
        StartPosition = FormStartPosition.CenterParent;
    }

    private void InitializeControls()
    {
        // Initialize grid
        dutyGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoGenerateColumns = false
        };
        dutyGrid.ApplyModernStyle();

        // Initialize add button
        addButton = new Button
        {
            Text = "Add New Duty Type",
            Dock = DockStyle.Top
        };
        addButton.Click += AddButton_Click;
        addButton.ApplyModernStyle();

        // Set up grid click handler
        dutyGrid.CellClick += DutyGrid_CellClick;
    }

    private void SetupLayout()
    {
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        addButton.Dock = DockStyle.None;
        buttonPanel.Controls.Add(addButton);

        // Add Order buttons
        var orderMorningButton = new Button
        {
            Text = "Order Morning Duties",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        orderMorningButton.Click += (s, e) => ShowDutyOrderForm(DutyCategory.Worship, ServiceType.Sunday_AM);
        orderMorningButton.ApplySecondaryStyle();
        buttonPanel.Controls.Add(orderMorningButton);

        var orderEveningButton = new Button
        {
            Text = "Order Evening Duties",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        orderEveningButton.Click += (s, e) => ShowDutyOrderForm(DutyCategory.Worship, ServiceType.Sunday_PM);
        orderEveningButton.ApplySecondaryStyle();
        buttonPanel.Controls.Add(orderEveningButton);

        var orderWednesdayButton = new Button
        {
            Text = "Order Wednesday Duties",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        orderWednesdayButton.Click += (s, e) => ShowDutyOrderForm(DutyCategory.Worship, ServiceType.Wednesday);
        orderWednesdayButton.ApplySecondaryStyle();
        buttonPanel.Controls.Add(orderWednesdayButton);

        // Add Audio/Visual ordering button
        var orderAVButton = new Button
        {
            Text = "Order A/V Duties",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        orderAVButton.Click += (s, e) => ShowDutyOrderForm(DutyCategory.AudioVisual, ServiceType.Sunday_AM);
        orderAVButton.ApplySecondaryStyle();
        buttonPanel.Controls.Add(orderAVButton);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        mainLayout.Controls.Add(buttonPanel, 0, 0);
        mainLayout.Controls.Add(dutyGrid, 0, 1);

        Controls.Add(mainLayout);
    }

    private async Task LoadDutyTypes()
    {
        try
        {
            _dutyTypes.Clear();
            _dutyTypes.AddRange(await _context.DutyTypes.OrderBy(dt => dt.Category).ThenBy(dt => dt.Name).ToListAsync());
            
            dutyGrid.DataSource = null;

            if (dutyGrid.Columns.Count == 0)
            {
                var columns = new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn
                    {
                        Name = "Name",
                        HeaderText = "Name",
                        DataPropertyName = "Name",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                        MinimumWidth = 100
                    },
                    new DataGridViewTextBoxColumn
                    {
                        Name = "Description",
                        HeaderText = "Description",
                        DataPropertyName = "Description",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                        MinimumWidth = 200
                    },
                    new DataGridViewTextBoxColumn
                    {
                        Name = "Category",
                        HeaderText = "Category",
                        DataPropertyName = "Category",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                    },
                    new DataGridViewButtonColumn
                    {
                        Name = "Edit",
                        Text = "Edit",
                        UseColumnTextForButtonValue = true,
                        FlatStyle = FlatStyle.Standard,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                    },
                    new DataGridViewButtonColumn
                    {
                        Name = "Delete",
                        Text = "Delete",
                        UseColumnTextForButtonValue = true,
                        FlatStyle = FlatStyle.Standard,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                    }
                };

                dutyGrid.Columns.AddRange(columns);
            }

            dutyGrid.DataSource = _dutyTypes;

            // Style the button columns
            if (dutyGrid.Columns["Edit"] is DataGridViewColumn editColumn)
            {
                editColumn.DefaultCellStyle.BackColor = AppStyling.Info;
                editColumn.DefaultCellStyle.ForeColor = Color.White;
            }

            if (dutyGrid.Columns["Delete"] is DataGridViewColumn deleteColumn)
            {
                deleteColumn.DefaultCellStyle.BackColor = AppStyling.Danger;
                deleteColumn.DefaultCellStyle.ForeColor = Color.White;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading duty types: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void DutyGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var dutyType = _dutyTypes[e.RowIndex];

        if (e.ColumnIndex >= 0 && dutyGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn buttonColumn)
        {
            if (buttonColumn.Name == "Edit")
            {
                // Show edit form
                var editForm = new DutyTypeForm(_context, dutyType);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDutyTypes();
                }
            }
            else if (buttonColumn.Name == "Delete")
            {
                // Check if the duty type is in use
                var inUseByMembers = await _context.MemberDuties.AnyAsync(md => md.DutyTypeId == dutyType.Id);
                var inUseByAssignments = await _context.DutyAssignments.AnyAsync(da => da.DutyTypeId == dutyType.Id);

                if (inUseByMembers || inUseByAssignments)
                {
                    MessageBox.Show(
                        "This duty type cannot be deleted because it is currently in use by members or assignments.",
                        "Cannot Delete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                if (MessageBox.Show(
                    $"Are you sure you want to delete the duty type '{dutyType.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        _context.DutyTypes.Remove(dutyType);
                        await _context.SaveChangesAsync();
                        await LoadDutyTypes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting duty type: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        var addForm = new DutyTypeForm(_context);
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            await LoadDutyTypes();
        }
    }

    private async void ShowDutyOrderForm(DutyCategory category, ServiceType serviceType)
    {
        var orderForm = new DutyOrderForm(_context, category, serviceType);
        if (orderForm.ShowDialog() == DialogResult.OK)
        {
            await LoadDutyTypes();
        }
    }
}
