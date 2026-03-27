using HabitBreaker.API.Controllers;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using HabitBreaker.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace HabitBreaker.Tests;

public class AuthControllerTests
{
    private static AuthController CreateController(
        Mock<UserManager<ApplicationUser>>? userMgr = null,
        Mock<SignInManager<ApplicationUser>>? signInMgr = null,
        Mock<ITokenService>? tokenSvc = null)
    {
        userMgr ??= MockUserManager();
        signInMgr ??= MockSignInManager(userMgr.Object);
        tokenSvc ??= new Mock<ITokenService>();
        return new AuthController(userMgr.Object, signInMgr.Object, tokenSvc.Object);
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> um)
    {
        var ctxAccessor = new Mock<IHttpContextAccessor>();
        var claimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        return new Mock<SignInManager<ApplicationUser>>(
            um, ctxAccessor.Object, claimsPrincipalFactory.Object, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var userMgr = MockUserManager();
        var tokenSvc = new Mock<ITokenService>();
        var signInMgr = MockSignInManager(userMgr.Object);

        userMgr.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);

        var user = new ApplicationUser { Id = "u1", Email = "test@test.com", UserName = "test@test.com" };
        userMgr.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        tokenSvc.Setup(t => t.GenerateAccessToken(It.IsAny<ApplicationUser>())).Returns("access-token");
        tokenSvc.Setup(t => t.GenerateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RefreshToken { Token = "refresh-token", UserId = "u1", ExpiresAt = DateTime.UtcNow.AddDays(30) });

        var controller = CreateController(userMgr, signInMgr, tokenSvc);
        var result = await controller.Register(new RegisterRequest("test@test.com", "Test!Password1"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("access-token", response.AccessToken);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken." }));

        var controller = CreateController(userMgr);
        var result = await controller.Register(new RegisterRequest("dupe@test.com", "Test!Password1"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.Login(new LoginRequest("bad@test.com", "password"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        var userMgr = MockUserManager();
        var signInMgr = MockSignInManager(userMgr.Object);
        var tokenSvc = new Mock<ITokenService>();

        var user = new ApplicationUser { Id = "u1", Email = "user@test.com", UserName = "user@test.com" };
        userMgr.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        signInMgr.Setup(s => s.CheckPasswordSignInAsync(user, "Test!Password1", true))
                 .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        tokenSvc.Setup(t => t.GenerateAccessToken(user)).Returns("jwt-token");
        tokenSvc.Setup(t => t.GenerateRefreshTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RefreshToken { Token = "refresh", UserId = "u1", ExpiresAt = DateTime.UtcNow.AddDays(30) });

        var controller = CreateController(userMgr, signInMgr, tokenSvc);
        var result = await controller.Login(new LoginRequest("user@test.com", "Test!Password1"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("jwt-token", response.AccessToken);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var tokenSvc = new Mock<ITokenService>();
        tokenSvc.Setup(t => t.ValidateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

        var controller = CreateController(tokenSvc: tokenSvc);
        var result = await controller.Refresh(new RefreshRequest("bad-token"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
