namespace Api.Exceptions;
public class UserLockedException : Exception
{
    public override string Message => "User is locked";
}