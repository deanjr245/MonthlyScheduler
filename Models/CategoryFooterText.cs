namespace MonthlyScheduler.Models;

public class CategoryFooterText
{
    public int Id { get; set; }
    public DutyCategory Category { get; set; }
    public string FooterText { get; set; } = string.Empty;
}
