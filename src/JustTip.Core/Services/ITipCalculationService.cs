using JustTip.Core.DTOs;

namespace JustTip.Core.Services;

public interface ITipCalculationService
{
    Task<WeeklyPayoutDto> CalculateWeeklyPayoutAsync(DateOnly weekStart);
    Task SaveDailyTipsAsync(IEnumerable<DailyTipInputDto> tips);
}
