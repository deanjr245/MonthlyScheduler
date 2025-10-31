using System.ComponentModel.DataAnnotations;

namespace MonthlyScheduler.Models;

public enum ManualAssignmentType
{
    [Display(Name = "Manual Assignment")]
    MemberSelection,
    
    [Display(Name = "Fill-in Text Assignment")]
    TextInput
}
