
using System.Linq.Expressions;
using EntranceExam.Repositories.Entities;
using EntranceExam.Services.Model;
using EntranceExam.Services.Services;
using EntranceExam.Services.Services.Implement;
using Microsoft.Extensions.Configuration;
using MockQueryable;
using Moq;

namespace EntranceExam.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IBaseRepository<User>> _userRepoMock;
        private Mock<IBaseRepository<Token>> _tokenRepoMock;
        private Mock<IJwtService> _jwtServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _userRepoMock = new Mock<IBaseRepository<User>>();
            _tokenRepoMock = new Mock<IBaseRepository<Token>>();
            _jwtServiceMock = new Mock<IJwtService>();
            _configurationMock = new Mock<IConfiguration>();
            _authService = new AuthService(_userRepoMock.Object, _tokenRepoMock.Object, _jwtServiceMock.Object, _configurationMock.Object);
        }

        [TestMethod]
        public async Task SignUpAsync_ShouldCreateUser_WhenEmailNotExists()
        {
            var users = new List<User>().AsQueryable().BuildMock();
            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(users);
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                         .ReturnsAsync((User u) => u);

            var request = new SignUpRequest
            {
                Email = "new@example.com",
                Password = "123456",
                FirstName = "John",
                LastName = "Doe"
            };

            var result = await _authService.SignUpAsync(request);

            Assert.AreEqual(request.Email, result.Email);
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Email already exists")]
        public async Task SignUpAsync_ShouldThrowException_WhenEmailExists()
        {
            var existingUsers = new List<User>
            {
                new User { Email = "new@example.com" }
            }.AsQueryable().BuildMock();

            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(existingUsers);

            var request = new SignUpRequest
            {
                Email = "new@example.com",
                Password = "123456",
                FirstName = "John",
                LastName = "Doe"
            };

            await _authService.SignUpAsync(request);
        }

        [TestMethod]
        public async Task SignInAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var password = "123456";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Email = "test@example.com", FirstName = "Test", LastName = "User", Hash = hash };
            var users = new List<User> { user }.AsQueryable().BuildMock();

            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(users);
            _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("access-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
            _configurationMock.Setup(c => c["JwtSettings:RefreshTokenLifetimeDays"]).Returns("7");
            _tokenRepoMock.Setup(t => t.AddAsync(It.IsAny<Token>())).ReturnsAsync((Token t) => t);

            var request = new SignInRequest { Email = "test@example.com", Password = password };

            var result = await _authService.SignInAsync(request);

            Assert.AreEqual(user.Email, result.User.Email);
            Assert.AreEqual("access-token", result.Token);
            Assert.AreEqual("refresh-token", result.RefreshToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Invalid email or password.")]
        public async Task SignInAsync_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            var users = new List<User>().AsQueryable().BuildMock();
            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(users);

            var request = new SignInRequest { Email = "wrong@example.com", Password = "wrongpass" };
            await _authService.SignInAsync(request);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = new User { Id = 1, Email = "user@example.com", FirstName = "Test", LastName = "User" };
            var users = new List<User> { user }.AsQueryable().BuildMock();
            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(users);

            var result = await _authService.GetUserByIdAsync(1);

            Assert.AreEqual("user@example.com", result.Email);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetUserByIdAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            var users = new List<User>().AsQueryable().BuildMock();
            _userRepoMock.Setup(r => r.GetQueryable(It.IsAny<Expression<Func<User, object>>[]>()))
                         .Returns(users);

            await _authService.GetUserByIdAsync(999);
        }
    }
}
