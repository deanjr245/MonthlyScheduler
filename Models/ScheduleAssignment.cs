namespace MonthlyScheduler.Models;

public class ScheduleAssignment
{
    public int Id { get; set; }
    public int DailyScheduleId { get; set; }
    public virtual DailySchedule DailySchedule { get; set; } = null!;
    public int? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    public int DutyTypeId { get; set; }
    public virtual DutyType DutyType { get; set; } = null!;
    public ServiceType ServiceType { get; set; }
    public string? Notes { get; set; }
}