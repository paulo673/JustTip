namespace JustTip.Core.Entities;

public class DailyTipPool : BaseEntity
{
    public required DateOnly Date { get; set; }
    public required decimal Amount { get; set; }
}