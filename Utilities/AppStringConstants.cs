namespace MonthlyScheduler.Utilities;

/// <summary>
/// Application-wide string constants for UI text, messages, and labels.
/// </summary>
public static class AppStringConstants
{
    #region Form1 Constants
    
    // UI Button Text
    public const string ButtonUploadText = "Upload Spreadsheet";
    public const string ButtonGenerateText = "Generate Schedule";
    public const string ButtonViewSchedulesText = "View Saved Schedules";
    public const string ButtonViewMembersText = "View Members";
    public const string ButtonExportMembersText = "Export Members - CSV";
    public const string ButtonAddMemberText = "Add New Member";
    public const string ButtonManageDutyTypesText = "Manage Duty Types";
    public const string ButtonManageFooterTextText = "Manage PDF Footer Text";
    
    // UI Labels
    public const string LabelSelectMonth = "Select Month:";
    public const string LabelSelectYear = "Select Year:";
    
    // Form Titles
    public const string FormTitleMainScheduler = "Monthly Scheduler";
    
    // Column Names
    public const string ColumnLastNameText = "Last Name";
    public const string ColumnFirstNameText = "First Name";
    public const string ColumnFormReceivedText = "Form Received";
    public const string ColumnExcludedText = "Excluded";
    public const string ColumnEditText = "Edit";
    public const string ColumnDeleteText = "Delete";
    public const string ColumnServiceText = "Service";
    public const string ColumnDutyText = "Duty";
    
    // Dialog Titles
    public const string SuccessTitle = "Success";
    public const string ErrorTitle = "Error";
    public const string ExportCompleteTitle = "Export Complete";
    public const string ValidationErrorTitle = "Validation Error";
    public const string ConfirmDeleteTitle = "Confirm Delete";
    public const string CannotEditAssignmentTitle = "Cannot Edit Assignment";
    
    // Success Messages
    public const string ScheduleGeneratedSuccess = "Schedule generated successfully!";
    public const string MemberAddedSuccess = "Member added successfully!";
    public const string MembersExportedSuccess = "Members exported successfully!";
    
    // Error Message Formats
    public const string ErrorGeneratingScheduleFormat = "Error generating schedule: {0}";
    public const string ErrorLoadingMembersFormat = "Error loading members: {0}";
    public const string ErrorAddingMemberFormat = "Error adding member: {0}";
    public const string ErrorRefreshingMembersFormat = "Error refreshing members view: {0}";
    public const string ErrorExportingMembersFormat = "Error exporting members: {0}";
    public const string ErrorManagingDutyTypesFormat = "Error managing duty types: {0}";
    public const string ErrorUpdatingAssignmentFormat = "Error updating assignment: {0}";
    public const string ErrorViewingSavedSchedulesFormat = "Error viewing saved schedules: {0}";
    public const string ErrorDeletingMemberFormat = "Error deleting member: {0}";
    
    // Confirmation Messages
    public const string ConfirmDeleteMemberFormat = "Are you sure you want to delete {0}?";
    
    // Validation/Info Messages
    public const string MemberNotFoundError = "Member not found.";
    public const string LastSundayOnlyOccursMessage = "This duty only occurs on the last Sunday evening of the month.";
    public const string LastSundayOnlyAssignMessage = "This duty can only be assigned on the last Sunday evening of the month.";
    
    #region MemberForm Constants
    
    public const string FormTitleMemberAdd = "Add New Member";
    public const string FormTitleMemberEdit = "Edit Member";
    public const string FirstLastNameRequired = "First name and last name are required.";
    
    #endregion
    
    #region DutyTypeForm Constants
    
    public const string FormTitleDutyTypeAdd = "Add Duty Type";
    public const string FormTitleDutyTypeEdit = "Edit Duty Type";
    public const string NameRequiredMessage = "Please enter a name for the duty type.";
    public const string DescriptionRequiredMessage = "Please enter a description for the duty type.";
    
    #endregion

    #region SchedulerService Constants
    public const string SongServiceText = "Song Service";
    public const string ServiceColumnName = "Service";
    public const string DutyColumnName = "Duty";
    #endregion
        
    // Misc
    public const string ClickToAssignText = "(Double-click to assign)";
    public const string YesText = "Yes";
    public const string NoText = "No";
    
    #endregion

    // Columns that are exceptions for duplicate checking
    public static readonly string[] DuplicateExceptions = 
    [
        SongServiceText,
        ClickToAssignText
    ];
}
