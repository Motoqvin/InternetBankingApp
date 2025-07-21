using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InterBanking.Api.Data;
using InterBanking.Api.Dtos;
using InterBanking.Api.Enums;
using InterBanking.Api.Exceptions;
using InterBanking.Api.Models;
using InterBanking.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class InterBankingRepository : IInterBankingRepository
{
    public InterDbContext DbContext { get; set; }

    public InterBankingRepository(InterDbContext dbContext)
    {
        this.DbContext = dbContext;
    }

    public async Task<int> IncreasePassMistakeCountAsync(string pClientCode, string pRequestFromIp)
    {
        var user = await GetUserByClientCodeAsync(pClientCode);
        if (user.IsLocked)
        {
            throw new UserLockedException();
        }
        user.Mistakes++;
        if (user.Mistakes >= 3)
        {
            user.IsLocked = true;
        }
        DbContext.Users.Update(user);
        DbContext.SaveChanges();
        return user.Mistakes;
    }

    public async Task<User> GetUserByClientCodeAsync(string clientCode)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == clientCode);
        return user!;
    }

    public async Task<User> GetUserByFinCodeAsync(string finCode)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.FinCode == finCode);
        return user!;
    }

    public async Task<string> GetFinCodeByClientCodeAsync(string clientCode)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == clientCode);
        return user?.FinCode!;
    }

    public async Task<string> GetUserStatusOnLoginAsync(string clientCode)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == clientCode);
        return user?.Status!;
    }

    public async Task<User> GetUser(string clientCode, string passwordHash)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == clientCode) ?? throw new NotFoundException(){ParamName = nameof(User)};
        if (user.Password != passwordHash)
        {
            await IncreasePassMistakeCountAsync(clientCode, null!);
            throw new WrongCredentialsException();
        }
        return user;
    }

    public async Task<bool> SaveOtpForClientAsync(string clientCode, string mobile, string otp, string email)
    {
        var isOk = false;
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == clientCode) ?? throw new NotFoundException(){ParamName=nameof(User)};
        user.Otp = otp;
        user.CellPhone = mobile;
        user.EMail = email;
        var res = DbContext.Users.Update(user);
        DbContext.SaveChanges();
        if (res != null) isOk = true;

        return isOk;
    }

    public async Task<StatusCode> CheckUserOtpAsync(string pClientCode, string otp, string ipAddress)
    {
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.ClientCode == pClientCode);

        user ??= await DbContext.Users.FirstOrDefaultAsync(u => u.Otp == otp);

        return user == null ? StatusCode.NotFound : StatusCode.Ok;
    }

    public async Task<User> Authenticate(string pClientCode, string pPasswordHash, string ipAddress, string os, string browser)
    {
        string extClientCode = "";
        if (pClientCode.Contains('/') && IsExtendedUser(pClientCode))
        {
            extClientCode = pClientCode;
            pPasswordHash = GetPasswordByExtendedUserAuthenticationInfo(pClientCode, pPasswordHash);
            pClientCode = pClientCode[..pClientCode.IndexOf('/')];
        }

        var userInfo = await GetUser(pClientCode, pPasswordHash);
        userInfo.ExtendedClientCode = extClientCode;

        DbContext.Users.Update(userInfo);
        DbContext.SaveChanges();

        return userInfo;
    }

    public string CheckQrToken(string token)
    {
        var result = DbContext.QrTokens.FirstOrDefault(q => q.Token == token) ?? throw new NotFoundException() { ParamName = nameof(QrToken) };
        return result.ClientCode;
    }

    public bool KillQrToken(string token)
    {
        var qrToken = DbContext.QrTokens.FirstOrDefault(q => q.Token == token);
        if (qrToken != null)
        {
            DbContext.QrTokens.Remove(qrToken);
            DbContext.SaveChanges();
            return true;
        }
        return false;
    }

    public async Task<User> AuthenticateQr(string pClientCode, string ipAddress, string os, string browser)
    {
        string extClientCode = "";
        if (pClientCode.Contains('/') && IsExtendedUser(pClientCode))
        {
            extClientCode = pClientCode;
            pClientCode = pClientCode[..pClientCode.IndexOf('/')];
        }

        var userInfo = await GetUserByClientCodeAsync(pClientCode);
        userInfo.ExtendedClientCode = extClientCode;

        DbContext.Users.Update(userInfo);
        DbContext.SaveChanges();

        return userInfo;
    }

    private string GetPasswordByExtendedUserAuthenticationInfo(string extendedClientCode, string passwordHash)
    {
        var user = DbContext.Users.FirstOrDefault(u => u.ExtendedClientCode == extendedClientCode) ?? throw new NotFoundException() { ParamName = nameof(User) };
        return user.Password;
    }

    private bool IsExtendedUser(string extendedClientCode)
    {
        return DbContext.Users.Any(u => u.ExtendedClientCode == extendedClientCode);
    }
}