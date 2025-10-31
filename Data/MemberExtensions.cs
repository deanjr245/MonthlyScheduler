using Microsoft.EntityFrameworkCore;
using MonthlyScheduler.Models;

namespace MonthlyScheduler.Data;

public static class MemberExtensions
{
    public static async Task UpdateMemberAsync(this SchedulerDbContext context, Member member)
    {
        // If the member is already being tracked, reload it to avoid tracking conflicts
        if (context.Entry(member).State != EntityState.Detached)
        {
            await context.Entry(member).ReloadAsync();
        }

        // Remove any duties that are no longer checked
        var dutiestoRemove = member.AvailableDuties
            .Where(d => !member.AvailableDuties.Any(nd => nd.DutyType == d.DutyType))
            .ToList();

        foreach (var duty in dutiestoRemove)
        {
            member.AvailableDuties.Remove(duty);
        }

        // Update the member
        context.Update(member);
        await context.SaveChangesAsync();
    }
}