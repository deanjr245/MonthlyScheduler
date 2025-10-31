using System.Text;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Services;

public class MemberExportService
{
    public async Task ExportMembersToCSV(SchedulerDbContext context, string filePath)
    {
        var members = await context.Members
            .Include(m => m.AvailableDuties)
            .ThenInclude(d => d.DutyType)
            .OrderBy(m => m.ExcludeFromScheduling)
            .ThenBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();

        var duties = await context.DutyTypes
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();

        var csv = new StringBuilder();
        
        // Add title
        csv.AppendLine("Member List Export");
        csv.AppendLine();
        
        // Add header row
        var headers = new List<string>
        {
            "Last Name",
            "First Name",
            "Form Received"
        };
        
        foreach (var duty in duties)
        {
            headers.Add(duty.Name);
        }
        
        headers.Add("Excluded");
        
        csv.AppendLine(string.Join(",", headers.Select(EscapeCSV)));
        
        // Add data rows using LINQ
        var dataRows = members.Select(member =>
        {
            var memberDutyIds = member.AvailableDuties.Select(d => d.DutyTypeId).ToHashSet();
            var values = new List<string>
            {
                EscapeCSV(member.LastName),
                EscapeCSV(member.FirstName),
                member.HasSubmittedForm ? "Yes" : "No"
            };
            
            values.AddRange(duties.Select(duty => memberDutyIds.Contains(duty.Id) ? "Yes" : ""));
            values.Add(member.ExcludeFromScheduling ? "Yes" : "No");
            
            return string.Join(",", values);
        });
        
        foreach (var row in dataRows)
        {
            csv.AppendLine(row);
        }
        
        File.WriteAllText(filePath, csv.ToString());
    }
    
    private string EscapeCSV(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
