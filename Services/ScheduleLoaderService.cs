using MonthlyScheduler.Models;
using MonthlyScheduler.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Utilities;

namespace MonthlyScheduler.Services;

public class ScheduleLoaderService
{
    private readonly SchedulerDbContext _context;
    private const string ClickToAssignText = AppStringConstants.ClickToAssignText;
    private const string SongServiceText = "Song Service";
    private const string ServiceColumnName = "Service";
    private const string DutyColumnName = "Duty";

    public ScheduleLoaderService(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<DataTable> LoadScheduleData(int year, int month)
    {
        // Build the base schedule table
        var baseTable = new DataTable();
        var firstSunday = GetFirstSundayOfMonth(year, month);
        var lastSunday = GetLastSundayOfMonth(year, month);

        // Add fixed columns for service and duty types
        baseTable.Columns.Add(ServiceColumnName, typeof(string));
        baseTable.Columns.Add(DutyColumnName, typeof(string));

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
            .ThenBy(dt => dt.OrderIndexAM)
            .ToListAsync();
        
        var dutiesByCategory = allDuties
            .GroupBy(dt => dt.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Pre-calculate all Sunday dates and add columns
        // Column names use date format for display, but we access by index to avoid string lookups
        var sundayDates = new List<DateTime>();
        var currentDate = firstSunday;
        while (currentDate <= lastSunday)
        {
            sundayDates.Add(currentDate);
            var columnCaption = currentDate.ToString("MMM d");
            baseTable.Columns.Add(columnCaption, typeof(string));
            currentDate = currentDate.AddDays(7);
        }
        
        // Column indices: 0 = Service, 1 = Duty, 2+ = Sunday dates
        const int firstDateColumnIndex = 2;

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
                row[ServiceColumnName] = service;
                row[DutyColumnName] = dutyType.Name;

                for (int i = 0; i < sundayDates.Count; i++)
                {
                    var d = sundayDates[i];
                    var columnIndex = firstDateColumnIndex + i;
                    string? cellValue = null;
                    
                    // Check if this duty should be skipped on last Sunday evening
                    bool isLastSundayEvening = serviceType == ServiceType.Sunday_PM && d.Date == lastSunday.Date && dutyType.SkipLastSundayEvening;
                    
                    if (isLastSundayEvening)
                    {
                        row[columnIndex] = SongServiceText;
                    }
                    else
                    {
                        if (schedule != null)
                        {
                            var dailySchedule = schedule.DailySchedules.FirstOrDefault(ds => ds.Date.Date ==
                                (serviceType == ServiceType.Wednesday ? d.AddDays(3).Date : d.Date));

                            if (dailySchedule != null)
                            {
                                var assignment = dailySchedule.Assignments
                                    .FirstOrDefault(a => a.DutyTypeId == dutyType.Id && a.ServiceType == serviceType);
                                cellValue = dutyType.ManualAssignmentType == ManualAssignmentType.TextInput ? assignment?.Notes : assignment?.Member?.FullName;
                            }
                        }

                        row[columnIndex] = cellValue ?? ClickToAssignText;
                    }
                }
                baseTable.Rows.Add(row);
            }

            // Helper to add a monthly duty row based on its frequency
            void AddMonthlyDutyRow(DutyType dutyType, ServiceType serviceType, string serviceName)
            {
                var row = baseTable.NewRow();
                row[ServiceColumnName] = serviceName;
                row[DutyColumnName] = dutyType.Name;

                for (int i = 0; i < sundayDates.Count; i++)
                {
                    var d = sundayDates[i];
                    var columnIndex = firstDateColumnIndex + i;
                    string? memberName = null;
                    bool isApplicable = dutyType.MonthlyDutyFrequency switch
                    {
                        MonthlyDutyFrequency.StartOfMonth => d.Date == firstSunday.Date,
                        MonthlyDutyFrequency.EndOfMonth => d.Date == lastSunday.Date,
                        MonthlyDutyFrequency.EachWeek => true,
                        _ => false
                    };

                    // Check if this duty should be skipped on last Sunday evening
                    bool isLastSundayEvening = serviceType == ServiceType.Sunday_PM && d.Date == lastSunday.Date && dutyType.SkipLastSundayEvening;
                    
                    if (isLastSundayEvening)
                    {
                        row[columnIndex] = SongServiceText;
                    }
                    else if (isApplicable && schedule != null)
                    {
                        // For service-independent monthly duties, only look at Sunday schedules
                        var dailySchedule = serviceType == ServiceType.Monthly
                            ? schedule.DailySchedules.FirstOrDefault(ds => ds.Date.Date == d.Date && ds.DayOfWeek == DayOfWeek.Sunday)
                            : schedule.DailySchedules.FirstOrDefault(ds => ds.Date.Date == d.Date);
                            
                        if (dailySchedule != null)
                        {
                            var assignment = dailySchedule.Assignments
                                .FirstOrDefault(a => a.DutyTypeId == dutyType.Id && a.ServiceType == serviceType);
                            memberName = assignment?.Member?.FullName;
                        }

                        row[columnIndex] = memberName ?? ClickToAssignText;
                    }
                    else
                    {
                        row[columnIndex] = isApplicable ? ClickToAssignText : string.Empty;
                    }
                }
                baseTable.Rows.Add(row);
            }

            // Add Morning Service duties (including monthly morning duties)
            var morningDuties = categoryDuties.Where(dt => dt.IsMorningDuty).OrderBy(dt => dt.OrderIndexAM);
            foreach (var duty in morningDuties)
            {
                if (duty.IsMonthlyDuty)
                {
                    AddMonthlyDutyRow(duty, ServiceType.Sunday_AM, $"{categoryName} - Morning");
                }
                else
                {
                    AddDutyRow($"{categoryName} - Morning", duty, ServiceType.Sunday_AM);
                }
            }

            // Add spacing row
            baseTable.Rows.Add(baseTable.NewRow());

            // Add Evening Service duties (including monthly evening duties)
            var eveningDuties = categoryDuties.Where(dt => dt.IsEveningDuty).OrderBy(dt => dt.OrderIndexPM);
            foreach (var duty in eveningDuties)
            {
                if (duty.IsMonthlyDuty)
                {
                    AddMonthlyDutyRow(duty, ServiceType.Sunday_PM, $"{categoryName} - Evening");
                }
                else
                {
                    AddDutyRow($"{categoryName} - Evening", duty, ServiceType.Sunday_PM);
                }
            }

            // Add spacing row
            baseTable.Rows.Add(baseTable.NewRow());

            // Add Wednesday Service duties (including monthly Wednesday duties)
            var wednesdayDuties = categoryDuties.Where(dt => dt.IsWednesdayDuty).OrderBy(dt => dt.OrderIndexWednesday);
            foreach (var duty in wednesdayDuties)
            {
                if (duty.IsMonthlyDuty)
                {
                    AddMonthlyDutyRow(duty, ServiceType.Wednesday, $"{categoryName} - Wednesday");
                }
                else
                {
                    AddDutyRow($"{categoryName} - Wednesday", duty, ServiceType.Wednesday);
                }
            }

            // Add service-independent monthly duties (not assigned to any specific service)
            var serviceIndependentMonthlyDuties = categoryDuties
                .Where(dt => dt.IsMonthlyDuty && !dt.IsMorningDuty && !dt.IsEveningDuty && !dt.IsWednesdayDuty)
                .OrderBy(dt => dt.OrderIndexPM)
                .ToList();

            if (serviceIndependentMonthlyDuties.Any())
            {
                // Add spacing row before service-independent duties
                baseTable.Rows.Add(baseTable.NewRow());

                foreach (var duty in serviceIndependentMonthlyDuties)
                {
                    // Use ServiceType.Monthly for service-independent duties
                    AddMonthlyDutyRow(duty, ServiceType.Monthly, $"{categoryName} - Monthly");
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
            var service = baseRows[i][ServiceColumnName]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(service))
            {
                // Keep blank row only if it's between two non-AV service rows
                var prev = i > 0 ? baseRows[i - 1][ServiceColumnName]?.ToString() ?? string.Empty : string.Empty;
                var next = i < baseRows.Count - 1 ? baseRows[i + 1][ServiceColumnName]?.ToString() ?? string.Empty : string.Empty;

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
        bool hasAnyAV = baseRows.Any(r => (r[ServiceColumnName]?.ToString() ?? string.Empty)
            .StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase));

        if (hasAnyAV && result.Rows.Count > 0)
        {
            var lastService = result.Rows[result.Rows.Count - 1][ServiceColumnName]?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(lastService))
            {
                result.Rows.Add(result.NewRow());
            }
        }

        // Append AV rows with single blank row between AV service types
        string? lastAVKind = null; // Morning/Evening/Wednesday
        
        baseRows.Where(row => 
            (row[ServiceColumnName]?.ToString() ?? string.Empty).StartsWith("AudioVisual", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(row =>
            {
                var service = row[ServiceColumnName]?.ToString() ?? string.Empty;
                string avKind = service.EndsWith("Morning", StringComparison.OrdinalIgnoreCase) ? "Morning"
                                : service.EndsWith("Evening", StringComparison.OrdinalIgnoreCase) ? "Evening"
                                : service.EndsWith("Wednesday", StringComparison.OrdinalIgnoreCase) ? "Wednesday"
                                : string.Empty;

                if (!string.IsNullOrEmpty(lastAVKind) && avKind != lastAVKind)
                {
                    // Insert a single separator when service kind changes
                    if (result.Rows.Count == 0 || !string.IsNullOrEmpty(result.Rows[result.Rows.Count - 1][ServiceColumnName]?.ToString()))
                    {
                        result.Rows.Add(result.NewRow());
                    }
                }

                result.ImportRow(row);
                lastAVKind = avKind;
            });

        // Trim any trailing blank rows
        while (result.Rows.Count > 0 && string.IsNullOrEmpty(result.Rows[result.Rows.Count - 1][ServiceColumnName]?.ToString()))
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