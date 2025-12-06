using JustTip.Core.Entities;
using JustTip.Core.Interfaces;
using JustTip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Repositories;

public class TipRepository(AppDbContext context) : ITipRepository
{
    public async Task UpsertDailyTipAsync(DateOnly date, decimal amount)
    {
        var existingTip = await context.DailyTipPools
            .FirstOrDefaultAsync(t => t.Date == date);

        if (existingTip is not null)
        {
            existingTip.Amount = amount;
        }
        else
        {
            context.DailyTipPools.Add(new DailyTipPool
            {
                Date = date,
                Amount = amount
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DailyTipPool>> GetTipsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await context.DailyTipPools
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync();
    }
}
