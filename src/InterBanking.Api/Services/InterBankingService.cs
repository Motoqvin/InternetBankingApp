using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using InterBanking.Api.Dtos;
using InterBanking.Api.Enums;
using InterBanking.Api.Exceptions;
using InterBanking.Api.Models;
using InterBanking.Api.Repositories;
using InterBanking.Api.Repositories.Base;
using InterBanking.Api.Services.Base;
using AutoMapper;

namespace InterBanking.Api.Services;

public class InterBankingService : IInterBankingService
{
    public IInterBankingRepository InterBankingRepository { get; set; }
    public IMapper Mapper { get; set; }
    public InterBankingService(IInterBankingRepository interBankingRepository, IMapper mapper)
    {
        this.InterBankingRepository = interBankingRepository;
        this.Mapper = mapper;
    }

    private static string GenerateOtp(int length, bool onlyNumeric)
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
            } while (otp.Contains(data, StringComparison.InvariantCulture));

            otp += data;
        }

        return otp;
    }

    public async Task<UserResponseDto> Authenticate(string pClientCode, string pPassword, string ipAddress, string browser, string os)
    {
        User user = new();
        UserResponseDto resp = new();

        var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

        if (testUserInfo == null)
        {
            var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
            testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);
        }

        if (testUserInfo == null)
        {
            throw new NotFoundException() { ParamName = nameof(User) };
        }

        if (testUserInfo.IsLocked)
        {
            throw new UserLockedException();
        }

        var passwordHash = pPassword.GetHashCode().ToString();

        user = await InterBankingRepository.Authenticate(testUserInfo.ClientCode, passwordHash, ipAddress, os, browser);

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

        if (user == null)
        {
            throw new WrongCredentialsException();
        }

        resp = Mapper.Map<User, UserResponseDto>(user);
        resp.StatusCode = StatusCode.Ok;

        return resp;
    }

    public async Task<UserResponseDto> AuthenticateQr(string pToken, string ipAddress, string browser, string os)
    {
        User user = new();
        UserResponseDto resp = new();

        string clientCode = InterBankingRepository.CheckQrToken(pToken);
        if (clientCode != null)
        {
            user = await InterBankingRepository.AuthenticateQr(clientCode, ipAddress, os, browser);

            InterBankingRepository.KillQrToken(pToken);
        }


        resp = Mapper.Map<User, UserResponseDto>(user);
        if (resp.ClientCode == null)
        {
            throw new NotFoundException() { ParamName = nameof(User) };
        }

        resp.StatusCode = StatusCode.Ok;

        return resp;
    }

    public async Task<bool> CheckAuthenticationOtp(string pClientCode, string otp, string ipAddress)
    {
        if (string.IsNullOrEmpty(pClientCode) || string.IsNullOrEmpty(otp))
            throw new BadRequestException() { ParamName = nameof(pClientCode) };

        var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

        if (testUserInfo == null)
        {
            var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
            testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);

            if (testUserInfo == null) throw new NotFoundException() { ParamName = nameof(User) };
        }

        if (testUserInfo.IsLocked) throw new UserLockedException();

        if (await InterBankingRepository.CheckUserOtpAsync(pClientCode, otp, ipAddress) != StatusCode.Ok)
        {
            throw new NotFoundException() { ParamName = nameof(otp) };
        }

        return true;
    }

    public async Task<GetOtpResponse> GetAuthenticationOtp(string pClientCode, string password, string ipAddress)
    {
        var response = new GetOtpResponse();

        if (string.IsNullOrEmpty(pClientCode) || string.IsNullOrEmpty(password))
            throw new BadRequestException() { ParamName = nameof(pClientCode) };

        var testUserInfo = await InterBankingRepository.GetUserByClientCodeAsync(pClientCode);

        if (testUserInfo == null)
        {
            var fin = await InterBankingRepository.GetFinCodeByClientCodeAsync(pClientCode);
            testUserInfo = await InterBankingRepository.GetUserByFinCodeAsync(fin);

            if (testUserInfo == null) throw new NotFoundException() { ParamName = nameof(User) };
        }

        if (testUserInfo.IsLocked) throw new UserLockedException();

        var status = await InterBankingRepository.GetUserStatusOnLoginAsync(pClientCode);

        if (status == "DONOTLET") throw new UserLockedException();

        var passwordHash = password.GetHashCode().ToString();

        var userInfo = await InterBankingRepository.GetUser(pClientCode, passwordHash) ?? throw new WrongCredentialsException();
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

        return response;
    }

    public async Task<string> GetUserStatusOnLogIn(string pClientCode)
    {
        string status = "DONOTLET";
        status = await InterBankingRepository.GetUserStatusOnLoginAsync(pClientCode);

        if (string.IsNullOrEmpty(status)) throw new ArgumentNullException("Status is empty");

        return status;
    }

    public async Task<int> IncreasePassMistakeCount(string pClientCode, string pRequestFromIp)
    {
        int mistakes = -1;

        if (string.IsNullOrEmpty(pClientCode))
            throw new BadRequestException() { ParamName = nameof(pClientCode) };

        mistakes = await InterBankingRepository.IncreasePassMistakeCountAsync(pClientCode, null!);

        return mistakes;
    }


    
}