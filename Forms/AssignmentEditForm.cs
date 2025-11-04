using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Forms;

public class AssignmentEditForm : Form
{
    private readonly SchedulerDbContext _context;
    private readonly ComboBox _memberComboBox;
    private Member? _selectedMember;

    public Member? SelectedMember => _selectedMember;

    public AssignmentEditForm(SchedulerDbContext context, DutyType dutyType, ServiceType serviceType)
    {
        _context = context;
        Text = $"Edit {dutyType.Name} Assignment";
        Size = new Size(400, 200);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppStyling.LightBackground;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var selectionPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 0, 0, 10)
        };
        selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var label = new Label
        {
            Text = "Select Member:",
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        label.ApplyModernStyle();

        _memberComboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _memberComboBox.ApplyModernStyle();

        selectionPanel.Controls.Add(label, 0, 0);
        selectionPanel.Controls.Add(_memberComboBox, 1, 0);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            Width = 100,
            DialogResult = DialogResult.Cancel
        };
        btnCancel.ApplySecondaryStyle();

        var btnSave = new Button
        {
            Text = "Save",
            Width = 100,
            DialogResult = DialogResult.OK
        };
        btnSave.ApplyModernStyle();
        btnSave.Click += (s, e) => 
        {
            if (_memberComboBox.SelectedItem is Member member)
            {
                _selectedMember = member;
            }
        };

        buttonPanel.Controls.AddRange(new Control[] { btnCancel, btnSave });

        mainLayout.Controls.Add(selectionPanel, 0, 0);
        mainLayout.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainLayout);

        LoadMembers(dutyType);
    }

    private async void LoadMembers(DutyType dutyType)
    {
        try
        {
            var members = await _context.Members
                .Include(m => m.AvailableDuties)
                .Where(m => !m.ExcludeFromScheduling && 
                           m.AvailableDuties.Any(d => d.DutyTypeId == dutyType.Id))
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();

            _memberComboBox.DisplayMember = "FullName";
            _memberComboBox.ValueMember = "Id";
            _memberComboBox.DataSource = members;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading members: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
