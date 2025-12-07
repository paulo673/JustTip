namespace JustTip.Core.DTOs;

public record ShiftDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record EmployeeRosterDto(
    int EmployeeId,
    string Name,
    IReadOnlyList<ShiftDto> Shifts,
    decimal TotalHours
);

public record WeeklyRosterDto(
    IReadOnlyList<EmployeeRosterDto> Employees
);
