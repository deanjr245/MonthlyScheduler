using MonthlyScheduler.Models;
using MonthlyScheduler.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Services;

public class ScheduleLoaderService
{
    private readonly SchedulerDbContext _context;

    public ScheduleLoaderService(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<DataTable> LoadScheduleData(int year, int month)
    {
        // Build the base schedule table (previously LoadScheduleToDataTable)
        var baseTable = new DataTable();
        var firstSunday = GetFirstSundayOfMonth(year, month);
        var lastSunday = GetLastSundayOfMonth(year, month);

        // Add fixed columns for service and duty types
        baseTable.Columns.Add("Service", typeof(string));
        baseTable.Columns.Add("Duty", typeof(string));

        // Find the schedule for this month - load all data in one query
        var schedule = await _context.GeneratedSchedules
            .AsNoTracking()
            .Include(s => s.DailySchedules)
                .ThenInclude(d => d.Assignments)
                    .ThenInclude(a => a.Member)
            .FirstOrDefaultAsync(s => s.Year == year && s.Month == month);

        // Load all duty types in one query and group by category
        var allDuties = await _context.DutyTypes
            .AsNoTracking()
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.OrderIndex)
            .ToListAsync();
        
        var dutiesByCategory = allDuties
            .GroupBy(dt => dt.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        var currentDate = firstSunday;
        while (currentDate <= lastSunday)
        {
            baseTable.Columns.Add(currentDate.ToString("MMM d"), typeof(string));
            currentDate = currentDate.AddDays(7);
        }

        // Add duties by category
        foreach (DutyCategory category in Enum.GetValues(typeof(DutyCategory)))
        {
            if (!dutiesByCategory.TryGetValue(category, out var categoryDuties))
            {
                continue; // Skip if no duties for this category
            }

            var categoryName = category.ToString();

            // Helper function to add a row for a specific duty type
            void AddDutyRow(string service, DutyType dutyType, ServiceType serviceType)
            {
                var row = baseTable.NewRow();
                row["Service"] = service;
                row["Duty"] = dutyType.Name;

                var d = firstSunday;
                while (d <= lastSunday)
                {
                    string? memberName = null;
                    if (schedule != null)
                    {
                        var dailySchedule = schedule.DailySchedules.FirstOrDefault(ds => ds.Date.Date ==
                            (serviceType == ServiceType.Wednesday ? d.AddDays(3).Date : d.Date));

                        if (dailySchedule != null)
                        {
                            var assignment = dailySchedule.Assignments
                                .FirstOrDefault(a => a.DutyTypeId == dutyType.Id && a.ServiceType == serviceType);
                            memberName = assignment?.Member?.FullName;
                        }
                    }

                    row[d.ToString("MMM d")] = memberName ?? "(Click to assign)";
                    d = d.AddDays(7);
                }
                baseTable.Rows.Add(row);
            }

            // Helper to add a monthly duty row based on its frequency
            void AddMonthlyDutyRow(DutyType dutyType)
            {
                var row = baseTable.NewRow();
                row["Service"] = $"{categoryName} - Monthly";
                row["Duty"] = dutyType.Name;

                var d = firstSunday;
                while (d <= lastSunday)
                {
                    string? memberName = null;
                    bool isApplicable = dutyType.MonthlyDutyFrequency switch
                    {
                        MonthlyDutyFrequency.StartOfMonth => d.Date == firstSunday.Date,
                        MonthlyDutyFrequency.EndOfMonth => d.Date == lastSunday.Date,
                        MonthlyDutyFrequency.EachWeek => true,
                        _ => false
                    };

                    if (isApplicable && schedule != null)
                    {
                        var dailySchedule = schedule.DailySchedules.FirstOrDefault(ds => ds.Date.Date == d.Date);
                        if (dailySchedule != null)
                        {
                            var assignment = dailySchedule.Assignments
                                .FirstOrDefault(a => a.DutyTypeId == dutyType.Id && a.ServiceType == ServiceType.Sunday_PM);
                            memberName = assignment?.Member?.FullName;
                        }
                    }

                    row[d.ToString("MMM d")] = isApplicable ? (memberName ?? "(Click to assign)") : string.Empty;
                    d = d.AddDays(7);
                }
                baseTable.Rows.Add(row);
            }

            // Partition out monthly duties
            var monthlyDuties = categoryDuties.Where(dt => dt.IsMonthlyDuty).OrderBy(dt => dt.OrderIndex).ToList();

            // Add Morning Service duties (excluding monthly duties)
            var morningDuties = categoryDuties.Where(dt => dt.IsMorningDuty && !dt.IsMonthlyDuty).OrderBy(dt => dt.OrderIndex);
            foreach (var duty in morningDuties)
            {
                AddDutyRow($"{categoryName} - Morning", duty, ServiceType.Sunday_AM);
            }

            // Add spacing row
            baseTable.Rows.Add(baseTable.NewRow());

            // Add Evening Service duties (excluding monthly duties)
            var eveningDuties = categoryDuties.Where(dt => dt.IsEveningDuty && !dt.IsMonthlyDuty).OrderBy(dt => dt.OrderIndex);
            foreach (var duty in eveningDuties)
            {
                AddDutyRow($"{categoryName} - Evening", duty, ServiceType.Sunday_PM);
            }

            // Add spacing row
            baseTable.Rows.Add(baseTable.NewRow());

            // Add Wednesday Service duties (excluding monthly duties)
            var wednesdayDuties = categoryDuties.Where(dt => dt.IsWednesdayDuty && !dt.IsMonthlyDuty).OrderBy(dt => dt.OrderIndex);
            foreach (var duty in wednesdayDuties)
            {
                AddDutyRow($"{categoryName} - Wednesday", duty, ServiceType.Wednesday);
            }

            // Add monthly duties if any exist (at the end, after Wednesday)
            if (monthlyDuties.Count > 0)
            {
                // Add spacing row before monthly duties
                baseTable.Rows.Add(baseTable.NewRow());
                
                foreach (var monthlyDuty in monthlyDuties)
                {
                    AddMonthlyDutyRow(monthlyDuty);
                }
            }

            // Add blank row between categories if not the last category
            if (category != DutyCategory.AudioVisual)
            {
                baseTable.Rows.Add(baseTable.NewRow());
            }
        }

        // Reorganize base rows into a single result, normalizing separators
        var result = baseTable.Clone();
        var baseRows = baseTable.Rows.Cast<DataRow>().ToList();

        // Process rows using indexed iteration for better performance
        for (int i = 0; i < baseRows.Count; i++)
        {
            var service = baseRows[i]["Service"]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(service))
            {
                // Keep blank row only if it's between two non-AV service rows
                var prev = i > 0 ? baseRows[i - 1]["Service"]?.ToString() ?? string.Empty : string.Empty;
                var next = i < baseRows.Count - 1 ? baseRows[i + 1]["Service"]?.ToString() ?? string.Empty : string.Empty;

                bool prevNonAV = !string.IsNullOrEmpty(prev) && !prev.StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase);
                bool nextNonAV = !string.IsNullOrEmpty(next) && !next.StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase);

                if (prevNonAV && nextNonAV)
                {
                    result.Rows.Add(result.NewRow());
                }
                continue;
            }

