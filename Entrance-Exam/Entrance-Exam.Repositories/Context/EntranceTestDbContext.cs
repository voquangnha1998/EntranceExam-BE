using EntranceExam.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntranceExam.Repositories.Context
{
    public class EntranceTestDbContext : DbContext
    {
        public EntranceTestDbContext(DbContextOptions<EntranceTestDbContext> options) : base(options) { }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
    }
}
