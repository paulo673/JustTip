using JustTip.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Employees.AnyAsync())
            return;

        var employees = new List<Employee>
        {
            new("Alice Johnson"),
            new("Bob Smith"),
            new("Carlos Rodriguez"),
            new("Diana Chen"),
            new("Eduardo Silva")
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday)
            monday = monday.AddDays(-7);

        var shifts = new List<Shift>();

        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = monday.AddDays(dayOffset);
            var dayOfWeek = date.DayOfWeek;

            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                shifts.Add(new Shift(employees[0].Id, date, new TimeOnly(10, 0), new TimeOnly(18, 0)));
                shifts.Add(new Shift(employees[1].Id, date, new TimeOnly(12, 0), new TimeOnly(20, 0)));
                shifts.Add(new Shift(employees[2].Id, date, new TimeOnly(14, 0), new TimeOnly(22, 0)));
            }
            else
            {
                shifts.Add(new Shift(employees[0].Id, date, new TimeOnly(9, 0), new TimeOnly(17, 0)));
                shifts.Add(new Shift(employees[1].Id, date, new TimeOnly(10, 0), new TimeOnly(18, 0)));
                shifts.Add(new Shift(employees[2].Id, date, new TimeOnly(11, 0), new TimeOnly(19, 0)));
                shifts.Add(new Shift(employees[3].Id, date, new TimeOnly(14, 0), new TimeOnly(22, 0)));
                shifts.Add(new Shift(employees[4].Id, date, new TimeOnly(16, 0), new TimeOnly(23, 0)));
            }
        }

        await context.Shifts.AddRangeAsync(shifts);

        var dailyTips = new List<DailyTipPool>();
        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = monday.AddDays(dayOffset);
            var amount = date.DayOfWeek switch
            {
                DayOfWeek.Friday => 450.00m,
                DayOfWeek.Saturday => 600.00m,
                DayOfWeek.Sunday => 350.00m,
                _ => 200.00m
            };

            dailyTips.Add(new DailyTipPool(date, amount));
        }

        await context.DailyTipPools.AddRangeAsync(dailyTips);
        await context.SaveChangesAsync();
    }
}
