using Api.Enums;
using Api.Models;

namespace Api.Repositories.Base;

public interface IInterBankingRepository
{
    Task<int> IncreasePassMistakeCountAsync(string pClientCode, string pRequestFromIp);
    Task<User> GetUserByClientCodeAsync(string clientCode);
    Task<User> GetUserByFinCodeAsync(string finCode);
    Task<string> GetFinCodeByClientCodeAsync(string clientCode);
    Task<string> GetUserStatusOnLoginAsync(string clientCode);
    Task<User> GetUser(string clientCode, string password);
    Task<bool> SaveOtpForClientAsync(string clientCode, string mobile, string otp, string email);
    Task<StatusCode> CheckUserOtpAsync(string pClientCode, string otp, string ipAddress);
    Task<User> Authenticate(string pClientCode, string pPassword, string ipAddress, string os, string browser);
    Task<User> AuthenticateQr(string pClientCode, string ipAddress, string os, string browser);
    string CheckQrToken(string token);
    bool KillQrToken(string token);
}