namespace JustTip.Core.Exceptions;

public class RetroactiveShiftException : Exception
{
    public RetroactiveShiftException()
        : base("Cannot create, update, or delete shifts for past dates.")
    {
    }

    public RetroactiveShiftException(string message)
        : base(message)
    {
    }
}
