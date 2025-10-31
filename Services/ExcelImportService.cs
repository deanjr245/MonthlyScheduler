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
        var skippedCount = 0;

        try
        {
            // Pre-load existing members for comparison
            var existingMembers = await _context.Members
                .Select(m => new { m.FirstName, m.LastName })
                .ToListAsync();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            if (!await csv.ReadAsync())
            {
                throw new InvalidOperationException("CSV file is empty");
            }

            // Load and attach all duty types from the database
            var allDutyTypes = await _context.DutyTypes.AsTracking().ToListAsync();
            _context.ChangeTracker.Clear(); // Clear any tracked entities
            
            // Reattach duty types and create dictionary using LINQ (no hard-coded strings)
            var duties = allDutyTypes.ToDictionary(d => d.Name, d => d);
            duties.Values.ToList().ForEach(d => _context.Attach(d));

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

                if (existingMembers.Any(m =>
                    m.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                    m.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase)))
                {
                    skippedCount++;
                    continue;
                }

                var member = new Member
                {
                    FirstName = firstName,
                    LastName = lastName,
                    HasSubmittedForm = csv.GetField("Form Rec")?.Trim().ToLower() == "yes",
                    ExcludeFromScheduling = csv.GetField("Exclude From Scheduling")?.Trim().ToLower() == "yes"
                };

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
                
                // Add all assigned duties to the member
                assignedDuties.ForEach(d => member.AddDuty(d!));

                newMembers.Add(member);
            }

            if (newMembers.Any())
            {
                await _context.Members.AddRangeAsync(newMembers);
                await _context.SaveChangesAsync();
            }

            var message = new List<string>();
            if (newMembers.Any())
            {
                message.Add($"Imported {newMembers.Count} new member(s)");
            }
            if (skippedCount > 0)
            {
                message.Add($"Skipped {skippedCount} existing member(s)");
            }

            if (!newMembers.Any() && skippedCount == 0)
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