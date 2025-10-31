using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class DutyAssignment
{
    [Key]
    public int Id { get; set; }
    public int ServiceScheduleId { get; set; }
    public int MemberId { get; set; }
    public int DutyTypeId { get; set; }

    // Navigation properties
    public ServiceSchedule ServiceSchedule { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public DutyType DutyType { get; set; } = null!;
}