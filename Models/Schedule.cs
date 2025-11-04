using System.Collections.ObjectModel;

namespace MonthlyScheduler.Models;

public class Schedule
{
    public DateTime Date { get; set; }
    public Dictionary<DutyType, Member> AMDuties { get; set; } = new();
    public Dictionary<DutyType, Member> PMDuties { get; set; } = new();
    public Dictionary<DutyType, Member> WednesdayDuties { get; set; } = new();
    public Dictionary<DutyType, Member> MonthlyDuties { get; set; } = new();
    public List<DutyAssignment> AllAssignments { get; set; } = new();

    public Schedule(DateTime date)
    {
        Date = date;
    }
}

