using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Data;
using MonthlyScheduler.Models;

namespace MonthlyScheduler.Services;

public class DutyTypeService
{
    private readonly SchedulerDbContext _context;

    public DutyTypeService(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<List<DutyType>> GetAllDutyTypesAsync()
    {
        return await _context.DutyTypes
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }

    public async Task<List<DutyType>> GetMorningDutiesAsync()
    {
        return await _context.DutyTypes
            .Where(dt => dt.IsMorningDuty)
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }

    public async Task<List<DutyType>> GetEveningDutiesAsync()
    {
        return await _context.DutyTypes
            .Where(dt => dt.IsEveningDuty)
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }

    public async Task<List<DutyType>> GetWednesdayDutiesAsync()
    {
        return await _context.DutyTypes
            .Where(dt => dt.IsWednesdayDuty)
            .OrderBy(dt => dt.Category)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }

    public async Task<List<DutyType>> GetDutiesByCategory(DutyCategory category)
    {
        return await _context.DutyTypes
            .Where(dt => dt.Category == category)
            .OrderBy(dt => dt.Name)
            .ToListAsync();
    }

    public async Task<DutyType?> GetDutyTypeByIdAsync(int id)
    {
        return await _context.DutyTypes.FindAsync(id);
    }

    public async Task<DutyType> CreateDutyTypeAsync(DutyType dutyType)
    {
        _context.DutyTypes.Add(dutyType);
        await _context.SaveChangesAsync();
        return dutyType;
    }

    public async Task UpdateDutyTypeAsync(DutyType dutyType)
    {
        _context.Entry(dutyType).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDutyTypeAsync(int id)
    {
        var dutyType = await _context.DutyTypes.FindAsync(id);
        if (dutyType != null)
        {
            _context.DutyTypes.Remove(dutyType);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsDutyTypeInUseAsync(int id)
    {
        return await _context.MemberDuties.AnyAsync(md => md.DutyTypeId == id) ||
               await _context.DutyAssignments.AnyAsync(da => da.DutyTypeId == id);
    }
}