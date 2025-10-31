using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class ServiceSchedule
{
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public Service Service { get; set; }

    // Navigation property for assignments
    public List<DutyAssignment> Assignments { get; set; } = new();

    public ServiceSchedule()
    {
        // Required by EF Core
    }

    public ServiceSchedule(DateTime date, Service service)
    {
        Date = date;
        Service = service;
    }

    public bool CanAssignMember(Member member)
    {
        // Member can't be assigned if they're already assigned to any duty in this service
        return !Assignments.Any(a => a.MemberId == member.Id);
    }

    public bool AssignDuty(DutyType dutyType, Member member)
    {
        if (!member.IsAvailableForDuty(dutyType) || !CanAssignMember(member))
        {
            return false;
        }

        var assignment = new DutyAssignment
        {
            ServiceScheduleId = Id,
            MemberId = member.Id,
            DutyTypeId = dutyType.Id,
            DutyType = dutyType
        };
        Assignments.Add(assignment);
        return true;
    }

    public Member? GetAssignedMember(DutyType dutyType)
    {
        var assignment = Assignments.FirstOrDefault(a => a.DutyTypeId == dutyType.Id);
        return assignment?.Member;
    }
}