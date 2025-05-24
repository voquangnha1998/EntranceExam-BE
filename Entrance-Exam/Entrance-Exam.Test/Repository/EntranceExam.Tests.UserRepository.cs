using EntranceExam.Repositories.Context;
using EntranceExam.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntranceExam.Tests.Repositories
{
    [TestClass]
    public class BaseRepositoryTests
    {
        private EntranceTestDbContext _context;
        private BaseRepository<User> _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EntranceTestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EntranceTestDbContext(options);
            _repository = new BaseRepository<User>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task AddAsync_ShouldAddEntity()
        {
            var password = "123456";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Email = "test@example.com", FirstName = "John", LastName = "Doe", Hash = hash };

            var result = await _repository.AddAsync(user);

            Assert.IsNotNull(result);
            Assert.AreEqual("test@example.com", result.Email);
            Assert.AreEqual(1, _context.Users.Count());
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var user = new User { Email = "find@example.com", FirstName = "Jane", LastName = "Doe" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result!.Email);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldModifyEntity()
        {
            var user = new User { Email = "update@example.com", FirstName = "Old", LastName = "Name" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.FirstName = "New";
            await _repository.UpdateAsync(user);

            var updated = await _context.Users.FindAsync(user.Id);
            Assert.AreEqual("New", updated!.FirstName);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            var user = new User { Id = 1, Email = "delete@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(user);

            Assert.IsFalse(_context.Users.Any(u => u.Id == user.Id));
        }

        [TestMethod]
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

            Assert.AreEqual(0, _context.Users.Count());
        }

        [TestMethod]
        public void GetQueryable_ShouldReturnQueryable()
        {
            var password = "123456";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Email = "test@example.com", FirstName = "John", LastName = "Doe", Hash = hash };

            _context.Users.Add(user);
            _context.SaveChanges();

            var query = _repository.GetQueryable();

            Assert.AreEqual(1, query.Count());
        }
    }
}
