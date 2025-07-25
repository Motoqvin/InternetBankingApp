using AutoMapper;
using InterBanking.Api.Dtos;
using InterBanking.Api.Enums;
using InterBanking.Api.Exceptions;
using InterBanking.Api.Models;
using InterBanking.Api.Repositories.Base;
using InterBanking.Api.Services;
using Moq;

namespace InterBanking.Api.Tests;

public class InterServiceTests
{
    private readonly Mock<IInterBankingRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly InterBankingService _interService;
    public InterServiceTests()
    {
        _mockRepo = new Mock<IInterBankingRepository>();
        _mockMapper = new Mock<IMapper>();
        _interService = new InterBankingService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Authenticate_UserNotFoundAsync()
    {
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync((User)null!);

        _mockRepo.Setup(r => r.GetFinCodeByClientCodeAsync("client1"))
                 .ReturnsAsync("fin1");

        _mockRepo.Setup(r => r.GetUserByFinCodeAsync("fin1"))
                 .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _interService.Authenticate("client1", "pass", "127.0.0.1", "Chrome", "Windows"));
    }

    [Fact]
    public async Task Authenticate_UserLockedAsync()
    {
        var user = new User
        {
            IsLocked = true
        };

        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync(user);

        await Assert.ThrowsAsync<UserLockedException>(() =>
            _interService.Authenticate("client1", "pass", "127.0.0.1", "Chrome", "Windows"));
    }

    [Fact]
    public async Task Authenticate_WrongCredentialsAsync()
    {
        var user = new User { ClientCode = "client1", IsLocked = false };

        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync(user);

        _mockRepo.Setup(r => r.Authenticate(user.ClientCode, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<WrongCredentialsException>(() =>
            _interService.Authenticate("client1", "wrongpass", "127.0.0.1", "Chrome", "Windows"));
    }

    [Fact]
    public async Task Authenticate_ReturnDto()
    {
        var user = new User { ClientCode = "client1" };
        var dto = new UserResponseDto
        {
            StatusCode = StatusCode.Ok,
        };

        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync(user);

        _mockRepo.Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(user);

        _mockMapper.Setup(m => m.Map<User, UserResponseDto>(user))
                    .Returns(dto);

        var result = await _interService.Authenticate("client1", "validpass", "127.0.0.1", "Chrome", "Windows");

        Assert.NotNull(result);
        Assert.Equal(StatusCode.Ok, result.StatusCode);
    }


    [Fact]
    public async Task AuthenticateQr_ReturnsUserDto()
    {
        var token = "valid-token";
        var clientCode = "client1";
        var user = new User { ClientCode = clientCode };
        var userDto = new UserResponseDto { ClientCode = clientCode };

        _mockRepo.Setup(r => r.CheckQrToken(token)).Returns(clientCode);
        _mockRepo.Setup(r => r.AuthenticateQr(clientCode, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<User, UserResponseDto>(user)).Returns(userDto);

        var result = await _interService.AuthenticateQr(token, "127.0.0.1", "Chrome", "Windows");

        Assert.NotNull(result);
        Assert.Equal(clientCode, result.ClientCode);
        Assert.Equal(StatusCode.Ok, result.StatusCode);
        _mockRepo.Verify(r => r.KillQrToken(token), Times.Once);
    }

    [Fact]
    public async Task AuthenticateQr_NotFound()
    {
        _mockRepo.Setup(r => r.CheckQrToken("bad-token")).Returns((string)null!);
        _mockMapper.Setup(m => m.Map<User, UserResponseDto>(It.IsAny<User>()))
                   .Returns(new UserResponseDto());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _interService.AuthenticateQr("bad-token", "ip", "browser", "os"));
    }

    [Fact]
    public async Task CheckAuthenticationOtp_BadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _interService.CheckAuthenticationOtp(null!, "", "ip"));
    }

