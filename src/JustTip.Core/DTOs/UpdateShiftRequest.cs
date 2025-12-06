namespace JustTip.Core.DTOs;

public record UpdateShiftRequest(
    int EmployeeId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);
