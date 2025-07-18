using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Api.Enums;
using Api.Exceptions;
using Api.Models;
using Api.Repositories;
using Api.Repositories.Base;
using Api.Services.Base;
using Microsoft.AspNetCore.Mvc;

namespace Api.Services;

public class InterBankingService : IInterBankingService
{
    public IInterBankingRepository InterBankingRepository { get; set; }
    public InterBankingService(IInterBankingRepository interBankingRepository)
    {
        this.InterBankingRepository = interBankingRepository;
    }

    private string GenerateOtp(int length, bool onlyNumeric)
    {
        const string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string smallAlphabets = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "1234567890";

        var characters = numbers;

        if (!onlyNumeric)
        {
            characters += alphabets + smallAlphabets + numbers;
        }

        var otp = string.Empty;

        for (var i = 0; i < length; i++)
        {
            string data;

            do
            {
                var index = new Random().Next(0, characters.Length);
                data = characters.ToCharArray()[index].ToString();
            } while (otp.IndexOf(data, StringComparison.InvariantCulture) != -1);

            otp += data;
        }

        return otp;
    }

    public async Task<User> Authenticate(string pClientCode, string pPassword, string ipAddress, string browser, string os)
    {
        User retVal = new();
        try
        {
            var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

            if (testUserInfo == null)
            {
                var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
                testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);
            }

            if (testUserInfo == null)
            {
                throw new NotFoundException();
            }

            if (testUserInfo.IsLocked)
            {
                throw new UserLockedException();
            }

            retVal = await InterBankingRepository.Authenticate(testUserInfo.ClientCode, pPassword, ipAddress, os, browser);

            _ = Task.Run(async () =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://e-bankofbaku.com/api/auth-registered-service/")
                };

                var request = new HttpRequestMessage(HttpMethod.Get, "check-last-login");
                request.Headers.Add("x-auth-token", testUserInfo.Sid);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            });

            if (retVal == null)
            {
                throw new WrongCredentialsException();
            }
        }
        catch (NotFoundException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (UserLockedException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (WrongCredentialsException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }

        return retVal;
    }

    public async Task<User> AuthenticateQr(string pToken, string ipAddress, string browser, string os)
    {
        User retVal = new();
        try
        {
            string clientCode = InterBankingRepository.CheckQrToken(pToken);
            if (clientCode != null)
            {
                retVal = await InterBankingRepository.AuthenticateQr(clientCode, ipAddress, os, browser);

                InterBankingRepository.KillQrToken(pToken);
            }
        }
        catch (NotFoundException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (UserLockedException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (WrongCredentialsException e)
        {
            retVal = new User { ClientType = "ExceptionCause_USER" };
            System.Console.WriteLine(e.Message);
            throw;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }

        return retVal;
    }

    public async Task<bool> CheckAuthenticationOtp(string pClientCode, string otp, string ipAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(pClientCode) || string.IsNullOrEmpty(otp))
                throw new BadRequestException();

            var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

            if (testUserInfo == null)
            {
                var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
                testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);

                if (testUserInfo == null) throw new NotFoundException();
            }

            if (testUserInfo.IsLocked) throw new UserLockedException();

            if (await InterBankingRepository.CheckUserOtpAsync(pClientCode, otp, ipAddress) != StatusCode.Ok)
            {
                throw new NotFoundException();
            }

            return true;
        }
        catch (NotFoundException e)
        {
            System.Console.WriteLine(e.Message);
        }
        catch (BadRequestException e)
        {
            System.Console.WriteLine(e.Message);
        }
        catch (UserLockedException e)
        {
            System.Console.WriteLine(e.Message);
        }
        catch (WrongCredentialsException e)
        {
            System.Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }

        return false;
    }

    public async Task<GetOtpResponse> GetAuthenticationOtp(string pClientCode, string password, string ipAddress)
    {
        var response = new GetOtpResponse();

        try
        {
            if (string.IsNullOrEmpty(pClientCode) || string.IsNullOrEmpty(password))
                throw new Exception("Client code or password is empty");

            var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

            if (testUserInfo == null)
            {
                var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
                testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);

                if (testUserInfo == null) throw new NotFoundException();
            }

            if (testUserInfo.IsLocked) throw new UserLockedException();

            var status = await InterBankingRepository.GetUserStatusOnLoginAsync(pClientCode);

            if (status == "DONOTLET") throw new UserLockedException();

            var userInfo = await InterBankingRepository.GetUser(pClientCode, password) ?? throw new WrongCredentialsException();
            var mobile = userInfo.CellPhone;
            var email = userInfo.EMail;

            var otp = GenerateOtp(6, true);

            if (testUserInfo.OtpMethod == "E")
            {
                mobile = "";
            }
            else
            {
                email = "";
            }

            if (!await InterBankingRepository.SaveOtpForClientAsync(pClientCode, mobile, otp, email))
            {
                throw new Exception("Can't save OTP");
            }

            response.Otp = otp;
            response.Mobile = mobile;
            response.ClientCode = pClientCode;
            response.StatusCode = StatusCode.Ok;
        }
        catch (NotFoundException e)
        {
            System.Console.WriteLine(e.Message);
            response.StatusCode = StatusCode.NotFound;
        }
        catch (BadRequestException e)
        {
            System.Console.WriteLine(e.Message);
            response.StatusCode = StatusCode.BadRequest;
        }
        catch (UserLockedException e)
        {
            System.Console.WriteLine(e.Message);
            response.StatusCode = StatusCode.BadRequest;
        }
        catch (WrongCredentialsException e)
        {
            System.Console.WriteLine(e.Message);
            response.StatusCode = StatusCode.BadRequest;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            response.StatusCode = StatusCode.ServerError;
        }

        return response;
    }

    public async Task<string> GetUserStatusOnLogIn(string pClientCode)
    {
        string status = "DONOTLET";
        try
        {
            status = await InterBankingRepository.GetUserStatusOnLoginAsync(pClientCode);

            if (string.IsNullOrEmpty(status)) throw new ArgumentNullException("Status is empty");
        }
        catch (ArgumentNullException e)
        {
            System.Console.WriteLine(e.Message);
        }

        return status;
    }

    public async Task<int> IncreasePassMistakeCount(string pClientCode, string pRequestFromIp)
    {
        int mistakes = -1;
        try
        {
            mistakes = await InterBankingRepository.IncreasePassMistakeCountAsync(pClientCode, null);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
        }
        return mistakes;
    }
}