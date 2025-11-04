using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;

namespace MonthlyScheduler.Forms;

public class DutyOrderForm : Form
{
    private readonly SchedulerDbContext _context;
    private readonly ListBox _dutyList;
    private readonly Button _btnUp;
    private readonly Button _btnDown;
    private readonly Button _btnClose;
    private readonly DutyCategory _category;
    private readonly ServiceType _serviceType;
    private readonly List<DutyType> _duties;

    public DutyOrderForm(SchedulerDbContext context, DutyCategory category, ServiceType serviceType)
    {
        _context = context;
        _category = category;
        _serviceType = serviceType;
        _duties = new List<DutyType>();

        Text = $"Order {_category} Duties - {GetServiceName(serviceType)}";
        Size = new Size(400, 500);
        StartPosition = FormStartPosition.CenterParent;


        // Create controls
        _dutyList = new ListBox
        {
            Dock = DockStyle.Fill,
            DrawMode = DrawMode.OwnerDrawFixed
        };
        _dutyList.DrawItem += DutyList_DrawItem;
        _dutyList.SelectedIndexChanged += DutyList_SelectedIndexChanged;

        _btnUp = new Button
        {
            Text = "Move Up",
            Enabled = false,
            Width = 100
        };
        _btnUp.Click += BtnUp_Click;
        _btnUp.ApplyModernStyle();


        _btnDown = new Button
        {
            Text = "Move Down",
            Enabled = false,
            Width = 100
        };
        _btnDown.Click += BtnDown_Click;
        _btnDown.ApplyModernStyle();

        _btnClose = new Button
        {
            Text = "Close",
            Width = 100,
            DialogResult = DialogResult.OK
        };
        _btnClose.ApplyModernStyle();

        // Create layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0)
        };

        buttonPanel.Controls.AddRange(new Control[] { _btnUp, _btnDown, _btnClose });

        mainLayout.Controls.Add(_dutyList, 0, 0);
        mainLayout.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainLayout);

        LoadDuties();
    }

    private string GetServiceName(ServiceType serviceType) => serviceType switch
    {
        ServiceType.Sunday_AM => "Sunday Morning",
        ServiceType.Sunday_PM => "Sunday Evening",
        ServiceType.Wednesday => "Wednesday",
        _ => throw new ArgumentException("Invalid service type")
    };

    private async void LoadDuties()
    {
        try
        {
            var duties = _serviceType switch
            {
                ServiceType.Sunday_AM => await _context.DutyTypes
                    .Where(d => d.Category == _category && d.IsMorningDuty)
                    .OrderBy(d => d.OrderIndexAM)
                    .ToListAsync(),
                ServiceType.Sunday_PM => await _context.DutyTypes
                    .Where(d => d.Category == _category && d.IsEveningDuty)
                    .OrderBy(d => d.OrderIndexPM)
                    .ToListAsync(),
                ServiceType.Wednesday => await _context.DutyTypes
                    .Where(d => d.Category == _category && d.IsWednesdayDuty)
                    .OrderBy(d => d.OrderIndexWednesday)
                    .ToListAsync(),
                _ => new List<DutyType>()
            };

            _duties.Clear();
            _duties.AddRange(duties);
            _dutyList.DataSource = null;
            _dutyList.DataSource = _duties;
            _dutyList.DisplayMember = "Name";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading duties: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DutyList_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();
        
        var duty = _duties[e.Index];
        var text = $"{e.Index + 1}. {duty.Name}";
        var brush = (e.State & DrawItemState.Selected) != 0 ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
        
        e.Graphics.DrawString(text, e.Font!, brush, e.Bounds);
        e.DrawFocusRectangle();
    }

    private void DutyList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var selectedIndex = _dutyList.SelectedIndex;
        _btnUp.Enabled = selectedIndex > 0;
        _btnDown.Enabled = selectedIndex >= 0 && selectedIndex < _dutyList.Items.Count - 1;
    }

    private void BtnUp_Click(object? sender, EventArgs e)
    {
        var selectedIndex = _dutyList.SelectedIndex;
        if (selectedIndex <= 0) return;

        SwapItems(selectedIndex, selectedIndex - 1);
    }

    private void BtnDown_Click(object? sender, EventArgs e)
    {
        var selectedIndex = _dutyList.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= _dutyList.Items.Count - 1) return;

        SwapItems(selectedIndex, selectedIndex + 1);
    }

    private async void SwapItems(int index1, int index2)
    {
        try
        {
            var temp = _duties[index1];
            _duties[index1] = _duties[index2];
            _duties[index2] = temp;

            // Update OrderIndex values for all duties based on their new positions
            for (int i = 0; i < _duties.Count; i++)
            {
                var duty = await _context.DutyTypes.FindAsync(_duties[i].Id);
                if (duty != null)
                {
                    // Set the appropriate OrderIndex value based on service type
                    switch (_serviceType)
                    {
                        case ServiceType.Sunday_AM:
                            duty.OrderIndexAM = i;
                            break;
                        case ServiceType.Sunday_PM:
                            duty.OrderIndexPM = i;
                            break;
                        case ServiceType.Wednesday:
                            duty.OrderIndexWednesday = i;
                            break;
                    }
                }
            }
            
            await _context.SaveChangesAsync();

            var selectedDuty = _duties[index2];
            _dutyList.DataSource = null;
            _dutyList.DataSource = _duties;
            _dutyList.DisplayMember = "Name";
            _dutyList.SelectedItem = selectedDuty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating duty order: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}