namespace JustTip.Core.Entities;

public class DailyTipPool : BaseEntity
{
    private DailyTipPool() { }

    public DailyTipPool(DateOnly date, decimal amount, int id = 0)
    {
        Id = id;
        Date = date;
        Amount = amount;
    }

    public DateOnly Date { get; private set; }
    public decimal Amount { get; private set; }

    public void UpdateAmount(decimal amount)
    {
        Amount = amount;
    }
}