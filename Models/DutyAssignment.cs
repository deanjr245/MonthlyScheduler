using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class DutyAssignment
{
    [Key]
    public int Id { get; set; }
    public int ServiceScheduleId { get; set; }
    public int MemberId { get; set; }
    public int DutyTypeId { get; set; }
    public int? AssignmentCategoryId { get; set; }

    // Navigation properties
    public virtual ServiceSchedule ServiceSchedule { get; set; } = null!;
    public virtual Member Member { get; set; } = null!;
    public virtual DutyType DutyType { get; set; } = null!;
    public virtual AssignmentCategory? AssignmentCategory { get; set; }
}