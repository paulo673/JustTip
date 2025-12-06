namespace JustTip.Core.DTOs;

public record EmployeePayoutDto(string EmployeeName, decimal TotalHours, decimal PayoutAmount);

public record WeeklyPayoutDto(
    decimal TotalWeeklyTips,
    decimal TotalWeeklyHours,
    IReadOnlyList<EmployeePayoutDto> EmployeePayouts
);
