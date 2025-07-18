using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Services.Base;
public interface IInterBankingService
{
    Task<GetOtpResponse> GetAuthenticationOtp(string pClientCode, string password, string ipAddress);

    Task<bool> CheckAuthenticationOtp(string pClientCode, string otp, string ipAddress);

    Task<User> Authenticate(string pClientCode, string pPassword, string ipAddress, string browser, string os);

    Task<User> AuthenticateQr(string pToken, string ipAddress, string browser, string os);

    Task<string> GetUserStatusOnLogIn(string pClientCode);

    Task<int> IncreasePassMistakeCount(string pClientCode, string pRequestFromIp);
}