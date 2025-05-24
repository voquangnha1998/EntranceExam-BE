using System.Globalization;
using EntranceExam.Repositories.Entities;
using EntranceExam.Services.Model;
using EntranceExam.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EntranceExam.Services.Services.Implement
{
    public class TokenService : ITokenService
    {
        private readonly IBaseRepository<Token> _repositoryToken;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;
        public TokenService(IBaseRepository<Token> repositoryToken, IJwtService jwtService, IConfiguration configuration)
        {
            _repositoryToken = repositoryToken;
            _jwtService = jwtService;
            _config = configuration;
        }
        public async Task RemoveRefreshTokensAsync(int userId)
        {
            var tokens = await _repositoryToken.GetQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            if (tokens == null || !tokens.Any())
                throw new ArgumentException("No tokens found for this user.");
            await _repositoryToken.DeleteRangeAsync(tokens);
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _repositoryToken.GetQueryable()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);

            if (existingToken == null)
                throw new KeyNotFoundException("Refresh token not found.");

            if (DateTime.UtcNow > DateTime.ParseExact(existingToken.ExpiresIn, Constants.DateTimeFormat, CultureInfo.InvariantCulture))
                throw new ArgumentException("Refresh token has expired.");

            await _repositoryToken.DeleteAsync(existingToken);

            var user = existingToken.User;
            var newJwtToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenLifetime = int.Parse(_config["JwtSettings:RefreshTokenLifetimeDays"]);
            var newToken = new Token
            {
                UserId = user.Id,
                RefreshToken = newRefreshToken,
                ExpiresIn = DateTime.UtcNow.AddDays(refreshTokenLifetime).ToString(Constants.DateTimeFormat),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repositoryToken.AddAsync(newToken);

            return new RefreshTokenResponse
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
