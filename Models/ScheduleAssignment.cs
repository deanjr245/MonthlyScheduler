using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class ScheduleAssignment
{
    public int Id { get; set; }
    public int DailyScheduleId { get; set; }
    public DailySchedule DailySchedule { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int DutyTypeId { get; set; }
    public DutyType DutyType { get; set; } = null!;
    public ServiceType ServiceType { get; set; }
}