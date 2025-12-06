namespace JustTip.Core.Entities;

public class Employee : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<Shift> Shifts { get; set; } = [];
}