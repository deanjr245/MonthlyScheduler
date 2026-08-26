using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using MonthlyScheduler.UI;

namespace MonthlyScheduler.Forms;

public sealed class MemberAssignmentSummaryForm : Form
{
    private readonly SchedulerDbContext _context;
    private readonly int _year;
    private readonly int _month;
    private readonly TextBox _searchBox;
    private readonly DataGridView _summaryGrid;
    private readonly Label _statusLabel;
    private List<MemberAssignmentSummary> _summaries = new();

    public MemberAssignmentSummaryForm(SchedulerDbContext context, int year, int month)
    {
        _context = context;
        _year = year;
        _month = month;

        Text = $"Member Assignments - {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
        Size = new Size(1250, 650);
        MinimumSize = new Size(700, 450);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppStyling.DarkBackground;

        var searchLabel = new Label
        {
            Text = "Search member:",
            AutoSize = true,
            ForeColor = AppStyling.LightText,
            Anchor = AnchorStyles.Left,
            Font = AppStyling.Font
        };

        _searchBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Type a first or last name...",
            Margin = new Padding(8, 0, 0, 0)
        };
        _searchBox.TextChanged += (_, _) => UpdateGrid();

        var searchLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchLayout.Controls.Add(searchLabel, 0, 0);
        searchLayout.Controls.Add(_searchBox, 1, 0);

        _summaryGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
            RowTemplate = { Height =  thirtyFive }
        };
        _summaryGrid.ApplyModernStyle();
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Member",
            HeaderText = "Member",
            DataPropertyName = nameof(MemberAssignmentSummary.Member),
            Width = 190,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Count",
            HeaderText = "Assignments",
            DataPropertyName = nameof(MemberAssignmentSummary.Count),
            Width = 100,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Assignments",
            HeaderText = "Duties and dates",
            DataPropertyName = nameof(MemberAssignmentSummary.Assignments),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = AppStyling.LightText,
            Margin = new Padding(0, 8, 0, 0),
            Font = AppStyling.Font
        };

        var closeButton = new Button
        {
            Text = "Close",
            Width = 100,
            DialogResult = DialogResult.Cancel,
            Anchor = AnchorStyles.Right
        };
        closeButton.ApplySecondaryStyle();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(searchLayout, 0, 0);
        layout.Controls.Add(_summaryGrid, 0, 1);
        layout.Controls.Add(_statusLabel, 0, 2);
        layout.Controls.Add(closeButton, 0, 3);
        Controls.Add(layout);

        Load += async (_, _) => await LoadSummariesAsync();
    }

    private const int thirtyFive = 35;

    private async Task LoadSummariesAsync()
    {
        var schedule = await _context.GeneratedSchedules
            .Where(item => item.Year == _year && item.Month == _month)
            .FirstOrDefaultAsync();

        var members = await _context.Members
            .AsNoTracking()
            .Where(member => !member.ExcludeFromScheduling)
            .OrderBy(member => member.LastName)
            .ThenBy(member => member.FirstName)
            .ToListAsync();

        var assignments = schedule?.DailySchedules
            .SelectMany(day => day.Assignments.Select(assignment => new { day.Date, Assignment = assignment }))
            .Where(item => item.Assignment.Member != null)
            .ToList() ?? new();

        _summaries = members.Select(member =>
        {
            var memberAssignments = assignments
                .Where(item => item.Assignment.MemberId == member.Id)
                .OrderBy(item => item.Date)
                .ThenBy(item => item.Assignment.DutyType.Name)
                .ToList();

            return new MemberAssignmentSummary
            {
                Member = member.FullName,
                Count = memberAssignments.Count,
                Assignments = string.Join("; ", memberAssignments.Select(item =>
                    $"{item.Date:MMM d}: {item.Assignment.DutyType.Name}"))
            };
        }).ToList();

        UpdateGrid();
    }

    private void UpdateGrid()
    {
        var search = _searchBox.Text.Trim();
        var filtered = string.IsNullOrEmpty(search)
            ? _summaries
            : _summaries.Where(summary => summary.Member.Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();

        _summaryGrid.DataSource = null;
        _summaryGrid.DataSource = filtered;
        _statusLabel.Text = $"Showing {filtered.Count} of {_summaries.Count} members";
    }
}
