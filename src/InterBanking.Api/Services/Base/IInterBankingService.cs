using InterBanking.Api.Dtos;
using InterBanking.Api.Enums;
using InterBanking.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace InterBanking.Api.Services.Base;
public interface IInterBankingService
{
    Task<GetOtpResponse> GetAuthenticationOtp(string pClientCode, string password, string ipAddress);

    Task<bool> CheckAuthenticationOtp(string pClientCode, string otp, string ipAddress);

    Task<UserResponseDto> Authenticate(string pClientCode, string pPassword, string ipAddress, string browser, string os);

    Task<UserResponseDto> AuthenticateQr(string pToken, string ipAddress, string browser, string os);

    Task<string> GetUserStatusOnLogIn(string pClientCode);

    Task<int> IncreasePassMistakeCount(string pClientCode, string pRequestFromIp);
}