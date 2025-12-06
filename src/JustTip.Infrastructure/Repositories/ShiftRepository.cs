using JustTip.Core.Entities;
using JustTip.Core.Interfaces;
using JustTip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Repositories;

public class ShiftRepository(AppDbContext context) : IShiftRepository
{
    public async Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await context.Shifts
            .Include(s => s.Employee)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Shift?> GetByIdAsync(int id)
    {
        return await context.Shifts
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> HasOverlappingShiftAsync(
        int employeeId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeShiftId = null)
    {
        var query = context.Shifts
            .Where(s => s.EmployeeId == employeeId && s.Date == date);

        if (excludeShiftId.HasValue)
        {
            query = query.Where(s => s.Id != excludeShiftId.Value);
        }

        return await query.AnyAsync(s =>
            (startTime < s.EndTime && endTime > s.StartTime)
        );
    }

    public async Task<Shift> AddAsync(Shift shift)
    {
        context.Shifts.Add(shift);
        await context.SaveChangesAsync();
        return shift;
    }

    public async Task UpdateAsync(Shift shift)
    {
        context.Shifts.Update(shift);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Shift shift)
    {
        context.Shifts.Remove(shift);
        await context.SaveChangesAsync();
    }
}
