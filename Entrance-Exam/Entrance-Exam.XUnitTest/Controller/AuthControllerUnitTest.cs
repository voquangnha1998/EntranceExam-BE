using System.Security.Claims;
using EntranceExam.Controllers;
using EntranceExam.Services.Model;
using EntranceExam.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Assert = Xunit.Assert;

namespace AuthControllerUnitTest;
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IBlacklistTokenService> _blacklistTokenServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _blacklistTokenServiceMock = new Mock<IBlacklistTokenService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _controller = new AuthController(
            _authServiceMock.Object,
            _blacklistTokenServiceMock.Object,
            _tokenServiceMock.Object
        );
    }

    [Fact]
    public async Task SignUp_ValidRequest_ReturnsOk()
    {
        var request = new SignUpRequest { Email = "test@example.com", Password = "password" };
        var response = new SignUpResponse { Id = 1 };
        _authServiceMock.Setup(s => s.SignUpAsync(request)).ReturnsAsync(response);

        var result = await _controller.SignUp(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task SignUp_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.SignUp(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SignUp_Exception_ReturnsInternalServerError()
    {
        var request = new SignUpRequest();
        _authServiceMock.Setup(s => s.SignUpAsync(request))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.SignUp(request);
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task SignUp_NullResponse_ReturnsBadRequest()
    {
        var request = new SignUpRequest();
        _authServiceMock.Setup(s => s.SignUpAsync(request)).ReturnsAsync((SignUpResponse)null);

        var result = await _controller.SignUp(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SignIn_ValidRequest_ReturnsOk()
    {
        var request = new SignInRequest { Email = "test@example.com", Password = "password" };
        var response = new SignInResponse { Token = "token", RefreshToken = "refresh" };
        _authServiceMock.Setup(s => s.SignInAsync(request)).ReturnsAsync(response);

        var result = await _controller.SignIn(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task SignIn_Exception_ReturnsInternalServerError()
    {
        var request = new SignInRequest();
        _authServiceMock.Setup(s => s.SignInAsync(request))
            .ThrowsAsync(new Exception("error"));

        var result = await _controller.SignIn(request);
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task SignIn_NullResponse_ReturnsBadRequest()
    {
        var request = new SignInRequest();
        _authServiceMock.Setup(s => s.SignInAsync(request)).ReturnsAsync((SignInResponse)null);

        var result = await _controller.SignIn(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_ValidRequest_ReturnsOk()
    {
        var token = "refresh-token";
        var response = new RefreshTokenResponse { Token = "new-token" };

        _tokenServiceMock.Setup(t => t.RefreshTokenAsync(token)).ReturnsAsync(response);
 
        var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = token });
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task SignOut_NoUserIdClaim_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.SignOut();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task SignOut_Exception_ReturnsInternalServerError()
    {
        SetupUserClaims(1);

        _tokenServiceMock.Setup(s => s.RemoveRefreshTokensAsync(1)).ThrowsAsync(new Exception("fail"));

        _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer fake.jwt.token";

        var result = await _controller.SignOut();
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Valid_ReturnsOk()
    {
        var token = "refresh-token";
        var response = new RefreshTokenResponse { Token = "new-token" };

        _tokenServiceMock.Setup(t => t.RefreshTokenAsync(token)).ReturnsAsync(response);

        var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = token });
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_KeyNotFound_ReturnsNotFound()
    {
        _tokenServiceMock.Setup(s => s.RefreshTokenAsync("abc"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = "abc" });
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_Exception_ReturnsInternalServerError()
    {
        _tokenServiceMock.Setup(s => s.RefreshTokenAsync("abc"))
            .ThrowsAsync(new Exception());

        var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = "abc" });
        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);
    }

    #region Helper Methods  
    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
           {
               new Claim(ClaimTypes.NameIdentifier, userId.ToString())
           };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    #endregion
}
