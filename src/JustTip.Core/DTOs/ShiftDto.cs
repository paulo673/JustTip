namespace JustTip.Core.DTOs;

public record ShiftDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);
