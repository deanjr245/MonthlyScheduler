namespace MonthlyScheduler.Models;

public class WeeklySchedule
{
    public DateTime WeekStartDate { get; set; }
    public Schedule? SundaySchedule { get; set; }
    public Schedule? WednesdaySchedule { get; set; }

    public WeeklySchedule(DateTime weekStartDate)
    {
        WeekStartDate = weekStartDate.Date;
    }

    public string GetDisplayHeader()
    {
        return SundaySchedule?.Date.ToString("MMM d") ?? "";
    }
}