using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public class GeneratedSchedule
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime GeneratedDate { get; set; }
    public List<DailySchedule> DailySchedules { get; set; } = new();
}