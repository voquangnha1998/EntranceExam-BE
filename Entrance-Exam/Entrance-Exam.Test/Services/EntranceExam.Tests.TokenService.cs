using System.Linq.Expressions;
using Entrance_Exam.Test.Constant;
using EntranceExam.Repositories.Entities;
using EntranceExam.Services.Services;
using EntranceExam.Services.Services.Implement;
using Microsoft.Extensions.Configuration;
using MockQueryable;
using Moq;

namespace EntranceExam.Tests.Services
{
    [TestClass]
    public class TokenServiceTests
    {
        private Mock<IBaseRepository<Token>> _tokenRepoMock;
        private Mock<IJwtService> _jwtServiceMock;
        private Mock<IConfiguration> _configMock;
        private TokenService _tokenService;

        [TestInitialize]
        public void Setup()
        {
            _tokenRepoMock = new Mock<IBaseRepository<Token>>();
            _jwtServiceMock = new Mock<IJwtService>();
            _configMock = new Mock<IConfiguration>();
            _tokenService = new TokenService(_tokenRepoMock.Object, _jwtServiceMock.Object, _configMock.Object);
        }

        [TestMethod]
        public async Task RemoveRefreshTokensAsync_ShouldRemoveTokens_WhenTokensExist()
        {
            var userId = 1;
            var tokens = new List<Token> { new Token { UserId = userId } }.AsQueryable().BuildMock();
            _tokenRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<Token, object>>[]>())).Returns(tokens);
            _tokenRepoMock.Setup(r => r.DeleteRangeAsync(It.IsAny<List<Token>>())).Returns(Task.CompletedTask);

            await _tokenService.RemoveRefreshTokensAsync(userId);

            _tokenRepoMock.Verify(r => r.DeleteRangeAsync(It.IsAny<List<Token>>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RemoveRefreshTokensAsync_ShouldThrow_WhenNoTokens()
        {
            var tokens = new List<Token>().AsQueryable().BuildMock();
            _tokenRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<Token, object>>[]>())).Returns(tokens);

            await _tokenService.RemoveRefreshTokensAsync(1);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenValid()
        {
            var refreshToken = "refresh_token";
            var token = new Token
            {
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.AddMinutes(10).ToString(Constants.DateTimeFormat),
                User = new User { Id = 1, Email = "test@example.com" }
            };

            var tokens = new List<Token> { token }.AsQueryable().BuildMock();
            _tokenRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<Token, object>>[]>())).Returns(tokens);
            _tokenRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Token>())).Returns(Task.CompletedTask);
            _tokenRepoMock.Setup(r => r.AddAsync(It.IsAny<Token>())).ReturnsAsync((Token t) => t);

            _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new_refresh_token");

            _configMock.Setup(c => c["JwtSettings:RefreshTokenLifetimeDays"]).Returns("1");

            var result = await _tokenService.RefreshTokenAsync(refreshToken);

            Assert.AreEqual("jwt_token", result.Token);
            Assert.AreEqual("new_refresh_token", result.RefreshToken);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RefreshTokenAsync_ShouldThrow_WhenTokenNotFound()
        {
            var tokens = new List<Token>().AsQueryable().BuildMock();
            _tokenRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<Token, object>>[]>())).Returns(tokens);

            await _tokenService.RefreshTokenAsync("invalid_token");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RefreshTokenAsync_ShouldThrow_WhenTokenExpired()
        {
            var token = new Token
            {
                RefreshToken = "expired_token",
                ExpiresIn = DateTime.UtcNow.AddMinutes(-5).ToString(Constants.DateTimeFormat),
                User = new User { Id = 1 }
            };

            var tokens = new List<Token> { token }.AsQueryable().BuildMock();
            _tokenRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<Token, object>>[]>())).Returns(tokens);

            await _tokenService.RefreshTokenAsync("expired_token");
        }
    }
}
