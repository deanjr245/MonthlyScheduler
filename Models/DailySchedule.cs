using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class DailySchedule
{
    public int Id { get; set; }
    public int GeneratedScheduleId { get; set; }
    public GeneratedSchedule Schedule { get; set; } = null!;
    public DateTime Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public List<ScheduleAssignment> Assignments { get; set; } = new();
}