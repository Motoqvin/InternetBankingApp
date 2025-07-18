namespace Api.Exceptions;
public class WrongCredentialsException : Exception
{
    public override string Message => "Credentials are wrong";
}