using System.Xml.Linq;
using PDist.Database.Models;

namespace PDist.Database;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Guid id);
    Task CreateAsync(T item);
    Task RemoveAsync(Guid id);
    Task<bool> UpdateAsync(T item, T originalItem);
}