namespace JustTip.Core.Entities;

public class Shift : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }

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