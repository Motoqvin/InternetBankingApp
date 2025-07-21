using InterBanking.Api.Enums;

namespace InterBanking.Api.Models;

public class GetOtpResponse
{
    public string? ClientCode { get; set; }
    public string? Otp { get; set; }
    public string? Mobile { get; set; }
    public StatusCode StatusCode { get; set; }
}