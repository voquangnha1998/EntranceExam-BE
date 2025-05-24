using EntranceExam.Repositories.Entities;

namespace EntranceExam.Services.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
