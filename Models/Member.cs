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
    public virtual List<MemberDuty> AvailableDuties { get; set; } = new();
    public virtual List<DutyAssignment> Assignments { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}";

    public void AddDuty(DutyType dutyType, bool isWilling = true, bool useForScheduling = true)
    {
        if (!isWilling)
        {
            useForScheduling = false;
        }

        var existing = AvailableDuties.FirstOrDefault(d => d.DutyTypeId == dutyType.Id);
        if (existing == null)
        {
            var memberDuty = new MemberDuty
            {
                MemberId = Id,
                Member = this,
                DutyTypeId = dutyType.Id,
                DutyType = dutyType,
                IsWilling = isWilling,
                UseForScheduling = useForScheduling
            };
            AvailableDuties.Add(memberDuty);
            return;
        }

        existing.IsWilling = isWilling;
        existing.UseForScheduling = useForScheduling && isWilling;
    }

    public bool IsAvailableForDuty(DutyType dutyType)
    {
        return AvailableDuties.Any(d => d.DutyTypeId == dutyType.Id && d.IsWilling && d.UseForScheduling);
    }

    public bool IsWillingForDuty(DutyType dutyType)
    {
        return AvailableDuties.Any(d => d.DutyTypeId == dutyType.Id && d.IsWilling);
    }
}