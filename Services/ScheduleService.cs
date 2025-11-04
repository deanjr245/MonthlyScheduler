using MonthlyScheduler.Models;
using MonthlyScheduler.Data;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Services;

public class ScheduleService
{
    private readonly SchedulerDbContext _context;
    private readonly Random _random = new();

    public ScheduleService(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<GeneratedSchedule> GenerateMonthlySchedule(int year, int month)
    {
        // Get all active members
        var members = await _context.Members
            .Include(m => m.AvailableDuties)
                .ThenInclude(ad => ad.DutyType)
            .Where(m => !m.ExcludeFromScheduling)
            .ToListAsync();

        if (!members.Any())
        {
            throw new InvalidOperationException("No active members found for scheduling");
        }

        var weeklySchedules = new List<WeeklySchedule>();
        var monthlyAssignments = new Dictionary<(int DutyId, ServiceType Service), HashSet<Member>>();
        var memberWeeklyAssignments = new Dictionary<DateTime, HashSet<Member>>();
        var dutyTypes = await _context.DutyTypes.AsNoTracking().ToListAsync();

        // Pre-filter duty types by service to avoid repeated filtering in the loop
        var morningDuties = dutyTypes.Where(dt => dt.IsMorningDuty).OrderBy(dt => dt.OrderIndexAM).ToList();
        var eveningDuties = dutyTypes.Where(dt => dt.IsEveningDuty).OrderBy(dt => dt.OrderIndexPM).ToList();
        var wednesdayDuties = dutyTypes.Where(dt => dt.IsWednesdayDuty).OrderBy(dt => dt.OrderIndexWednesday).ToList();
        
        // Service-independent monthly duties (not assigned to any specific service)
        var serviceIndependentMonthlyDuties = dutyTypes
            .Where(dt => dt.IsMonthlyDuty && !dt.IsMorningDuty && !dt.IsEveningDuty && !dt.IsWednesdayDuty)
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.OrderIndexPM)
            .ToList();

        // Initialize member monthly counts using LINQ
        var memberMonthlyCount = members.ToDictionary(m => m, m => 0);
        
        // Initialize monthly assignment tracking using LINQ
        var serviceTypes = new[] { ServiceType.Sunday_AM, ServiceType.Sunday_PM, ServiceType.Wednesday, ServiceType.Monthly };
        dutyTypes.SelectMany(duty => serviceTypes.Select(service => (duty.Id, service)))
            .ToList()
            .ForEach(key => monthlyAssignments[key] = new HashSet<Member>());

        // Get first Sunday of the month
        var sundayDate = GetFirstSundayOfMonth(year, month);
        
        DateTime lastSunday = GetLastSundayOfMonth(year, month);
        
        while (sundayDate <= lastSunday)
        {
            var weeklySchedule = new WeeklySchedule(sundayDate);
            
            // Create Sunday schedule
            var sundaySchedule = new Schedule(sundayDate);
            AssignDuties(sundaySchedule.AMDuties, members, monthlyAssignments, ServiceType.Sunday_AM,
                morningDuties, memberMonthlyCount, memberWeeklyAssignments, sundayDate);
            
            // Check if this is the last Sunday evening
            var isLastSunday = sundayDate.Date == lastSunday.Date;
            AssignDuties(sundaySchedule.PMDuties, members, monthlyAssignments, ServiceType.Sunday_PM,
                eveningDuties, memberMonthlyCount, memberWeeklyAssignments, sundayDate, isLastSunday);
            weeklySchedule.SundaySchedule = sundaySchedule;

            // Create Wednesday schedule - include it if the week starts in our target month
            var wednesdayDate = sundayDate.AddDays(3); // Wednesday is 3 days after Sunday
            if (sundayDate.Month == month)
            {
                var wednesdaySchedule = new Schedule(wednesdayDate);
                AssignDuties(wednesdaySchedule.WednesdayDuties, members, monthlyAssignments, ServiceType.Wednesday,
                    wednesdayDuties, memberMonthlyCount, memberWeeklyAssignments, sundayDate);
                weeklySchedule.WednesdaySchedule = wednesdaySchedule;
            }

            // Assign service-independent monthly duties to the Sunday schedule
            if (serviceIndependentMonthlyDuties.Any())
            {
                AssignDuties(sundaySchedule.MonthlyDuties, members, monthlyAssignments, ServiceType.Monthly,
                    serviceIndependentMonthlyDuties, memberMonthlyCount, memberWeeklyAssignments, sundayDate);
            }

            weeklySchedules.Add(weeklySchedule);
            sundayDate = sundayDate.AddDays(7);
        }

        // We'll handle any Wednesday that belongs to a week starting in our target month,
        // even if that Wednesday falls into the next calendar month

        // Mark any existing active schedule for this month as inactive
        // Create new generated schedule
        var generatedSchedule = new GeneratedSchedule
        {
            Year = year,
            Month = month,
            GeneratedDate = DateTime.Now
        };

         // Add daily schedules
        foreach (var weekSchedule in weeklySchedules)
        {
            if (weekSchedule.SundaySchedule != null)
            {
                var dailySchedule = new DailySchedule
                {
                    Date = weekSchedule.SundaySchedule.Date,
                    DayOfWeek = DayOfWeek.Sunday,
                    Schedule = generatedSchedule
                };

                // Add AM duties
                foreach (var (dutyType, member) in weekSchedule.SundaySchedule.AMDuties)
                {
                    // Skip null members (manually scheduled duties)
                    if (member == null) continue;

                    var amAssignment = new ScheduleAssignment
                    {
                        MemberId = member.Id,
                        DutyTypeId = dutyType.Id,
                        ServiceType = ServiceType.Sunday_AM,
                        DailyScheduleId = dailySchedule.Id
                    };
                    dailySchedule.Assignments.Add(amAssignment);
                }

                // Add PM duties
                foreach (var (dutyType, member) in weekSchedule.SundaySchedule.PMDuties)
                {
                    // Skip null members (manually scheduled duties)
                    if (member == null) continue;

                    var pmAssignment = new ScheduleAssignment
                    {
                        MemberId = member.Id,
                        DutyTypeId = dutyType.Id,
                        ServiceType = ServiceType.Sunday_PM,
                        DailyScheduleId = dailySchedule.Id
                    };
                    dailySchedule.Assignments.Add(pmAssignment);
                }

                // Add service-independent monthly duties
                foreach (var (dutyType, member) in weekSchedule.SundaySchedule.MonthlyDuties)
                {
                    // Skip null members (manually scheduled duties)
                    if (member == null) continue;

                    var monthlyAssignment = new ScheduleAssignment
                    {
                        MemberId = member.Id,
                        DutyTypeId = dutyType.Id,
                        ServiceType = ServiceType.Monthly,
                        DailyScheduleId = dailySchedule.Id
                    };
                    dailySchedule.Assignments.Add(monthlyAssignment);
                }

                generatedSchedule.DailySchedules.Add(dailySchedule);
            }

            if (weekSchedule.WednesdaySchedule != null)
            {
                var dailySchedule = new DailySchedule
                {
                    Date = weekSchedule.WednesdaySchedule.Date,
                    DayOfWeek = DayOfWeek.Wednesday,
                    Schedule = generatedSchedule
                };

                // Add Wednesday duties
                foreach (var (dutyType, member) in weekSchedule.WednesdaySchedule.WednesdayDuties)
                {
                    // Skip null members (manually scheduled duties)
                    if (member == null) continue;

                    var wednesdayAssignment = new ScheduleAssignment
                    {
                        MemberId = member.Id,
                        DutyTypeId = dutyType.Id,
                        ServiceType = ServiceType.Wednesday,
                        DailyScheduleId = dailySchedule.Id
                    };
                    dailySchedule.Assignments.Add(wednesdayAssignment);
                }

                generatedSchedule.DailySchedules.Add(dailySchedule);
            }
        }

        // Save the schedule
        await _context.GeneratedSchedules.AddAsync(generatedSchedule);
        await _context.SaveChangesAsync();

        return generatedSchedule;
    }

