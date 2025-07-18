namespace Api.Models;

public class QrToken
{
    public string ClientCode { get; set; }
    public string Token { get; set; }
    public string TokenType { get; set; }
    public bool isActive{ get; set; }
}