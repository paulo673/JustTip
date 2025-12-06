using JustTip.Core.DTOs;
using JustTip.Core.Entities;
using JustTip.Core.Exceptions;
using JustTip.Core.Interfaces;
using JustTip.Core.Services;
using NSubstitute;

namespace JustTip.Tests.Services;

public class ShiftOverlapScenariosTests
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly RosterService _sut;

    private readonly Employee _testEmployee = new() { Id = 1, Name = "John Doe" };
    private readonly DateOnly _testDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    public ShiftOverlapScenariosTests()
    {
        _shiftRepository = Substitute.For<IShiftRepository>();
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _sut = new RosterService(_shiftRepository, _employeeRepository);

        _employeeRepository.GetByIdAsync(_testEmployee.Id).Returns(_testEmployee);
    }

    [Theory]
    [InlineData(8, 0, 10, 0, true, "New shift ends during existing shift")]
    [InlineData(11, 0, 14, 0, true, "New shift starts during existing shift")]
    [InlineData(10, 0, 11, 0, true, "New shift completely inside existing shift")]
    [InlineData(8, 0, 14, 0, true, "New shift completely covers existing shift")]
    [InlineData(9, 0, 12, 0, true, "New shift exactly matches existing shift")]
    [InlineData(6, 0, 9, 0, false, "New shift ends exactly when existing starts (adjacent - no overlap)")]
    [InlineData(12, 0, 15, 0, false, "New shift starts exactly when existing ends (adjacent - no overlap)")]
    [InlineData(6, 0, 8, 0, false, "New shift completely before existing shift")]
    [InlineData(14, 0, 18, 0, false, "New shift completely after existing shift")]
    public async Task CreateShiftAsync_OverlapScenarios_ShouldBehaveCorrectly(
        int newStartHour, int newStartMinute,
        int newEndHour, int newEndMinute,
        bool shouldOverlap,
        string scenario)
    {
        var newStart = new TimeOnly(newStartHour, newStartMinute);
        var newEnd = new TimeOnly(newEndHour, newEndMinute);

        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: newStart,
            EndTime: newEnd
        );

        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, newStart, newEnd, null
        ).Returns(shouldOverlap);

        _shiftRepository.AddAsync(Arg.Any<Shift>()).Returns(callInfo =>
        {
            var shift = callInfo.Arg<Shift>();
            shift.Id = 1;
            return shift;
        });

        if (shouldOverlap)
        {
            var exception = await Assert.ThrowsAsync<ShiftOverlapException>(
                () => _sut.CreateShiftAsync(request)
            );
            Assert.NotNull(exception);
        }
        else
        {
            var result = await _sut.CreateShiftAsync(request);
            Assert.NotNull(result);
            await _shiftRepository.Received(1).AddAsync(Arg.Any<Shift>());
        }
    }

    [Fact]
    public async Task CreateShiftAsync_DifferentEmployees_ShouldNotConflict()
    {
        var employee1 = new Employee { Id = 1, Name = "John" };
        var employee2 = new Employee { Id = 2, Name = "Jane" };

        _employeeRepository.GetByIdAsync(employee1.Id).Returns(employee1);
        _employeeRepository.GetByIdAsync(employee2.Id).Returns(employee2);

        var request1 = new CreateShiftRequest(
            EmployeeId: employee1.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(12, 0)
        );

        var request2 = new CreateShiftRequest(
            EmployeeId: employee2.Id,
            Date: _testDate,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(12, 0)
        );

        _shiftRepository.HasOverlappingShiftAsync(employee1.Id, _testDate, Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), null).Returns(false);
        _shiftRepository.HasOverlappingShiftAsync(employee2.Id, _testDate, Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), null).Returns(false);

        _shiftRepository.AddAsync(Arg.Any<Shift>()).Returns(callInfo =>
        {
            var shift = callInfo.Arg<Shift>();
            shift.Id = 1;
            return shift;
        });

        var result1 = await _sut.CreateShiftAsync(request1);
        var result2 = await _sut.CreateShiftAsync(request2);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        await _shiftRepository.Received(2).AddAsync(Arg.Any<Shift>());
    }

    [Fact]
    public async Task CreateShiftAsync_SameEmployeeDifferentDays_ShouldNotConflict()
    {
        var day1 = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var day2 = DateOnly.FromDateTime(DateTime.Today.AddDays(8));

        var request1 = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: day1,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        var request2 = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: day2,
            StartTime: new TimeOnly(9, 0),
            EndTime: new TimeOnly(17, 0)
        );

        _shiftRepository.HasOverlappingShiftAsync(_testEmployee.Id, day1, Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), null).Returns(false);
        _shiftRepository.HasOverlappingShiftAsync(_testEmployee.Id, day2, Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), null).Returns(false);

        _shiftRepository.AddAsync(Arg.Any<Shift>()).Returns(callInfo =>
        {
            var shift = callInfo.Arg<Shift>();
            shift.Id = 1;
            return shift;
        });

        var result1 = await _sut.CreateShiftAsync(request1);
        var result2 = await _sut.CreateShiftAsync(request2);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
    }

    [Theory]
    [InlineData(9, 0, 10, 30, 10, 30, 12, 0, false, "Back-to-back shifts (first ends when second starts)")]
    [InlineData(9, 0, 10, 30, 10, 29, 12, 0, true, "One minute overlap")]
    public async Task CreateShiftAsync_EdgeCases_ShouldBehaveCorrectly(
        int existingStartHour, int existingStartMinute,
        int existingEndHour, int existingEndMinute,
        int newStartHour, int newStartMinute,
        int newEndHour, int newEndMinute,
        bool shouldOverlap,
        string scenario)
    {
        var newStart = new TimeOnly(newStartHour, newStartMinute);
        var newEnd = new TimeOnly(newEndHour, newEndMinute);

        var request = new CreateShiftRequest(
            EmployeeId: _testEmployee.Id,
            Date: _testDate,
            StartTime: newStart,
            EndTime: newEnd
        );

        _shiftRepository.HasOverlappingShiftAsync(
            _testEmployee.Id, _testDate, newStart, newEnd, null
        ).Returns(shouldOverlap);

        _shiftRepository.AddAsync(Arg.Any<Shift>()).Returns(callInfo =>
        {
            var shift = callInfo.Arg<Shift>();
            shift.Id = 1;
            return shift;
        });

        if (shouldOverlap)
        {
            await Assert.ThrowsAsync<ShiftOverlapException>(() => _sut.CreateShiftAsync(request));
        }
        else
        {
            var result = await _sut.CreateShiftAsync(request);
            Assert.NotNull(result);
        }
    }
}
