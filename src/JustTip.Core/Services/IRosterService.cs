using JustTip.Core.DTOs;

namespace JustTip.Core.Services;

public interface IRosterService
{
    Task<IEnumerable<ShiftDto>> GetWeeklyRosterAsync(DateOnly startDate);
    Task<ShiftDto?> GetShiftByIdAsync(int id);
    Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request);
    Task<ShiftDto> UpdateShiftAsync(int id, UpdateShiftRequest request);
    Task DeleteShiftAsync(int id);
}
