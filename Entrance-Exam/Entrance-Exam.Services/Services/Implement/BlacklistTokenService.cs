using StackExchange.Redis;

namespace EntranceExam.Services.Services.Implement
{
    public class BlacklistTokenService : IBlacklistTokenService
    {
        private readonly IDatabase _redis;

        public BlacklistTokenService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task AddToBlacklistAsync(string token, DateTime expiresAt)
        {
            var expiry = expiresAt - DateTime.UtcNow;
            await _redis.StringSetAsync($"blacklist:{token}", "1", expiry);
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            return await _redis.KeyExistsAsync($"blacklist:{token}");
        }
    }

}
