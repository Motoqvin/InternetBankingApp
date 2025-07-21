namespace InterBanking.Api.Models;

public class QrToken
{
    public int Id { get; set; }
    public string ClientCode { get; set; }
    public string Token { get; set; }
    public string TokenType { get; set; }
    public bool IsActive{ get; set; }
}