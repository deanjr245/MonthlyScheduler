namespace MonthlyScheduler.Models;

public class MonthlySchedule
{
    private readonly List<ServiceSchedule> _serviceSchedules;
    public DateTime MonthStartDate { get; }

    public MonthlySchedule(DateTime startDate)
    {
        MonthStartDate = new DateTime(startDate.Year, startDate.Month, 1);
        _serviceSchedules = new List<ServiceSchedule>();
        GenerateScheduleDates();
    }

    private void GenerateScheduleDates()
    {
        var currentDate = MonthStartDate;
        var endDate = MonthStartDate.AddMonths(1);

        while (currentDate < endDate)
        {
            // Add Sunday services
            if (currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                _serviceSchedules.Add(new ServiceSchedule(currentDate, Service.SundayMorning));
                _serviceSchedules.Add(new ServiceSchedule(currentDate, Service.SundayEvening));
            }
            // Add Wednesday services
            else if (currentDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                _serviceSchedules.Add(new ServiceSchedule(currentDate, Service.Wednesday));
            }

            currentDate = currentDate.AddDays(1);
        }
    }

    public bool CanAssignMemberForWeek(Member member, DateTime weekStarting)
    {
        // Get all services for the week (Sunday to Saturday)
        var weekStart = weekStarting.Date;
        var weekEnd = weekStart.AddDays(7);

        var weeklyServices = _serviceSchedules
            .Where(s => s.Date >= weekStart && s.Date < weekEnd);

        // Check if member is already assigned to any service this week
        return !weeklyServices.Any(service => 
            service.Assignments.Any(a => a.MemberId == member.Id));
    }

    public bool TryAssignDuty(DateTime date, Service service, DutyType duty, Member member)
    {
        var serviceSchedule = _serviceSchedules
            .FirstOrDefault(s => s.Date.Date == date.Date && s.Service == service);

        if (serviceSchedule == null)
            return false;

        // Get the start of the week (Sunday)
        var weekStart = date.AddDays(-(int)date.DayOfWeek);

        // Check if member can be assigned for this week
        if (!CanAssignMemberForWeek(member, weekStart))
            return false;

        return serviceSchedule.AssignDuty(duty, member);
    }

    public IEnumerable<ServiceSchedule> GetSchedulesForWeek(DateTime weekStarting)
    {
        var weekStart = weekStarting.Date;
        var weekEnd = weekStart.AddDays(7);

        return _serviceSchedules
            .Where(s => s.Date >= weekStart && s.Date < weekEnd)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Service);
    }

    public IEnumerable<ServiceSchedule> GetAllSchedules()
    {
        return _serviceSchedules
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Service);
    }
}