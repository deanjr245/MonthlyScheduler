namespace MonthlyScheduler.Models;

public class MemberDuty
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int DutyTypeId { get; set; }
    public bool IsWilling { get; set; }
    public bool UseForScheduling { get; set; }

    // Navigation properties
    public virtual Member Member { get; set; } = null!;
    public virtual DutyType DutyType { get; set; } = null!;
}