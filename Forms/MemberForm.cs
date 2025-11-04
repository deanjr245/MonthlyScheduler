using MonthlyScheduler.Models;
using MonthlyScheduler.Data;
using MonthlyScheduler.UI;
using MonthlyScheduler.Utilities;
using Microsoft.EntityFrameworkCore;
using static MonthlyScheduler.Utilities.AppStringConstants;

namespace MonthlyScheduler.Forms;

public partial class MemberForm : Form
{
    private readonly SchedulerDbContext _context;
    private readonly Member? _member;
    private readonly Dictionary<int, CheckBox> _dutyCheckboxes = new();

    public MemberForm(SchedulerDbContext context, Member? member = null)
    {
        _context = context;
        _member = member;
        InitializeComponent();
        LoadMemberData();
    }

    private void InitializeComponent()
    {
        Text = _member == null ? FormTitleMemberAdd : FormTitleMemberEdit;
        Size = new Size(550, 800);
        MinimumSize = new Size(450, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppStyling.DarkBackground;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            ColumnCount = 2,
            RowCount = 2,
            BackColor = AppStyling.LightBackground
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Personal info section
        var personalInfoGroup = new GroupBox
        {
            Text = "Personal Information",
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            AutoSize = true
        };

        var personalInfoLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(10)
        };
        personalInfoLayout.AutoSize = true;
        personalInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        personalInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        for (int i = 0; i < 4; i++)
        {
            personalInfoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        // First Name
        var lblFirstName = new Label { Text = "First Name:", Dock = DockStyle.Fill };
        lblFirstName.ApplyModernStyle();
        personalInfoLayout.Controls.Add(lblFirstName, 0, 0);
        txtFirstName = new TextBox { Dock = DockStyle.Fill, Height = 30, Font = new Font("Segoe UI", 9) };
        personalInfoLayout.Controls.Add(txtFirstName, 1, 0);

        // Last Name
        var lblLastName = new Label { Text = "Last Name:", Dock = DockStyle.Fill };
        lblLastName.ApplyModernStyle();
        personalInfoLayout.Controls.Add(lblLastName, 0, 1);
        txtLastName = new TextBox { Dock = DockStyle.Fill, Height = 30, Font = new Font("Segoe UI", 9) };
        personalInfoLayout.Controls.Add(txtLastName, 1, 1);

        var formLabel = new Label { Text = "Form Received:", Dock = DockStyle.Fill };
        formLabel.ApplyModernStyle();
        personalInfoLayout.Controls.Add(formLabel, 0, 2);
        chkFormReceived = new CheckBox { Dock = DockStyle.Fill, Text = "Yes" };
        personalInfoLayout.Controls.Add(chkFormReceived, 1, 2);

        var excludeLabel = new Label { Text = "Exclude from Scheduling:", Dock = DockStyle.Fill };
        excludeLabel.ApplyModernStyle();
        personalInfoLayout.Controls.Add(excludeLabel, 0, 3);
        chkExcludeFromScheduling = new CheckBox { Dock = DockStyle.Fill, Text = "Yes" };
        personalInfoLayout.Controls.Add(chkExcludeFromScheduling, 1, 3);

        personalInfoGroup.Controls.Add(personalInfoLayout);
        mainLayout.Controls.Add(personalInfoGroup, 0, 0);
        mainLayout.SetColumnSpan(personalInfoGroup, 2);

        // Duties section
        var dutiesGroup = new GroupBox
        {
            Text = "Available Duties",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(10)
        };
        dutiesGroup.Font = new Font("Segoe UI", 9, FontStyle.Regular);

        var dutiesLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            WrapContents = false
        };

        // Load duty types from database
        var dutyTypes = _context.DutyTypes
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToList();

        foreach (var dutyType in dutyTypes)
        {
            var chk = new CheckBox
            {
                Text = dutyType.Name,
                Tag = dutyType,  // Store the full duty type object
                AutoSize = true,
                Margin = new Padding(5)
            };
            _dutyCheckboxes[dutyType.Id] = chk;
            dutiesLayout.Controls.Add(chk);
        }

        dutiesGroup.Controls.Add(dutiesLayout);
        mainLayout.Controls.Add(dutiesGroup, 0, 1);
        mainLayout.SetColumnSpan(dutiesGroup, 2);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 60,
            Padding = new Padding(10),
            BackColor = AppStyling.Light
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 100
        };
        btnCancel.ApplySecondaryStyle();
        btnCancel.Click += (s, e) => Close();

        var btnSave = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Width = 100
        };
        btnSave.ApplyModernStyle();
        btnSave.Click += BtnSave_Click;

        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);

        Controls.Add(mainLayout);
        Controls.Add(buttonPanel);
    }

    private void LoadMemberData()
    {
        if (_member != null)
        {
            txtFirstName.Text = _member.FirstName;
            txtLastName.Text = _member.LastName;
            chkFormReceived.Checked = _member.HasSubmittedForm;
            chkExcludeFromScheduling.Checked = _member.ExcludeFromScheduling;

            var memberDutyIds = _member.AvailableDuties.Select(d => d.DutyTypeId).ToHashSet();
            foreach (var dutyCheckbox in _dutyCheckboxes)
            {
                dutyCheckbox.Value.Checked = memberDutyIds.Contains(dutyCheckbox.Key);
            }
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show(FirstLastNameRequired, ValidationErrorTitle, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_member == null)
            {
                // Creating new member
                var newMember = new Member
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    HasSubmittedForm = chkFormReceived.Checked,
                    ExcludeFromScheduling = chkExcludeFromScheduling.Checked
                };

                // Add selected duties
                foreach (var duty in _dutyCheckboxes.Where(d => d.Value.Checked))
                {
                    if (duty.Value.Tag is DutyType dutyType)
                    {
                        newMember.AddDuty(dutyType);
                    }
                }

                await _context.Members.AddAsync(newMember);
            }
            else
            {
                try
                {
                    // Updating existing member
                    var member = await _context.Members
                        .Include(m => m.AvailableDuties)
                        .ThenInclude(d => d.DutyType)  // Include DutyType to ensure it's loaded
                        .FirstOrDefaultAsync(m => m.Id == _member.Id);

                    if (member == null)
                    {
                        throw new Exception("Member not found");
                    }

                    // Update basic properties
                    member.FirstName = txtFirstName.Text.Trim();
                    member.LastName = txtLastName.Text.Trim();
                    member.HasSubmittedForm = chkFormReceived.Checked;
                    member.ExcludeFromScheduling = chkExcludeFromScheduling.Checked;

                    // Clear and update duties
                    member.AvailableDuties.Clear();
                    
                    // Get all duty types from context to ensure they're tracked
                    var dutyTypes = await _context.DutyTypes.ToDictionaryAsync(dt => dt.Id);
                    
                    foreach (var duty in _dutyCheckboxes.Where(d => d.Value.Checked))
                    {
                        if (duty.Value.Tag is DutyType dutyType && dutyTypes.TryGetValue(dutyType.Id, out var trackedDutyType))
                        {
                            member.AddDuty(trackedDutyType);
                        }
                    }

                    _context.Members.Update(member);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating member: {ex.Message}", ex);
                }
            }

            await _context.SaveChangesAsync();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving member: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private TextBox txtFirstName = null!;
    private TextBox txtLastName = null!;
    private CheckBox chkFormReceived = null!;
    private CheckBox chkExcludeFromScheduling = null!;
}
