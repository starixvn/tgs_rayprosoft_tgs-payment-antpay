namespace Tgs.Payment.AntPay;

internal class ApiFailException : Exception
{
    public ApiFailException()
    {
    }

    public ApiFailException(string? message) : base(message)
    {
    }
}