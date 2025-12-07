using JustTip.Core.DTOs;
using JustTip.Core.Entities;
using JustTip.Core.Exceptions;
using JustTip.Core.Interfaces;
using JustTip.Core.Services;
using NSubstitute;

namespace JustTip.Tests.Services;

public class RosterServiceOverlappingTests
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly RosterService _sut;

    private readonly Employee _testEmployee = new("John Doe", 1);
    private readonly DateOnly _testDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    public RosterServiceOverlappingTests()
    {
        _shiftRepository = Substitute.For<IShiftRepository>();
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _sut = new RosterService(_shiftRepository, _employeeRepository);

        _employeeRepository.GetByIdAsync(_testEmployee.Id).Returns(_testEmployee);
    }

    [Fact]
    public async Task CreateShiftAsync_WhenNoOverlap_ShouldCreateShift()
    {
        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, request.StartTime, request.EndTime, null
        ).Returns(false);

        _shiftRepository.AddAsync(Arg.Any<Shift>()).Returns(callInfo =>
        {
            var shift = callInfo.Arg<Shift>();
            return new Shift(shift.EmployeeId, shift.Date, shift.StartTime, shift.EndTime, 1);
        });

        var result = await _sut.CreateShiftAsync(request);

        Assert.NotNull(result);
        Assert.Equal(_testEmployee.Id, result.EmployeeId);
        await _shiftRepository.Received(1).AddAsync(Arg.Any<Shift>());
    }

    [Fact]
    public async Task CreateShiftAsync_WhenOverlapExists_ShouldThrowShiftOverlapException()
    {
        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, request.StartTime, request.EndTime, null
        ).Returns(true);

        await Assert.ThrowsAsync<ShiftOverlapException>(() => _sut.CreateShiftAsync(request));
        await _shiftRepository.DidNotReceive().AddAsync(Arg.Any<Shift>());
    }

    [Fact]
    public async Task UpdateShiftAsync_WhenNoOverlap_ShouldUpdateShift()
    {
        var shiftId = 1;
        var existingShift = new Shift(
            _testEmployee.Id,
            _testDate,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            shiftId,
            _testEmployee
        );

        var request = new UpdateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(13, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _shiftRepository.GetByIdAsync(shiftId).Returns(existingShift);
        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, request.StartTime, request.EndTime, shiftId
        ).Returns(false);

        var result = await _sut.UpdateShiftAsync(shiftId, request);

        Assert.NotNull(result);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.EndTime, result.EndTime);
        await _shiftRepository.Received(1).UpdateAsync(existingShift);
    }

    [Fact]
    public async Task UpdateShiftAsync_WhenOverlapExists_ShouldThrowShiftOverlapException()
    {
        var shiftId = 1;
        var existingShift = new Shift(
            _testEmployee.Id,
            _testDate,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            shiftId,
            _testEmployee
        );

        var request = new UpdateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(18, 0)
        );

        _shiftRepository.GetByIdAsync(shiftId).Returns(existingShift);
        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, request.StartTime, request.EndTime, shiftId
        ).Returns(true);

        await Assert.ThrowsAsync<ShiftOverlapException>(() => _sut.UpdateShiftAsync(shiftId, request));
        await _shiftRepository.DidNotReceive().UpdateAsync(Arg.Any<Shift>());
    }

    [Fact]
    public async Task UpdateShiftAsync_WhenUpdatingSameShiftWithSameTime_ShouldExcludeItselfFromOverlapCheck()
    {
        var shiftId = 1;
        var existingShift = new Shift(
            _testEmployee.Id,
            _testDate,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            shiftId,
            _testEmployee
        );

        var request = new UpdateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _shiftRepository.GetByIdAsync(shiftId).Returns(existingShift);
        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, request.StartTime, request.EndTime, shiftId
        ).Returns(false);

        var result = await _sut.UpdateShiftAsync(shiftId, request);

        await _shiftRepository.Received(1).HasOverlappingShiftAsync(
            _testEmployee.Id,
            _testDate,
            request.StartTime,
            request.EndTime,
            shiftId
        );
    }

    [Fact]
    public async Task CreateShiftAsync_WhenStartTimeEqualsEndTime_ShouldThrowArgumentException()
    {
        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(9, 0)
        );

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateShiftAsync(request));
        Assert.Contains("StartTime must be before EndTime", exception.Message);
    }

    [Fact]
    public async Task CreateShiftAsync_WhenStartTimeAfterEndTime_ShouldThrowArgumentException()
    {
        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: new TimeOnly(17, 0),
            EndTime: new TimeOnly(9, 0)
        );

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateShiftAsync(request));
        Assert.Contains("StartTime must be before EndTime", exception.Message);
    }

    [Fact]
    public async Task CreateShiftAsync_WhenEmployeeNotFound_ShouldThrowArgumentException()
    {
        var nonExistentEmployeeId = 999;
        var request = new CreateShiftRequest(
            EmployeeId: nonExistentEmployeeId,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _employeeRepository.GetByIdAsync(nonExistentEmployeeId).Returns((Employee?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateShiftAsync(request));
        Assert.Contains($"Employee with ID {nonExistentEmployeeId} not found", exception.Message);
    }
}
