using MonthlyScheduler.Models;
using MonthlyScheduler.Data;
using MonthlyScheduler.Exceptions;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace MonthlyScheduler.Services;

public class ExcelImportService
{
    private readonly SchedulerDbContext _context;

    public ExcelImportService(SchedulerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Required CSV columns:
    ///   1. Last Name        (first column)
    ///   2. First Name       (second column)
    ///   3. Form Rec         (optional header column; use Yes/No)
    ///   4. Exclude From Scheduling (optional header column; use Yes/No)
    ///   5. AM Scripture Reading
    ///   6. AM Song Leading
    ///   7. PM Song Leading
    ///   8. Wed Song Leading
    ///   9. AM Preside at Table
    ///  10. PM Preside at Table
    ///  11. Opening Prayer
    ///  12. Closing Prayer
    ///  13. Foyer Security
    ///  14. Visitor Usher
    ///  15. Sound Board Operator
    ///  16. Advance Song Slides
    ///  17. AV Booth Operator
    ///  18. Transportation
    ///  19. PM Scripture Reading
    ///  20. Backup Booth Operator
    ///  21. Wednesday Invitation
    ///  22. Monthly Song Service Leader
    ///  23. Building Closing
    ///  24. Visitor Engagement
    /// 
    /// For duty columns, any of the following values will count as selected: Yes, Y, True, 1.
    /// Notes:
    ///   - First and last name are read by position, not by header name.
    ///   - Duty columns are matched by header name case-insensitively against the existing DutyType names.
    ///   - Existing members with the same first and last name are skipped.
    /// </summary>
    public async Task ImportMembersFromExcel(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified CSV file was not found.", filePath);
        }
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        var newMembers = new List<Member>();
        var updatedCount = 0;

        try
        {
            // Pre-load existing members with their duties for comparison and update
            var existingMembers = await _context.Members
                .Include(m => m.AvailableDuties)
                .ToListAsync();

            var existingMemberLookup = existingMembers
                .ToDictionary(
                    m => (m.FirstName.Trim() + "|" + m.LastName.Trim()).ToLowerInvariant(),
                    m => m);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            if (!await csv.ReadAsync())
            {
                throw new InvalidOperationException("CSV file is empty");
            }

            // Load all duty types from the database
            var allDutyTypes = await _context.DutyTypes.ToListAsync();
            var duties = allDutyTypes.ToDictionary(d => d.Name, d => d);

            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            if (headers == null || headers.Length == 0)
            {
                throw new InvalidOperationException("No headers found in CSV file");
            }
            
            // Create a case-insensitive lookup for headers using LINQ
            var headerLookup = headers
                .Select((h, i) => new { Header = h, Index = i })
                .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

            while (await csv.ReadAsync())
            {
                var lastName = csv.GetField(0)?.Trim();
                var firstName = csv.GetField(1)?.Trim();

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    continue;
                }

                var memberKey = (firstName + "|" + lastName).ToLowerInvariant();
                existingMemberLookup.TryGetValue(memberKey, out var existingMember);

                var member = existingMember ?? new Member
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                member.HasSubmittedForm = csv.GetField("Form Rec")?.Trim().ToLower() == "yes";
                member.ExcludeFromScheduling = csv.GetField("Exclude From Scheduling")?.Trim().ToLower() == "yes";

                // Process duties using LINQ where possible
                var validValues = new HashSet<string> { "yes", "y", "true", "1" };
                
                var assignedDuties = duties
                    .Where(duty => headerLookup.ContainsKey(duty.Key))
                    .Select(duty =>
                    {
                        try
                        {
                            var rawValue = csv.GetField(headerLookup[duty.Key]);
                            
                            if (!string.IsNullOrWhiteSpace(rawValue))
                            {
                                var value = rawValue.Trim().ToLower();
                                if (validValues.Contains(value))
                                {
                                    return duty.Value;
                                }
                            }
                        }
                        catch
                        {
                            // Skip duties that can't be read
                        }
                        return null;
                    })
                    .Where(d => d != null)
                    .ToList();

                var assignedDutyIds = assignedDuties.Select(d => d!.Id).ToHashSet();

                // Update existing duties or add new ones
                var existingDutyLookup = member.AvailableDuties.ToDictionary(d => d.DutyTypeId);

                // Ensure assigned duties are present
                foreach (var duty in assignedDuties)
                {
                    if (duty == null)
                    {
                        continue;
                    }

                    if (existingDutyLookup.TryGetValue(duty.Id, out var currentDuty))
                    {
                        currentDuty.IsWilling = true;
                        currentDuty.UseForScheduling = true;
                    }
                    else
                    {
                        member.AddDuty(duty, isWilling: true, useForScheduling: true);
                    }
                }

                // Remove duties that are no longer selected in the CSV import
                var dutiesToRemove = member.AvailableDuties
                    .Where(md => !assignedDutyIds.Contains(md.DutyTypeId))
                    .ToList();

                foreach (var dutyToRemove in dutiesToRemove)
                {
                    member.AvailableDuties.Remove(dutyToRemove);
                    _context.Entry(dutyToRemove).State = EntityState.Deleted;
                }

                if (existingMember == null)
                {
                    newMembers.Add(member);
                    existingMemberLookup[memberKey] = member;
                }
                else
                {
                    updatedCount++;
                }
            }

            if (newMembers.Any())
            {
                await _context.Members.AddRangeAsync(newMembers);
            }

            if (newMembers.Any() || updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            var message = new List<string>();
            if (newMembers.Any())
            {
                message.Add($"Imported {newMembers.Count} new member(s)");
            }
            if (updatedCount > 0)
            {
                message.Add($"Updated {updatedCount} existing member(s)");
            }

            if (!newMembers.Any() && updatedCount == 0)
            {
                throw new InvalidOperationException("No valid members found in the CSV file.");
            }

            throw new ImportResultException(string.Join(", ", message));
        }
        catch (ImportResultException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error processing CSV file: {ex.Message}", ex);
        }
    }
}