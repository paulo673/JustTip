namespace JustTip.Core.Entities;

public class Shift : BaseEntity
{
    private Shift() { }

    public Shift(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int id = 0, Employee? employee = null)
    {
        Id = id;
        EmployeeId = employeeId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Employee = employee;
    }

    public int EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }

    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public void Update(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        EmployeeId = employeeId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    public double DurationInHours
    {
        get
        {
            var start = StartTime.ToTimeSpan();
            var end = EndTime.ToTimeSpan();

            if (end < start)
            {
                return (end.Add(TimeSpan.FromDays(1)) - start).TotalHours;
            }

            return (end - start).TotalHours;
        }
    }
}