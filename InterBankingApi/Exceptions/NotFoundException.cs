namespace Api.Exceptions;
public class NotFoundException : Exception
{
    public override string Message => "Not Found";
}