
using System.Linq.Expressions;
using EntranceExam.Repositories.Context;
using Microsoft.EntityFrameworkCore;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly EntranceTestDbContext _context;
    public BaseRepository(EntranceTestDbContext context)
    {
        _context = context;
    }

    public IQueryable<T> GetQueryable(Expression<Func<T, object>>[] expression = null)
    {
        var query = _context.Set<T>().AsQueryable();
        if (expression != null)
        {
            foreach (var include in expression)
            {
                query = query.Include(include);
            }
        }
        return query;
    }


    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }
    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await SaveChangesAsync();
    }
    public async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
