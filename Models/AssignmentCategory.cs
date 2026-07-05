namespace MonthlyScheduler.Models;

public class AssignmentCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int MaxAssignmentsPerMonth { get; set; } = 1;

    public List<DutyType> DutyTypes { get; set; } = new();
    public List<ScheduleAssignment> ScheduleAssignments { get; set; } = new();
    public List<DutyAssignment> DutyAssignments { get; set; } = new();
}
