using EntranceExam.Repositories.Context;
using EntranceExam.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace TokenRepositoryUnitTest
{
    public class TokenRepositoryTests : IDisposable
    {
        private readonly EntranceTestDbContext _context;
        private readonly BaseRepository<Token> _repository;

        public TokenRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<EntranceTestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Token_{Guid.NewGuid()}")
                .Options;

            _context = new EntranceTestDbContext(options);
            _repository = new BaseRepository<Token>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_ShouldAddToken()
        {
            var token = new Token
            {
                UserId = 1,
                RefreshToken = "refresh-token-123",
                ExpiresIn = "23/06/2025 03:55:19",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(token);

            Assert.NotNull(result);
            Assert.Equal("refresh-token-123", result.RefreshToken);
            Assert.Equal(1, _context.Tokens.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnToken()
        {
            var token = new Token
            {
                UserId = 2,
                RefreshToken = "find-token",
                ExpiresIn = "23/06/2025 03:55:19",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(token.Id);

            Assert.NotNull(result);
            Assert.Equal("find-token", result!.RefreshToken);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyToken()
        {
            var token = new Token
            {
                UserId = 3,
                RefreshToken = "old-token",
                ExpiresIn = "23/06/2025 03:55:19",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            token.RefreshToken = "new-token";
            token.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(token);

            var updated = await _context.Tokens.FindAsync(token.Id);
            Assert.Equal("new-token", updated!.RefreshToken);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveToken()
        {
            var token = new Token
            {
                UserId = 4,
                RefreshToken = "delete-token",
                ExpiresIn = "23/06/2025 03:55:19",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(token);

            Assert.False(_context.Tokens.Any(t => t.Id == token.Id));
        }

        [Fact]
        public async Task DeleteRangeAsync_ShouldRemoveTokens()
        {
            var tokens = new[]
            {
                new Token { UserId = 5, RefreshToken = "a", ExpiresIn = "23/06/2025 03:55:19", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Token { UserId = 6, RefreshToken = "b", ExpiresIn = "23/06/2025 03:55:19", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            _context.Tokens.AddRange(tokens);
            await _context.SaveChangesAsync();

            await _repository.DeleteRangeAsync(tokens);

            Assert.Equal(0, _context.Tokens.Count());
        }
    }
}
