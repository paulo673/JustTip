namespace JustTip.Core.Exceptions;

public class ShiftOverlapException : Exception
{
    public ShiftOverlapException()
        : base("The employee already has a shift that overlaps with the specified time range.")
    {
    }

    public ShiftOverlapException(string message)
        : base(message)
    {
    }

    public ShiftOverlapException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
