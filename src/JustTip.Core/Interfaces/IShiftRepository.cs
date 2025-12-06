using JustTip.Core.Entities;

namespace JustTip.Core.Interfaces;

public interface IShiftRepository
{
    Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<Shift?> GetByIdAsync(int id);
    Task<bool> HasOverlappingShiftAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeShiftId = null);
    Task<Shift> AddAsync(Shift shift);
    Task UpdateAsync(Shift shift);
    Task DeleteAsync(Shift shift);
}
