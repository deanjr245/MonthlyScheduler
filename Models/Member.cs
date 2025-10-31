using System.Collections.ObjectModel;

namespace MonthlyScheduler.Models;

public class Member
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool HasSubmittedForm { get; set; }
    public bool ExcludeFromScheduling { get; set; }
    // Navigation properties
    public List<MemberDuty> AvailableDuties { get; set; } = new();
    public List<DutyAssignment> Assignments { get; set; } = new();

    public Member()
    {
        // Required by EF Core
    }

    public Member(string firstName, string lastName) : this()
    {
        FirstName = firstName;
        LastName = lastName;
        HasSubmittedForm = false;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void AddDuty(DutyType dutyType)
    {
        if (!AvailableDuties.Any(d => d.DutyTypeId == dutyType.Id))
        {
            var memberDuty = new MemberDuty 
            { 
                MemberId = Id,
                Member = this,
                DutyTypeId = dutyType.Id, 
                DutyType = dutyType 
            };
            AvailableDuties.Add(memberDuty);
        }
    }

    public void RemoveDuty(DutyType dutyType)
    {
        var memberDuty = AvailableDuties.FirstOrDefault(d => d.DutyTypeId == dutyType.Id);
        if (memberDuty != null)
        {
            AvailableDuties.Remove(memberDuty);
        }
    }

    public bool IsAvailableForDuty(DutyType dutyType)
    {
        return AvailableDuties.Any(d => d.DutyTypeId == dutyType.Id);
    }

    public void ClearDuties()
    {
        AvailableDuties.Clear();
    }

    public void UpdateAvailableDuties(IEnumerable<DutyType> dutyTypes)
    {
        AvailableDuties = dutyTypes.Select(dt => new MemberDuty 
        { 
            MemberId = Id, 
            Member = this,
            DutyTypeId = dt.Id, 
            DutyType = dt 
        }).ToList();
    }
}