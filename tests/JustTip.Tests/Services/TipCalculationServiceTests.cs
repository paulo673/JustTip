using JustTip.Core.DTOs;
using JustTip.Core.Entities;
using JustTip.Core.Interfaces;
using JustTip.Core.Services;
using NSubstitute;

namespace JustTip.Tests.Services;

public class TipCalculationServiceTests
{
    private readonly IShiftRepository _shiftRepository;
    private readonly ITipRepository _tipRepository;
    private readonly TipCalculationService _sut;

    private readonly DateOnly _weekStart = new(2025, 1, 13);
    private readonly DateOnly _tuesday;
    private readonly DateOnly _friday;

    public TipCalculationServiceTests()
    {
        _shiftRepository = Substitute.For<IShiftRepository>();
        _tipRepository = Substitute.For<ITipRepository>();
        _sut = new TipCalculationService(_shiftRepository, _tipRepository);

        _tuesday = _weekStart.AddDays(1);
        _friday = _weekStart.AddDays(4);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_HighLowDayScenario_ShouldPayProportionallyPerDay()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };
        var bob = new Employee { Id = 2, Name = "Bob" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) },
            new() { Id = 2, EmployeeId = 2, Employee = bob, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _tuesday, Amount = 10m },
            new() { Id = 2, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Equal(110m, result.TotalWeeklyTips);
        Assert.Equal(10m, result.TotalWeeklyHours);
        Assert.Equal(2, result.EmployeePayouts.Count);

        var alicePayout = result.EmployeePayouts.First(p => p.EmployeeName == "Alice");
        var bobPayout = result.EmployeePayouts.First(p => p.EmployeeName == "Bob");

        Assert.Equal(10m, alicePayout.PayoutAmount);
        Assert.Equal(100m, bobPayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_MixedShiftsSameDay_ShouldDivideProportionallyByHours()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };
        var bob = new Employee { Id = 2, Name = "Bob" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) },
            new() { Id = 2, EmployeeId = 2, Employee = bob, Date = _friday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(19, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Equal(100m, result.TotalWeeklyTips);
        Assert.Equal(10m, result.TotalWeeklyHours);

        var alicePayout = result.EmployeePayouts.First(p => p.EmployeeName == "Alice");
        var bobPayout = result.EmployeePayouts.First(p => p.EmployeeName == "Bob");

        Assert.Equal(50m, alicePayout.PayoutAmount);
        Assert.Equal(50m, bobPayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_UnequalHoursSameDay_ShouldDivideProportionally()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };
        var bob = new Employee { Id = 2, Name = "Bob" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) },
            new() { Id = 2, EmployeeId = 2, Employee = bob, Date = _friday, StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(18, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 90m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        var alicePayout = result.EmployeePayouts.First(p => p.EmployeeName == "Alice");
        var bobPayout = result.EmployeePayouts.First(p => p.EmployeeName == "Bob");

        Assert.Equal(30m, alicePayout.PayoutAmount);
        Assert.Equal(60m, bobPayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_MultipleShiftsSameEmployeeSameDay_ShouldSumHours()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) },
            new() { Id = 2, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(17, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 60m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Single(result.EmployeePayouts);
        var alicePayout = result.EmployeePayouts.First();
        Assert.Equal("Alice", alicePayout.EmployeeName);
        Assert.Equal(6m, alicePayout.TotalHours);
        Assert.Equal(60m, alicePayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_ComplexWeekScenario_ShouldCalculateDailyAndSum()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };
        var bob = new Employee { Id = 2, Name = "Bob" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) },
            new() { Id = 2, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) },
            new() { Id = 3, EmployeeId = 2, Employee = bob, Date = _friday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(19, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _tuesday, Amount = 20m },
            new() { Id = 2, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Equal(120m, result.TotalWeeklyTips);

        var alicePayout = result.EmployeePayouts.First(p => p.EmployeeName == "Alice");
        var bobPayout = result.EmployeePayouts.First(p => p.EmployeeName == "Bob");

        Assert.Equal(70m, alicePayout.PayoutAmount);
        Assert.Equal(50m, bobPayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_DayWithTipsButNoShifts_ShouldIgnoreThatDay()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _tuesday, Amount = 50m },
            new() { Id = 2, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        var alicePayout = result.EmployeePayouts.First();
        Assert.Equal(50m, alicePayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_DayWithShiftsButNoTips_ShouldContributeZeroToThatDay()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) },
            new() { Id = 2, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        var alicePayout = result.EmployeePayouts.First();
        Assert.Equal(10m, alicePayout.TotalHours);
        Assert.Equal(100m, alicePayout.PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_NoShifts_ShouldReturnEmptyPayouts()
    {
        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(new List<Shift>());
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Equal(100m, result.TotalWeeklyTips);
        Assert.Equal(0m, result.TotalWeeklyHours);
        Assert.Empty(result.EmployeePayouts);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_NoTips_ShouldReturnZeroPayouts()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(14, 0) }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(new List<DailyTipPool>());

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        Assert.Equal(0m, result.TotalWeeklyTips);
        Assert.Equal(5m, result.TotalWeeklyHours);
        Assert.Single(result.EmployeePayouts);
        Assert.Equal(0m, result.EmployeePayouts.First().PayoutAmount);
    }

    [Fact]
    public async Task CalculateWeeklyPayoutAsync_ShouldRoundToTwoDecimalPlaces()
    {
        var alice = new Employee { Id = 1, Name = "Alice" };
        var bob = new Employee { Id = 2, Name = "Bob" };
        var charlie = new Employee { Id = 3, Name = "Charlie" };

        var shifts = new List<Shift>
        {
            new() { Id = 1, EmployeeId = 1, Employee = alice, Date = _friday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) },
            new() { Id = 2, EmployeeId = 2, Employee = bob, Date = _friday, StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(15, 0) },
            new() { Id = 3, EmployeeId = 3, Employee = charlie, Date = _friday, StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(18, 0) }
        };

        var tips = new List<DailyTipPool>
        {
            new() { Id = 1, Date = _friday, Amount = 100m }
        };

        var weekEnd = _weekStart.AddDays(6);
        _shiftRepository.GetShiftsByDateRangeAsync(_weekStart, weekEnd).Returns(shifts);
        _tipRepository.GetTipsByDateRangeAsync(_weekStart, weekEnd).Returns(tips);

        var result = await _sut.CalculateWeeklyPayoutAsync(_weekStart);

        foreach (var payout in result.EmployeePayouts)
        {
            Assert.Equal(33.33m, payout.PayoutAmount);
        }
    }

    [Fact]
    public async Task SaveDailyTipsAsync_ShouldCallRepositoryForEachTip()
    {
        var tips = new List<DailyTipInputDto>
        {
            new(_tuesday, 50m),
            new(_friday, 100m)
        };

        await _sut.SaveDailyTipsAsync(tips);

        await _tipRepository.Received(1).UpsertDailyTipAsync(_tuesday, 50m);
        await _tipRepository.Received(1).UpsertDailyTipAsync(_friday, 100m);
    }
}
