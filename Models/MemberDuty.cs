namespace MonthlyScheduler.Models;

public class MemberDuty
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int DutyTypeId { get; set; }
    public bool IsWilling { get; set; }
    public bool UseForScheduling { get; set; }

    // Navigation properties
    public Member Member { get; set; } = null!;
    public DutyType DutyType { get; set; } = null!;
}