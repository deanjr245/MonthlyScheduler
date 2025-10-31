namespace MonthlyScheduler.Models;

public class DutyType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DutyCategory Category { get; set; }
    public bool IsMorningDuty { get; set; }
    public bool IsEveningDuty { get; set; }
    public bool IsWednesdayDuty { get; set; }
    public int OrderIndex { get; set; }
    public bool ExemptFromServiceMax { get; set; }
    public bool ManuallyScheduled { get; set; }
    public ManualAssignmentType? ManualAssignmentType { get; set; }
    public bool IsMonthlyDuty { get; set; }
    public MonthlyDutyFrequency? MonthlyDutyFrequency { get; set; }

    // Navigation properties
    public List<MemberDuty> MemberDuties { get; set; } = new();
    public List<DutyAssignment> DutyAssignments { get; set; } = new();
}