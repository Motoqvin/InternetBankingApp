namespace InterBanking.Api.Exceptions;

public class BadRequestException : Exception
{
    public override string Message => "Empty Credentials";
    public string? ParamName { get; set; }
}