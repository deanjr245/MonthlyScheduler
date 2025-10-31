using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;

namespace MonthlyScheduler.Forms;

public partial class DutyTypeForm : Form
{
    private readonly SchedulerDbContext _context;
    private readonly DutyType? _dutyType;
    private TextBox nameBox = null!;
    private TextBox descriptionBox = null!;
    private ComboBox categoryCombo = null!;
    private CheckBox morningCheck = null!;
    private CheckBox eveningCheck = null!;
    private CheckBox wednesdayCheck = null!;
    private CheckBox exemptFromServiceMaxCheck = null!;
    private CheckBox manuallyScheduledCheck = null!;
    private ComboBox manualAssignmentTypeCombo = null!;
    private CheckBox monthlyDutyCheck = null!;
    private ComboBox monthlyDutyFrequencyCombo = null!;
    private Button saveButton = null!;
    private Button cancelButton = null!;

    public DutyTypeForm(SchedulerDbContext context, DutyType? dutyType = null)
    {
        _context = context;
        _dutyType = dutyType;
        InitializeComponent();
        InitializeControls();
        SetupLayout();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = _dutyType == null ? "Add Duty Type" : "Edit Duty Type";
        Size = new Size(550, 750);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
    }

    private void InitializeControls()
    {
        // Name input
        nameBox = new TextBox { Width = 300 };
        nameBox.Font = AppStyling.Font;

        // Description input (multiline)
        descriptionBox = new TextBox 
        { 
            Width = 300,
            Height = 100,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        descriptionBox.Font = AppStyling.Font;

        // Category dropdown
        categoryCombo = new ComboBox
        {
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        categoryCombo.Items.AddRange(Enum.GetNames(typeof(DutyCategory)));
        categoryCombo.SelectedIndex = 0;
        categoryCombo.ApplyModernStyle();

        // Service checkboxes
        morningCheck = new CheckBox { Text = "Include in Morning Service", AutoSize = true, MaximumSize = new Size(450, 0) };
        morningCheck.Font = AppStyling.Font;
        
        eveningCheck = new CheckBox { Text = "Include in Evening Service", AutoSize = true, MaximumSize = new Size(450, 0) };
        eveningCheck.Font = AppStyling.Font;
        
        wednesdayCheck = new CheckBox { Text = "Include in Wednesday Service", AutoSize = true, MaximumSize = new Size(450, 0) };
        wednesdayCheck.Font = AppStyling.Font;

        exemptFromServiceMaxCheck = new CheckBox { Text = "Exempt from Service Maximum", AutoSize = true, MaximumSize = new Size(450, 0) };
        exemptFromServiceMaxCheck.Font = AppStyling.Font;

        manuallyScheduledCheck = new CheckBox { Text = "Include but don't schedule", AutoSize = true, MaximumSize = new Size(450, 0) };
        manuallyScheduledCheck.Font = AppStyling.Font;
        manuallyScheduledCheck.CheckedChanged += ManuallyScheduledCheck_CheckedChanged;

        manualAssignmentTypeCombo = new ComboBox
        {
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };
        manualAssignmentTypeCombo.Items.AddRange(GetEnumDisplayNames<Models.ManualAssignmentType>());
        manualAssignmentTypeCombo.SelectedIndex = 0;
        manualAssignmentTypeCombo.ApplyModernStyle();

        monthlyDutyCheck = new CheckBox { Text = "Monthly Duty (not tied to a specific service)", AutoSize = true, MaximumSize = new Size(450, 0) };
        monthlyDutyCheck.Font = AppStyling.Font;
        monthlyDutyCheck.CheckedChanged += MonthlyDutyCheck_CheckedChanged;

        monthlyDutyFrequencyCombo = new ComboBox
        {
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };
        monthlyDutyFrequencyCombo.Items.AddRange(GetEnumDisplayNames<MonthlyDutyFrequency>());
        monthlyDutyFrequencyCombo.SelectedIndex = 0;
        monthlyDutyFrequencyCombo.ApplyModernStyle();

        // Buttons
        saveButton = new Button { Text = "Save", Width = 100 };
        saveButton.Click += SaveButton_Click;
        saveButton.ApplyModernStyle();

        cancelButton = new Button { Text = "Cancel", Width = 100 };
        cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        cancelButton.ApplySecondaryStyle();
    }

    private void SetupLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 8,
            AutoSize = true
        };

        // Add controls with labels
        AddControlWithLabel(mainLayout, "Name:", nameBox, 0);
        AddControlWithLabel(mainLayout, "Description:", descriptionBox, 1);
        AddControlWithLabel(mainLayout, "Category:", categoryCombo, 2);

        // Services group
        var servicesGroup = new GroupBox
        {
            Text = "Service Availability",
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            AutoSize = true,
            MinimumSize = new Size(0, 150)
        };
        var servicesLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        servicesLayout.Controls.Add(morningCheck);
        servicesLayout.Controls.Add(eveningCheck);
        servicesLayout.Controls.Add(wednesdayCheck);
        servicesLayout.Controls.Add(exemptFromServiceMaxCheck);
        servicesLayout.Controls.Add(manuallyScheduledCheck);
        
        var manualAssignmentPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(20, 5, 0, 0)
        };
        var manualAssignmentLabel = new Label { Text = "Type:", AutoSize = true, Margin = new Padding(0, 5, 10, 0) };
        manualAssignmentLabel.ApplyModernStyle();
        manualAssignmentPanel.Controls.Add(manualAssignmentLabel);
        manualAssignmentPanel.Controls.Add(manualAssignmentTypeCombo);
        servicesLayout.Controls.Add(manualAssignmentPanel);
        
