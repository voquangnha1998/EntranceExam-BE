using EntranceExam.Repositories.Context;
using EntranceExam.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace UserRepositoryUnitTest
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly EntranceTestDbContext _context;
        private readonly BaseRepository<User> _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<EntranceTestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EntranceTestDbContext(options);
            _repository = new BaseRepository<User>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            var password = "123456";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Email = "test@example.com", FirstName = "John", LastName = "Doe", Hash = hash };

            var result = await _repository.AddAsync(user);

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal(1, _context.Users.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var user = new User { Email = "find@example.com", FirstName = "Jane", LastName = "Doe" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyEntity()
        {
            var user = new User { Email = "update@example.com", FirstName = "Old", LastName = "Name" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.FirstName = "New";
            await _repository.UpdateAsync(user);

            var updated = await _context.Users.FindAsync(user.Id);
            Assert.Equal("New", updated!.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            var user = new User { Email = "delete@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(user);

            Assert.False(_context.Users.Any(u => u.Id == user.Id));
        }

        [Fact]
        public async Task DeleteRangeAsync_ShouldRemoveEntities()
        {
            var users = new List<User>
            {
                new User { Email = "a@example.com" },
                new User { Email = "b@example.com" }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            await _repository.DeleteRangeAsync(users);

            Assert.Empty(_context.Users);
        }

        [Fact]
        public void GetQueryable_ShouldReturnQueryable()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("123456");
            var user = new User { Email = "test@example.com", FirstName = "John", LastName = "Doe", Hash = hash };

            _context.Users.Add(user);
            _context.SaveChanges();

            var query = _repository.GetQueryable();

            Assert.Single(query);
        }
    }
}
