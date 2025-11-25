using System.Data;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MonthlyScheduler.Services;

public class ScheduleExportService
{
    public void ExportToCSV(DataTable scheduleData, string filePath, string title)
    {
        var csv = new StringBuilder();
        
        // Add title
        csv.AppendLine(title);
        csv.AppendLine();
        
        // Add header row using LINQ
        var headers = scheduleData.Columns.Cast<DataColumn>()
            .Select(column => EscapeCSV(column.ColumnName));
        csv.AppendLine(string.Join(",", headers));
        
        // Add data rows using LINQ
        var dataRows = scheduleData.Rows.Cast<DataRow>()
            .Select(row => string.Join(",", row.ItemArray.Select(item => EscapeCSV(item?.ToString() ?? string.Empty))));
        
        foreach (var dataRow in dataRows)
        {
            csv.AppendLine(dataRow);
        }
        
        File.WriteAllText(filePath, csv.ToString());
    }
    
    public async void ExportToPDF(DataTable scheduleData, string filePath, string title)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Find Service and Duty column indices using LINQ
        const string ServiceColumn = "Service";
        const string DutyColumn = "Duty";
        
        var columnIndices = scheduleData.Columns.Cast<DataColumn>()
            .Select((col, index) => new { col.ColumnName, Index = index })
            .ToDictionary(x => x.ColumnName, x => x.Index);
        
        int serviceColumnIndex = columnIndices.TryGetValue(ServiceColumn, out var serviceIdx) ? serviceIdx : -1;
        int dutyColumnIndex = columnIndices.TryGetValue(DutyColumn, out var dutyIdx) ? dutyIdx : -1;
        
        // Load footer texts from database
        var context = Program.ServiceProvider.GetRequiredService<SchedulerDbContext>();
        var worshipFooter = await context.CategoryFooterTexts.FirstOrDefaultAsync(f => f.Category == DutyCategory.Worship);
        var avFooter = await context.CategoryFooterTexts.FirstOrDefaultAsync(f => f.Category == DutyCategory.AudioVisual);
        
        var worshipFooterText = worshipFooter?.FooterText ?? string.Empty;
        var avFooterText = avFooter?.FooterText ?? string.Empty;
        
        // Split data into worship and AV assignments
        var (worshipRows, avRows) = ParseAssignmentRows(scheduleData);
        
        Document.Create(container =>
        {
            // Page 1: Worship Assignments
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8));
                