            // Non-AV rows are copied as-is
            if (!service.StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase))
            {
                result.ImportRow(baseRows[i]);
            }
        }

        // If there are AV rows, ensure exactly one separator between non-AV and AV sections
        bool hasAnyAV = baseRows.Any(r => (r["Service"]?.ToString() ?? string.Empty)
            .StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase));

        if (hasAnyAV && result.Rows.Count > 0)
        {
            var lastService = result.Rows[result.Rows.Count - 1]["Service"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(lastService))
            {
                result.Rows.Add(result.NewRow());
            }
        }

        // Append AV rows with single blank row between AV service types
        string? lastAVKind = null; // Morning/Evening/Wednesday
        
        baseRows.Where(row => 
            (row["Service"]?.ToString() ?? string.Empty).StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(row =>
            {
                var service = row["Service"]?.ToString() ?? string.Empty;
                string avKind = service.EndsWith("Morning", StringComparison.OrdinalIgnoreCase) ? "Morning"
                                : service.EndsWith("Evening", StringComparison.OrdinalIgnoreCase) ? "Evening"
                                : service.EndsWith("Wednesday", StringComparison.OrdinalIgnoreCase) ? "Wednesday"
                                : string.Empty;

                if (!string.IsNullOrEmpty(lastAVKind) && avKind != lastAVKind)
                {
                    // Insert a single separator when service kind changes
                    if (result.Rows.Count == 0 || !string.IsNullOrEmpty(result.Rows[result.Rows.Count - 1]["Service"]?.ToString()))
                    {
                        result.Rows.Add(result.NewRow());
                    }
                }

                result.ImportRow(row);
                lastAVKind = avKind;
            });

        // Trim any trailing blank rows
        while (result.Rows.Count > 0 && string.IsNullOrEmpty(result.Rows[result.Rows.Count - 1]["Service"]?.ToString()))
        {
            result.Rows.RemoveAt(result.Rows.Count - 1);
        }

        return result;
    }

    private static DateTime GetFirstSundayOfMonth(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        while (date.DayOfWeek != DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    private static DateTime GetLastSundayOfMonth(int year, int month)
    {
        var firstDayOfNextMonth = new DateTime(year, month, 1).AddMonths(1);
        var lastSunday = firstDayOfNextMonth.AddDays(-1);
        while (lastSunday.DayOfWeek != DayOfWeek.Sunday)
        {
            lastSunday = lastSunday.AddDays(-1);
        }
        return lastSunday;
    }
}