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
            foreach (var dutyType in allDutyTypes)
            {
                _context.Attach(dutyType); // Re-attach with unchanged state
            }
            
            var duties = new Dictionary<string, DutyType>
            {
                ["Scripture Reading"] = allDutyTypes.First(d => d.Name == "Scripture Reading"),
                ["AM Song Leading"] = allDutyTypes.First(d => d.Name == "AM Song Leading"),
                ["PM Song Leading"] = allDutyTypes.First(d => d.Name == "PM Song Leading"),
                ["Wed Song Leading"] = allDutyTypes.First(d => d.Name == "Wed Song Leading"),
                ["AM Preside at Table"] = allDutyTypes.First(d => d.Name == "AM Preside at Table"),
                ["PM Preside at Table"] = allDutyTypes.First(d => d.Name == "PM Preside at Table"),
                ["Opening Prayer"] = allDutyTypes.First(d => d.Name == "Opening Prayer"),
                ["Closing Prayer"] = allDutyTypes.First(d => d.Name == "Closing Prayer"),
                ["Foyer Security"] = allDutyTypes.First(d => d.Name == "Foyer Security"),
                ["Visitor Usher"] = allDutyTypes.First(d => d.Name == "Visitor Usher"),
                ["Sound Board Operator"] = allDutyTypes.First(d => d.Name == "Sound Board Operator"),
                ["Advance Song Slides"] = allDutyTypes.First(d => d.Name == "Advance Song Slides"),
                ["AV Booth Operator"] = allDutyTypes.First(d => d.Name == "AV Booth Operator"),
                ["Transportation"] = allDutyTypes.First(d => d.Name == "Transportation"),
            };

            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            if (headers == null || headers.Length == 0)
            {
                throw new InvalidOperationException("No headers found in CSV file");
            }
            Console.WriteLine("Headers found in CSV: " + string.Join(", ", headers));
            
            // Validate all required duty columns exist
            var missingColumns = duties.Keys.Where(duty => !headers.Contains(duty, StringComparer.OrdinalIgnoreCase)).ToList();
            if (missingColumns.Any())
            {
                Console.WriteLine($"Warning: Missing columns: {string.Join(", ", missingColumns)}");
            }

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

                foreach (var duty in duties)
                {
                    try
                    {
                        // Try to get the field value with exact header match first
                        string? rawValue = csv.GetField(duty.Key);
                        
                        // If not found, try case-insensitive match
                        if (rawValue == null)
                        {
                            var headerIndex = headers.Select((h, i) => new { Header = h, Index = i })
                                                    .FirstOrDefault(h => h.Header.Equals(duty.Key, StringComparison.OrdinalIgnoreCase));
                            if (headerIndex != null)
                            {
                                rawValue = csv.GetField(headerIndex.Index);
                            }
                        }

                        Console.WriteLine($"Raw value for {duty.Key}: '{rawValue}'");
                        
                        if (!string.IsNullOrWhiteSpace(rawValue))
                        {
                            var value = rawValue.Trim().ToLower();
                            if (value == "yes" || value == "y" || value == "true" || value == "1")
                            {
                                member.AddDuty(duty.Value);
                                Console.WriteLine($"Added duty {duty.Key} for {member.FirstName} {member.LastName}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No value found for {duty.Key}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading {duty.Key}: {ex.Message}");
                    }
                }

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