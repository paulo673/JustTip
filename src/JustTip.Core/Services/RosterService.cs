using JustTip.Core.DTOs;
using JustTip.Core.Entities;
using JustTip.Core.Exceptions;
using JustTip.Core.Interfaces;

namespace JustTip.Core.Services;

public class RosterService(IShiftRepository shiftRepository, IEmployeeRepository employeeRepository)
    : IRosterService
{
    public async Task<IEnumerable<ShiftDto>> GetWeeklyRosterAsync(DateOnly startDate)
    {
        var endDate = startDate.AddDays(6);
        var shifts = await shiftRepository.GetShiftsByDateRangeAsync(startDate, endDate);

        return shifts.Select(s => new ShiftDto(
            s.Id,
            s.EmployeeId,
            s.Employee?.Name ?? string.Empty,
            s.Date,
            s.StartTime,
            s.EndTime
        ));
    }

    public async Task<ShiftDto?> GetShiftByIdAsync(int id)
    {
        var shift = await shiftRepository.GetByIdAsync(id);

        if (shift is null)
        {
            return null;
        }

        return new ShiftDto(
            shift.Id,
            shift.EmployeeId,
            shift.Employee?.Name ?? string.Empty,
            shift.Date,
            shift.StartTime,
            shift.EndTime
        );
    }

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request)
    {
        ValidateTimeRange(request.StartTime, request.EndTime);
        ValidateNotRetroactive(request.Date);

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId)
            ?? throw new ArgumentException($"Employee with ID {request.EmployeeId} not found.");

        var hasOverlap = await shiftRepository.HasOverlappingShiftAsync(
            request.EmployeeId,
            request.Date,
            request.StartTime,
            request.EndTime
        );

        if (hasOverlap)
        {
            throw new ShiftOverlapException();
        }

        var shift = new Shift(request.EmployeeId, request.Date, request.StartTime, request.EndTime);

        var createdShift = await shiftRepository.AddAsync(shift);

        return new ShiftDto(
            createdShift.Id,
            createdShift.EmployeeId,
            employee.Name,
            createdShift.Date,
            createdShift.StartTime,
            createdShift.EndTime
        );
    }

    public async Task<ShiftDto> UpdateShiftAsync(int id, UpdateShiftRequest request)
    {
        ValidateTimeRange(request.StartTime, request.EndTime);

        var shift = await shiftRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Shift with ID {id} not found.");

        ValidateNotRetroactive(shift.Date);
        ValidateNotRetroactive(request.Date);

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId)
            ?? throw new ArgumentException($"Employee with ID {request.EmployeeId} not found.");

        var hasOverlap = await shiftRepository.HasOverlappingShiftAsync(
            request.EmployeeId,
            request.Date,
            request.StartTime,
            request.EndTime,
            excludeShiftId: id
        );

        if (hasOverlap)
        {
            throw new ShiftOverlapException();
        }

        shift.Update(request.EmployeeId, request.Date, request.StartTime, request.EndTime);

        await shiftRepository.UpdateAsync(shift);

        return new ShiftDto(
            shift.Id,
            shift.EmployeeId,
            employee.Name,
            shift.Date,
            shift.StartTime,
            shift.EndTime
        );
    }

    public async Task DeleteShiftAsync(int id)
    {
        var shift = await shiftRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Shift with ID {id} not found.");

        ValidateNotRetroactive(shift.Date);

        await shiftRepository.DeleteAsync(shift);
    }

    private static void ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new ArgumentException("StartTime must be before EndTime.");
        }
    }

    private static void ValidateNotRetroactive(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date < today)
        {
            throw new RetroactiveShiftException();
        }
    }
}