                page.Header().Element(c => ComposeHeader(c, title, "Worship Assignments"));
                page.Content().Element(c => ComposeTable(c, scheduleData, worshipRows, serviceColumnIndex, dutyColumnIndex, worshipFooterText));
                page.Footer().Element(ComposeFooter);
            });
            
            // Page 2: AV Assignments
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8));
                
                page.Header().Element(c => ComposeHeader(c, title, "Audio-Visual Assignments"));
                page.Content().Element(c => ComposeTable(c, scheduleData, avRows, serviceColumnIndex, dutyColumnIndex, avFooterText));
                page.Footer().Element(ComposeFooter);
            });
        })
        .GeneratePdf(filePath);
    }

    private void ComposeHeader(IContainer container, string title, string pageTitle)
    {
        container.Column(column =>
        {
            column.Item().AlignLeft().Text(title).FontSize(16).Bold();
            column.Item().PaddingTop(5).AlignLeft().Text(pageTitle).FontSize(12).Bold();
        });
    }

    private void ComposeTable(IContainer container, DataTable data, List<DataRow> rows, int serviceColumnIndex, int dutyColumnIndex, string footerText)
    {
        container.Column(column =>
        {
            // Main table
            column.Item().PaddingVertical(10).Table(table =>
            {
                // Define columns (excluding Service column, Duty column wider)
                table.ColumnsDefinition(columns =>
                {
                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        if (i == serviceColumnIndex)
                            continue;
                        
                        if (i == dutyColumnIndex)
                        {
                            columns.RelativeColumn(1.5f); // Duty column 50% wider
                        }
                        else
                        {
                            columns.RelativeColumn();
                        }
                    }
                });
            
                // Header with light blue background
                table.Header(header =>
                {
                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        if (i == serviceColumnIndex)
                            continue;
                        
                        if (i == dutyColumnIndex)
                        {
                            header.Cell().Element(HeaderCellStyle).AlignLeft().Text(data.Columns[i].ColumnName).Bold();
                        }
                        else
                        {
                            header.Cell().Element(HeaderCellStyle).Text(data.Columns[i].ColumnName).Bold();
                        }
                    }
                    
                    static IContainer HeaderCellStyle(IContainer container)
                    {
                        return container.Background("#ADD8E6").Border(1).Padding(7).AlignCenter();
                    }
                });
            
                // Rows with eggshell background
                foreach (DataRow row in rows)
                {
                    var service = row["Service"]?.ToString() ?? string.Empty;
                    var isBlankRow = string.IsNullOrWhiteSpace(service);
                    var isEvening = service.Contains("Evening", StringComparison.OrdinalIgnoreCase);
                    
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        // Skip Service column
                        if (i == serviceColumnIndex)
                            continue;
                            
                        var cellText = row.ItemArray[i]?.ToString() ?? string.Empty;
                        var isDutyColumn = i == dutyColumnIndex;
                        
                        if (isBlankRow)
                        {
                            var cell = table.Cell().Element(BlankCellStyle);
                            cell.Text(cellText);
                        }
                        else
                        {
                            var cell = table.Cell().Element(NormalCellStyle);
                            if (isDutyColumn)
                                cell.AlignLeft();
                                
                            cell.Text(text =>
                            {
                                if (isEvening)
                                {
                                    text.Span(cellText).FontColor("#FF0000");
                                }
                                else
                                {
                                    text.Span(cellText);
                                }
                            });
                        }
                    }
                    
                    static IContainer NormalCellStyle(IContainer container)
                    {
                        return container.Background("#F5F5DC").Border(1).Padding(5).AlignCenter();
                    }
                    
                    static IContainer BlankCellStyle(IContainer container)
                    {
                        return container.Background("#E8E8D8").Border(1).Padding(5).AlignCenter();
                    }
                }
            });

            // Add footer text if provided
            if (!string.IsNullOrWhiteSpace(footerText))
            {
                column.Item().PaddingTop(15).Text(footerText).FontSize(9).Italic();
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignRight().Text($"Generated: {DateTime.Now:MM/dd/yyyy}");
    }
    
    private (List<DataRow> worshipRows, List<DataRow> avRows) ParseAssignmentRows(DataTable scheduleData)
    {
        var worshipRows = new List<DataRow>();
        var avRows = new List<DataRow>();
        bool inAVSection = false;
        
        // Get the AudioVisual category name from the enum
        var avCategoryName = DutyCategory.AudioVisual.ToString();
        
        foreach (DataRow row in scheduleData.Rows)
        {
            var service = row["Service"]?.ToString() ?? string.Empty;
            
            // Check if this is an AV service row by comparing with the enum value
            if (service.StartsWith(avCategoryName, StringComparison.OrdinalIgnoreCase))
            {
                inAVSection = true;
                avRows.Add(row);
            }
            // Blank rows go to the current section
            else if (string.IsNullOrWhiteSpace(service))
            {
                if (inAVSection)
                {
                    avRows.Add(row);
                }
                else
                {
                    worshipRows.Add(row);
                }
            }
            // Non-AV service rows
            else
            {
                worshipRows.Add(row);
            }
        }

        // Remove trailing blank rows from both sections
        while (worshipRows.Count > 0 && string.IsNullOrWhiteSpace(worshipRows[^1]["Service"]?.ToString()))
            worshipRows.RemoveAt(worshipRows.Count - 1);
        
        while (avRows.Count > 0 && string.IsNullOrWhiteSpace(avRows[^1]["Service"]?.ToString()))
            avRows.RemoveAt(avRows.Count - 1);
        
        return (worshipRows, avRows);
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
