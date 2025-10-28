using System.Linq.Expressions;

namespace IHM_Distribution.Data.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdAsync(Guid id, string? includeProperties = null);
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> SaveAllAsync();
    }
}