using System.Diagnostics;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Models;
using Api.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using UAParser;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class InterBankingController : ControllerBase
{
    private readonly IInterBankingService interBankingService;

    public InterBankingController(IInterBankingService interBankingService)
    {
        this.interBankingService = interBankingService;
    }

    [HttpPost]
    public async Task<IActionResult> GetAuthenticationOtp([FromBody] GetOtpDto dto)
    {
        var response = await interBankingService.GetAuthenticationOtp(dto.ClientCode, dto.Password, HttpContext.Connection.RemoteIpAddress?.ToString()!);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CheckAuthenticationOtp([FromBody] CheckOtpDto dto)
    {
        var res = await interBankingService.CheckAuthenticationOtp(dto.ClientCode,
        dto.Otp,
        HttpContext.Connection.RemoteIpAddress?.ToString()!);

        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateDto dto)
    {
        var parser = Parser.GetDefault();
        ClientInfo clientInfo = parser.Parse(HttpContext.Request.Headers.UserAgent.ToString());

        var browser = clientInfo.UA.Family;
        var os = clientInfo.OS.Family;

        var user = await interBankingService.Authenticate(dto.Username,
        dto.Password,
        HttpContext.Connection.RemoteIpAddress?.ToString()!,
        browser,
        os);

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AuthenticateQr([FromBody] AuthenticateQrDto dto)
    {
        var parser = Parser.GetDefault();
        ClientInfo clientInfo = parser.Parse(HttpContext.Request.Headers.UserAgent.ToString());

        var browser = clientInfo.UA.Family;
        var os = clientInfo.OS.Family;

        var user = await interBankingService.AuthenticateQr(dto.Token,
         HttpContext.Connection.RemoteIpAddress?.ToString()!,
          browser, os);

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> GetUserStatusOnLogIn([FromBody] GetUserStatusDto dto)
    {
        var status = await interBankingService.GetUserStatusOnLogIn(dto.ClientCode);

        return Ok(status);
    }

    [HttpPost]
    public IActionResult IncreaseMistakeCount([FromBody] IncreaseMistakeDto dto)
    {
        var result = interBankingService.IncreasePassMistakeCount(dto.ClientCode, null!);

        return Ok(result);
    }
}
