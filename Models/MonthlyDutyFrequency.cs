using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public enum MonthlyDutyFrequency
{
    [Display(Name = "Start of Month")]
    StartOfMonth,
    
    [Display(Name = "Each Week")]
    EachWeek,
    
    [Display(Name = "End of Month")]
    EndOfMonth
}
