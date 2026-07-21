using ClosedXML.Excel;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Services;

public class MemberExportService
{
    private const string DefaultSheetName = "Members";

    public async Task ExportMembers(SchedulerDbContext context, string filePath, string sheetName = DefaultSheetName)
    {
        var members = await LoadMembers(context);
        var duties = await LoadDuties(context);

        WriteWorkbook(filePath, members, duties, sheetName);
    }

    private void WriteWorkbook(string filePath, List<Member> members, List<DutyType> duties, string sheetName)
    {
        var safeSheetName = string.IsNullOrWhiteSpace(sheetName) ? DefaultSheetName : sheetName;
        safeSheetName = safeSheetName.Length > 31 ? safeSheetName[..31] : safeSheetName;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(safeSheetName);

        var headers = new List<string>
        {
            "Last Name",
            "First Name",
            "Form Received"
        };

        headers.AddRange(duties.Select(duty => duty.Name));
        headers.Add("Excluded");

        for (var index = 0; index < headers.Count; index++)
        {
            worksheet.Cell(1, index + 1).Value = headers[index];
        }

        for (var rowIndex = 0; rowIndex < members.Count; rowIndex++)
        {
            var member = members[rowIndex];
            var memberDutyIds = member.AvailableDuties.Select(d => d.DutyTypeId).ToHashSet();
            var row = rowIndex + 2;

            worksheet.Cell(row, 1).Value = member.LastName;
            worksheet.Cell(row, 2).Value = member.FirstName;
            worksheet.Cell(row, 3).Value = member.HasSubmittedForm ? "Yes" : "No";

            for (var dutyIndex = 0; dutyIndex < duties.Count; dutyIndex++)
            {
                worksheet.Cell(row, 4 + dutyIndex).Value = memberDutyIds.Contains(duties[dutyIndex].Id) ? "Yes" : string.Empty;
            }

            worksheet.Cell(row, 4 + duties.Count).Value = member.ExcludeFromScheduling ? "Yes" : "No";
        }

        workbook.SaveAs(filePath);
    }

    private async Task<List<Member>> LoadMembers(SchedulerDbContext context)
    {
        return await context.Members
            .Include(m => m.AvailableDuties)
            .ThenInclude(d => d.DutyType)
            .OrderBy(m => m.ExcludeFromScheduling)
            .ThenBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();
    }

    private async Task<List<DutyType>> LoadDuties(SchedulerDbContext context)
    {
        return await context.DutyTypes
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }
}
