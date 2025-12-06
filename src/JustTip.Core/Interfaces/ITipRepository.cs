using JustTip.Core.Entities;

namespace JustTip.Core.Interfaces;

public interface ITipRepository
{
    Task UpsertDailyTipAsync(DateOnly date, decimal amount);
    Task<IEnumerable<DailyTipPool>> GetTipsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
}
