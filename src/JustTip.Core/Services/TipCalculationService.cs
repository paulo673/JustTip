using JustTip.Core.DTOs;
using JustTip.Core.Interfaces;

namespace JustTip.Core.Services;

public class TipCalculationService(IShiftRepository shiftRepository, ITipRepository tipRepository)
    : ITipCalculationService
{
    public async Task<WeeklyPayoutDto> CalculateWeeklyPayoutAsync(DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        var shifts = (await shiftRepository.GetShiftsByDateRangeAsync(weekStart, weekEnd)).ToList();
        var tips = (await tipRepository.GetTipsByDateRangeAsync(weekStart, weekEnd)).ToList();

        var totalWeeklyTips = tips.Sum(t => t.Amount);
        var totalWeeklyHours = (decimal)shifts.Sum(s => s.DurationInHours);

        var tipsByDate = tips.ToDictionary(t => t.Date, t => t.Amount);

        var employeePayouts = new Dictionary<string, (decimal Hours, decimal Payout)>();

        var shiftsByDate = shifts.GroupBy(s => s.Date);

        foreach (var dayGroup in shiftsByDate)
        {
            var date = dayGroup.Key;
            var dayShifts = dayGroup.ToList();

            var dailyTips = tipsByDate.GetValueOrDefault(date, 0m);

            var totalHoursForDay = (decimal)dayShifts.Sum(s => s.DurationInHours);

            if (totalHoursForDay == 0)
            {
                continue;
            }

            var hourlyRate = dailyTips / totalHoursForDay;

            var employeeHoursForDay = dayShifts
                .GroupBy(s => s.Employee?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => (decimal)g.Sum(s => s.DurationInHours));

            foreach (var (employeeName, hours) in employeeHoursForDay)
            {
                var dayPayout = hourlyRate * hours;

                if (employeePayouts.TryGetValue(employeeName, out var existing))
                {
                    employeePayouts[employeeName] = (existing.Hours + hours, existing.Payout + dayPayout);
                }
                else
                {
                    employeePayouts[employeeName] = (hours, dayPayout);
                }
            }
        }

        var payoutList = employeePayouts
            .Select(kvp => new EmployeePayoutDto(
                kvp.Key,
                kvp.Value.Hours,
                Math.Round(kvp.Value.Payout, 2)
            ))
            .OrderBy(p => p.EmployeeName)
            .ToList();

        return new WeeklyPayoutDto(totalWeeklyTips, totalWeeklyHours, payoutList);
    }

    public async Task SaveDailyTipsAsync(IEnumerable<DailyTipInputDto> tips)
    {
        foreach (var tip in tips)
        {
            await tipRepository.UpsertDailyTipAsync(tip.Date, tip.Amount);
        }
    }
}
