namespace PDist.Database;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Guid id);
    Task<IEnumerable<T>> ListAsync();
    Task CreateAsync(T item);
    Task RemoveAsync(Guid id);
    Task<bool> UpdateAsync(T item, T originalItem);
}