        servicesGroup.Controls.Add(servicesLayout);
        mainLayout.Controls.Add(servicesGroup, 0, 3);

        // Monthly Duty group
        var monthlyDutyGroup = new GroupBox
        {
            Text = "Monthly Duty",
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            AutoSize = true,
            MinimumSize = new Size(0, 100)
        };
        var monthlyDutyLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        monthlyDutyLayout.Controls.Add(monthlyDutyCheck);
        
        var frequencyPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(20, 5, 0, 0)
        };
        var frequencyLabel = new Label { Text = "Frequency:", AutoSize = true, Margin = new Padding(0, 5, 10, 0) };
        frequencyLabel.ApplyModernStyle();
        frequencyPanel.Controls.Add(frequencyLabel);
        frequencyPanel.Controls.Add(monthlyDutyFrequencyCombo);
        monthlyDutyLayout.Controls.Add(frequencyPanel);
        
        monthlyDutyGroup.Controls.Add(monthlyDutyLayout);
        mainLayout.Controls.Add(monthlyDutyGroup, 0, 4);

        // Buttons panel
        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        buttonsPanel.Controls.AddRange(new Control[] { cancelButton, saveButton });
        mainLayout.Controls.Add(buttonsPanel, 0, 5);

        Controls.Add(mainLayout);
    }

    private void AddControlWithLabel(TableLayoutPanel layout, string labelText, Control control, int row)
    {
        var label = new Label { Text = labelText, AutoSize = true };
        label.ApplyModernStyle();

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        panel.Controls.AddRange(new Control[] { label, control });

        layout.Controls.Add(panel, 0, row);
    }

    private void LoadData()
    {
        if (_dutyType != null)
        {
            nameBox.Text = _dutyType.Name;
            descriptionBox.Text = _dutyType.Description;
            categoryCombo.SelectedItem = _dutyType.Category.ToString();
            morningCheck.Checked = _dutyType.IsMorningDuty;
            eveningCheck.Checked = _dutyType.IsEveningDuty;
            wednesdayCheck.Checked = _dutyType.IsWednesdayDuty;
            exemptFromServiceMaxCheck.Checked = _dutyType.ExemptFromServiceMax;
            manuallyScheduledCheck.Checked = _dutyType.ManuallyScheduled;
            
            if (_dutyType.ManuallyScheduled && _dutyType.ManualAssignmentType.HasValue)
            {
                manualAssignmentTypeCombo.SelectedIndex = (int)_dutyType.ManualAssignmentType.Value;
            }
            
            monthlyDutyCheck.Checked = _dutyType.IsMonthlyDuty;
            
            if (_dutyType.IsMonthlyDuty && _dutyType.MonthlyDutyFrequency.HasValue)
            {
                monthlyDutyFrequencyCombo.SelectedIndex = (int)_dutyType.MonthlyDutyFrequency.Value;
            }
        }
    }

    private void ManuallyScheduledCheck_CheckedChanged(object? sender, EventArgs e)
    {
        manualAssignmentTypeCombo.Enabled = manuallyScheduledCheck.Checked;
    }

    private void MonthlyDutyCheck_CheckedChanged(object? sender, EventArgs e)
    {
        monthlyDutyFrequencyCombo.Enabled = monthlyDutyCheck.Checked;
        
        // When monthly duty is checked, disable service checkboxes
        if (monthlyDutyCheck.Checked)
        {
            morningCheck.Checked = false;
            morningCheck.Enabled = false;
            eveningCheck.Checked = false;
            eveningCheck.Enabled = false;
            wednesdayCheck.Checked = false;
            wednesdayCheck.Enabled = false;
        }
        else
        {
            morningCheck.Enabled = true;
            eveningCheck.Enabled = true;
            wednesdayCheck.Enabled = true;
        }
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            MessageBox.Show("Please enter a name for the duty type.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(descriptionBox.Text))
        {
            MessageBox.Show("Please enter a description for the duty type.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_dutyType == null)
            {
                // Get the category for the new duty type
                var selectedCategory = categoryCombo.SelectedItem?.ToString() is string category 
                    ? (DutyCategory)Enum.Parse(typeof(DutyCategory), category) 
                    : DutyCategory.Worship;

                // Find the max OrderIndex for this category and set the new one to max + 1
                var maxOrderIndex = await _context.DutyTypes
                    .Where(dt => dt.Category == selectedCategory)
                    .MaxAsync(dt => (int?)dt.OrderIndex) ?? -1;

                // Create new duty type
                var dutyType = new DutyType
                {
                    Name = nameBox.Text.Trim(),
                    Description = descriptionBox.Text.Trim(),
                    Category = selectedCategory,
                    IsMorningDuty = morningCheck.Checked,
                    IsEveningDuty = eveningCheck.Checked,
                    IsWednesdayDuty = wednesdayCheck.Checked,
                    ExemptFromServiceMax = exemptFromServiceMaxCheck.Checked,
                    ManuallyScheduled = manuallyScheduledCheck.Checked,
                    ManualAssignmentType = manuallyScheduledCheck.Checked 
                        ? (Models.ManualAssignmentType)manualAssignmentTypeCombo.SelectedIndex 
                        : null,
                    IsMonthlyDuty = monthlyDutyCheck.Checked,
                    MonthlyDutyFrequency = monthlyDutyCheck.Checked 
                        ? (MonthlyDutyFrequency)monthlyDutyFrequencyCombo.SelectedIndex 
                        : null,
                    OrderIndex = maxOrderIndex + 1
                };

                _context.DutyTypes.Add(dutyType);
            }
            else
            {
                // Update existing duty type
                _dutyType.Name = nameBox.Text.Trim();
                _dutyType.Description = descriptionBox.Text.Trim();
                _dutyType.Category = categoryCombo.SelectedItem?.ToString() is string category ? (DutyCategory)Enum.Parse(typeof(DutyCategory), category) : DutyCategory.Worship;
                _dutyType.IsMorningDuty = morningCheck.Checked;
                _dutyType.IsEveningDuty = eveningCheck.Checked;
                _dutyType.IsWednesdayDuty = wednesdayCheck.Checked;
                _dutyType.ExemptFromServiceMax = exemptFromServiceMaxCheck.Checked;
                _dutyType.ManuallyScheduled = manuallyScheduledCheck.Checked;
                _dutyType.ManualAssignmentType = manuallyScheduledCheck.Checked 
                    ? (Models.ManualAssignmentType)manualAssignmentTypeCombo.SelectedIndex 
                    : null;
                _dutyType.IsMonthlyDuty = monthlyDutyCheck.Checked;
                _dutyType.MonthlyDutyFrequency = monthlyDutyCheck.Checked 
                    ? (MonthlyDutyFrequency)monthlyDutyFrequencyCombo.SelectedIndex 
                    : null;

                _context.Entry(_dutyType).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving duty type: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string[] GetEnumDisplayNames<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(value =>
            {
                var field = typeof(TEnum).GetField(value.ToString()!);
                var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                return attribute?.Name ?? value.ToString();
            })
            .ToArray();
    }
}
