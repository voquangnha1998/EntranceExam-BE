using System.Linq.Expressions;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> GetQueryable(Expression<Func<T, object>>[] expression = null);
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
