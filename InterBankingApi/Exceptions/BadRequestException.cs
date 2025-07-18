namespace Api.Exceptions;
public class BadRequestException : Exception
{
    public override string Message => "Empty Credentials";
}