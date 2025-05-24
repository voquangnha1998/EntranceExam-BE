using EntranceExam.Repositories.Entities;
using EntranceExam.Services.Model;
using EntranceExam.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EntranceExam.Services.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _repository;
        private readonly IBaseRepository<Token> _repositoryToken;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthService(IBaseRepository<User> repository, IBaseRepository<Token> repositoryToken, IJwtService jwtService, IConfiguration configuration)
        {
            _repository = repository;
            _repositoryToken = repositoryToken;
            _jwtService = jwtService;
            _config = configuration;
        }

        public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
        {
            var existingUser = await _repository.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Hash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(user);

            return new SignUpResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<SignInResponse> SignInAsync(SignInRequest request)
        {
            var user = await _repository.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Hash))
                throw new ArgumentException("Invalid email or password.");

            string accessToken = _jwtService.GenerateToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenLifetime = int.Parse(_config["JwtSettings:RefreshTokenLifetimeDays"]);
            var token = new Token
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresIn = DateTime.UtcNow.AddDays(refreshTokenLifetime).ToString(Constants.DateTimeFormat)
            };
            await _repositoryToken.AddAsync(token);

            return new SignInResponse
            {
                User = new UserInfoDto
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                },
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<UserInfoDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _repository.GetQueryable()
                .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found.");
                return new UserInfoDto
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching user information", ex);
            }
            
        }
    }
}
