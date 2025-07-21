namespace InterBanking.Api.Exceptions;

public class NotFoundException : Exception
{
    public override string Message => "Not Found";
    public string? ParamName { get; set; }
}