using EntranceExam.Repositories.Context;
using EntranceExam.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntranceExam.Tests.Repositories
{
    [TestClass]
    public class TokenRepositoryTests
    {
        private EntranceTestDbContext _context;
        private BaseRepository<Token> _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EntranceTestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Token_{Guid.NewGuid()}")
                .Options;

            _context = new EntranceTestDbContext(options);
            _repository = new BaseRepository<Token>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
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

            Assert.IsNotNull(result);
            Assert.AreEqual("refresh-token-123", result.RefreshToken);
            Assert.AreEqual(1, _context.Tokens.Count());
        }

        [TestMethod]
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

            Assert.IsNotNull(result);
            Assert.AreEqual("find-token", result!.RefreshToken);
        }

        [TestMethod]
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
            Assert.AreEqual("new-token", updated!.RefreshToken);
        }

        [TestMethod]
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

            Assert.IsFalse(_context.Tokens.Any(t => t.Id == token.Id));
        }

        [TestMethod]
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

            Assert.AreEqual(0, _context.Tokens.Count());
        }
    }
}
