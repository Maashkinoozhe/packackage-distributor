using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PDist.Database.Configuration;
using PDist.Database.Models;
using System.Collections.Concurrent;

namespace PDist.Database;

public class Repository<T> : IRepository<T>, IAsyncDisposable, IDisposable where T : DataItem
{
    private readonly ConcurrentDictionary<Guid, T> _cache = new();
    private readonly string _dbLocation;

    public Repository(IOptionsMonitor<DbOptions> optionsMonitor)
    {
        var dbOption = optionsMonitor.CurrentValue ?? throw new ArgumentException("Did not produce a value", nameof(optionsMonitor));
        _dbLocation = Path.Combine(dbOption.DbLocation, $"{typeof(T).Name}.json");
    }

    public async Task<T?> GetAsync(Guid id)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);

        return _cache.TryGetValue(id, out var item) ? item : null;
    }

    public async Task CreateAsync(T item)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);

        if (_cache.ContainsKey(item.Id) || !_cache.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"Item already exists in db: {item}");
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);

        if (!_cache.ContainsKey(id) || !_cache.TryRemove(id, out var _))
        {
            throw new InvalidOperationException($"Item does not exist in db. id: {id}");
        }
    }

    public async Task<bool> UpdateAsync(T item, T originalItem)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);

        if (!_cache.ContainsKey(item.Id))
        {
            throw new InvalidOperationException($"Item does not exist in db. id: {item.Id}");
        }

        return _cache.TryUpdate(item.Id, item, originalItem);
    }

    private async Task EnsureLoadedAsync()
    {
        if (!_cache.IsEmpty || !File.Exists(_dbLocation))
        {
            return;
        }

        var rawData = await File.ReadAllTextAsync(_dbLocation).ConfigureAwait(false);
        var newData = JsonConvert.DeserializeObject<Dictionary<Guid, T>>(rawData);

        if (newData == null)
        {
            throw new InvalidOperationException($"Could not Parse Db {_dbLocation}");
        }

        lock (_cache)
        {
            if (!_cache.IsEmpty)
            {
                return;

            }

            _cache.Clear();
            foreach (var dataItem in newData)
            {
                _cache.TryAdd(dataItem.Key, dataItem.Value);
            }
        }
    }

    private async Task SaveAsync()
    {
        var data = JsonConvert.SerializeObject(_cache, Formatting.Indented);
        Directory.CreateDirectory(Path.GetDirectoryName(_dbLocation)!);
        await File.WriteAllTextAsync(_dbLocation, data).ConfigureAwait(false);
    }

    private bool Disposed { get; set; }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Disposed) return;
        GC.SuppressFinalize(this);
        await SaveAsync().ConfigureAwait(false);
        Disposed = true;
    }
}