    private void AssignDuties(Dictionary<DutyType, Member> duties, List<Member> members, 
        Dictionary<(int DutyId, ServiceType Service), HashSet<Member>> monthlyAssignments,
        ServiceType serviceType, List<DutyType> dutyTypes,
        Dictionary<Member, int> memberMonthlyCount,
        Dictionary<DateTime, HashSet<Member>> memberWeeklyAssignments,
        DateTime weekStartDate, bool isLastSundayEvening = false)
    {
        // Keep track of members already assigned to this service
        var assignedToService = new HashSet<Member>();

        // Calculate date boundaries for monthly duties
        var firstSundayOfMonth = GetFirstSundayOfMonth(weekStartDate.Year, weekStartDate.Month);
        var lastSundayOfMonth = GetLastSundayOfMonth(weekStartDate.Year, weekStartDate.Month);

        // dutyTypes is already ordered by OrderIndex when passed in
        foreach (var dutyType in dutyTypes)
        {
            try
            {
                // Handle duties that should skip last Sunday evening
                if (isLastSundayEvening && dutyType.SkipLastSundayEvening)
                {
                    duties[dutyType] = null!; // Add as null to show placeholder
                    continue;
                }

                // Handle monthly duties that only occur on specific weeks
                if (dutyType.IsMonthlyDuty && dutyType.MonthlyDutyFrequency.HasValue)
                {
                    bool shouldSkip = dutyType.MonthlyDutyFrequency.Value switch
                    {
                        MonthlyDutyFrequency.StartOfMonth => weekStartDate.Date != firstSundayOfMonth.Date,
                        MonthlyDutyFrequency.EndOfMonth => weekStartDate.Date != lastSundayOfMonth.Date,
                        MonthlyDutyFrequency.EachWeek => false,
                        _ => false
                    };

                    if (shouldSkip)
                    {
                        continue; // Skip this duty for this week
                    }
                }

                // Always add manually scheduled duties to the list, but skip automatic assignment
                if (dutyType.ManuallyScheduled)
                {
                    duties[dutyType] = null!;
                    continue;
                }

                // Get eligible members for this duty
                var eligibleMembers = members
                    .Where(m => m.IsAvailableForDuty(dutyType))
                    .ToList();

                if (eligibleMembers.Any())
                {
                    // Filter out members based on all constraints
                    var availableMembers = eligibleMembers
                        .Where(m => 
                            // Not already assigned to this duty type in this service this month
                            !monthlyAssignments[(dutyType.Id, serviceType)].Contains(m) &&
                            // Not already assigned to this service
                            !assignedToService.Contains(m) &&
                            // Not used more than 3 times in the month (unless duty is exempt)
                            (dutyType.ExemptFromServiceMax || memberMonthlyCount[m] <= 3) &&
                            // Not used this week
                            (!memberWeeklyAssignments.ContainsKey(weekStartDate) || 
                             !memberWeeklyAssignments[weekStartDate].Contains(m)))
                        .ToList();

                    // If no members available, gradually relax constraints
                    if (!availableMembers.Any())
                    {
                        // Try allowing members already used this week but under monthly limit
                        availableMembers = eligibleMembers
                            .Where(m => 
                                !monthlyAssignments[(dutyType.Id, serviceType)].Contains(m) &&
                                !assignedToService.Contains(m) &&
                                memberMonthlyCount[m] <= 3)
                            .ToList();

                        if (!availableMembers.Any())
                        {
                            // Try allowing members already used this week but more than 3 times
                            availableMembers = eligibleMembers
                            .Where(m =>
                                !monthlyAssignments[(dutyType.Id, serviceType)].Contains(m) &&
                                !assignedToService.Contains(m))
                            .ToList();
                            
                            if (!availableMembers.Any())
                            {
                                throw new InvalidOperationException($"Unable to assign {dutyType.Name} duty. All eligible members are either at their monthly limit or already assigned to this service. Available members for this duty: {string.Join(", ", eligibleMembers.Select(m => m.FullName))}.");
                            }
                        }
                    }

                    // Randomly select a member
                    var selectedMember = availableMembers[_random.Next(availableMembers.Count)];
                    duties[dutyType] = selectedMember;

                    // Track all assignments
                    monthlyAssignments[(dutyType.Id, serviceType)].Add(selectedMember);
                    assignedToService.Add(selectedMember);
                    
                    // Update monthly count
                    memberMonthlyCount[selectedMember]++;
                    
                    // Update weekly assignments
                    if (!memberWeeklyAssignments.ContainsKey(weekStartDate))
                    {
                        memberWeeklyAssignments[weekStartDate] = new HashSet<Member>();
                    }
                    memberWeeklyAssignments[weekStartDate].Add(selectedMember);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error assigning {dutyType.Name}: {ex.Message}", ex);
            }
        }
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