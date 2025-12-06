namespace JustTip.Core.DTOs;

public record CreateShiftRequest(
    int EmployeeId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime
);
