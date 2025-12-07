namespace JustTip.Core.Entities;

public class Employee : BaseEntity
{
    private Employee() { Name = string.Empty; }

    public Employee(string name, int id = 0)
    {
        Id = id;
        Name = name;
    }

    public string Name { get; private set; }

    public ICollection<Shift> Shifts { get; private set; } = [];
}