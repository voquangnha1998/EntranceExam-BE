using EntranceExam.Repositories.Entities;
using EntranceExam.Service.Services;
using EntranceExam.Services.Model;

namespace EntranceExam.Services.Services
{
    public interface ITokenService : IBaseService<Token>
    {
        Task RemoveRefreshTokensAsync(int userId);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    }
}
