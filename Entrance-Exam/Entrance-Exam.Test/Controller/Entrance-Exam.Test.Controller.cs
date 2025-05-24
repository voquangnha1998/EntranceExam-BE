using System.Security.Claims;
using EntranceExam.Controllers;
using EntranceExam.Services.Model;
using EntranceExam.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Entrance_Exam.Test.Controller
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private Mock<IBlacklistTokenService> _blacklistTokenServiceMock;
        private Mock<ITokenService> _tokenServiceMock;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
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

        [TestMethod]
        public async Task SignUp_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new SignUpRequest { Email = "test@example.com", Password = "password" };
            var response = new SignUpResponse { Id = 1 };
            _authServiceMock.Setup(s => s.SignUpAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.SignUp(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(response, okResult.Value);
        }

        [TestMethod]
        public async Task SignUp_NullRequest_ReturnsBadRequest()
        {
            var result = await _controller.SignUp(null);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task SignUp_Exception_ReturnsInternalServerError()
        {
            var request = new SignUpRequest();
            _authServiceMock.Setup(s => s.SignUpAsync(request))
                .ThrowsAsync(new Exception("error"));

            var result = await _controller.SignUp(request);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task SignUp_NullResponse_ReturnsBadRequest()
        {
            var request = new SignUpRequest();
            _authServiceMock.Setup(s => s.SignUpAsync(request)).ReturnsAsync((SignUpResponse)null);

            var result = await _controller.SignUp(request);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task SignIn_ValidRequest_ReturnsOk()
        {
            var request = new SignInRequest { Email = "test@example.com", Password = "password" };
            var response = new SignInResponse { Token = "abc" };
            _authServiceMock.Setup(s => s.SignInAsync(request)).ReturnsAsync(response);

            var result = await _controller.SignIn(request);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(response, okResult.Value);
        }

        [TestMethod]
        public async Task SignIn_Exception_ReturnsInternalServerError()
        {
            var request = new SignInRequest();
            _authServiceMock.Setup(s => s.SignInAsync(request))
                .ThrowsAsync(new Exception("error"));

            var result = await _controller.SignIn(request);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task SignIn_NullResponse_ReturnsBadRequest()
        {
            var request = new SignInRequest();
            _authServiceMock.Setup(s => s.SignInAsync(request)).ReturnsAsync((SignInResponse)null);

            var result = await _controller.SignIn(request);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetUserInformation_ValidUser_ReturnsOk()
        {
            var user = new UserInfoDto { FirstName = "1111", LastName = "2222", Email = "user@example.com" };
            _authServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);
            var userClaims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(userClaims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = await _controller.GetUserInformation();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(user, okResult.Value);
        }


        [TestMethod]
        public async Task GetUserInformation_NoUserIdClaim_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var result = await _controller.GetUserInformation();
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetUserInformation_UserNotFound_ReturnsNotFound()
        {
            SetupUserClaims(1);
            _authServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(It.IsAny<UserInfoDto>);

            var result = await _controller.GetUserInformation();
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task GetUserInformation_Exception_ReturnsInternalServerError()
        {
            SetupUserClaims(1);
            _authServiceMock.Setup(s => s.GetUserByIdAsync(1)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.GetUserInformation();
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task SignOut_NoUserIdClaim_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var result = await _controller.SignOut();
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task SignOut_Exception_ReturnsInternalServerError()
        {
            SetupUserClaims(1);

            _tokenServiceMock.Setup(s => s.RemoveRefreshTokensAsync(1)).ThrowsAsync(new Exception("fail"));

            _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer fake.jwt.token";

            var result = await _controller.SignOut();
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
        }

        [TestMethod]
        public async Task RefreshToken_Valid_ReturnsOk()
        {
            var token = "refresh-token";
            var response = new RefreshTokenResponse { Token = "new-token" };

            _tokenServiceMock.Setup(t => t.RefreshTokenAsync(token)).ReturnsAsync(response);

            var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = token });
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(response, okResult.Value);
        }

        [TestMethod]
        public async Task RefreshToken_KeyNotFound_ReturnsNotFound()
        {
            _tokenServiceMock.Setup(s => s.RefreshTokenAsync("abc"))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = "abc" });
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task RefreshToken_Exception_ReturnsInternalServerError()
        {
            _tokenServiceMock.Setup(s => s.RefreshTokenAsync("abc"))
                .ThrowsAsync(new Exception());

            var result = await _controller.Refresh(new RefreshTokenRequest { RefreshToken = "abc" });
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
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
}
