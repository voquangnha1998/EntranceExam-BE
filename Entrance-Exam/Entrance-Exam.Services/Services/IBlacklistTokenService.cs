namespace EntranceExam.Services.Services
{
    public interface IBlacklistTokenService
    {
        Task AddToBlacklistAsync(string token, DateTime expiresAt);
        Task<bool> IsBlacklistedAsync(string token);
    }

}