    [Fact]
    public async Task CheckAuthenticationOtp_UserLocked()
    {
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync(new User { IsLocked = true });

        await Assert.ThrowsAsync<UserLockedException>(() =>
            _interService.CheckAuthenticationOtp("client1", "otp", "ip"));
    }

    [Fact]
    public async Task CheckAuthenticationOtp_NotFound()
    {
        var user = new User { ClientCode = "client1" };
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.CheckUserOtpAsync("client1", "123456", "ip")).ReturnsAsync(StatusCode.NotFound);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _interService.CheckAuthenticationOtp("client1", "123456", "ip"));
    }

    [Fact]
    public async Task CheckAuthenticationOtp_ReturnsTrue()
    {
        var user = new User { ClientCode = "client1" };
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.CheckUserOtpAsync("client1", "123456", "ip")).ReturnsAsync(StatusCode.Ok);

        var result = await _interService.CheckAuthenticationOtp("client1", "123456", "ip");

        Assert.True(result);
    }


    [Fact]
    public async Task GetAuthenticationOtp_BadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _interService.GetAuthenticationOtp("", "", "ip"));
    }

    [Fact]
    public async Task GetAuthenticationOtp_UserLocked()
    {
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1"))
                 .ReturnsAsync(new User { IsLocked = true });

        await Assert.ThrowsAsync<UserLockedException>(() =>
            _interService.GetAuthenticationOtp("client1", "pass", "ip"));
    }

    [Fact]
    public async Task GetAuthenticationOtp_UserNotAllowed()
    {
        var user = new User { ClientCode = "client1" };
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.GetUserStatusOnLoginAsync("client1")).ReturnsAsync("DONOTLET");

        await Assert.ThrowsAsync<UserLockedException>(() =>
            _interService.GetAuthenticationOtp("client1", "pass", "ip"));
    }

    [Fact]
    public async Task GetAuthenticationOtp_WrongCredentials()
    {
        var user = new User { ClientCode = "client1" };
        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.GetUserStatusOnLoginAsync("client1")).ReturnsAsync("OK");
        _mockRepo.Setup(r => r.GetUser(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<WrongCredentialsException>(() =>
            _interService.GetAuthenticationOtp("client1", "wrongpass", "ip"));
    }

    [Fact]
    public async Task GetAuthenticationOtp_ReturnsOtpResponse()
    {
        var user = new User
        {
            ClientCode = "client1",
            CellPhone = "1234567890",
            EMail = "user@example.com",
            OtpMethod = "S"
        };

        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.GetUserStatusOnLoginAsync("client1")).ReturnsAsync("OK");
        _mockRepo.Setup(r => r.GetUser(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(user);
        _mockRepo.Setup(r => r.SaveOtpForClientAsync("client1", "1234567890", It.IsAny<string>(), ""))
                 .ReturnsAsync(true);

        var result = await _interService.GetAuthenticationOtp("client1", "pass", "ip");

        Assert.NotNull(result);
        Assert.Equal("1234567890", result.Mobile);
        Assert.Equal("client1", result.ClientCode);
        Assert.Equal(StatusCode.Ok, result.StatusCode);
    }

    [Fact]
    public async Task GetAuthenticationOtp_SaveOtpFails()
    {
        var user = new User
        {
            ClientCode = "client1",
            CellPhone = "1234567890",
            EMail = "user@example.com",
            OtpMethod = "S"
        };

        _mockRepo.Setup(r => r.GetUserByClientCodeAsync("client1")).ReturnsAsync(user);
        _mockRepo.Setup(r => r.GetUserStatusOnLoginAsync("client1")).ReturnsAsync("OK");
        _mockRepo.Setup(r => r.GetUser(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);
        _mockRepo.Setup(r => r.SaveOtpForClientAsync("client1", "1234567890", It.IsAny<string>(), ""))
                 .ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() =>
            _interService.GetAuthenticationOtp("client1", "pass", "ip"));
    }
}
