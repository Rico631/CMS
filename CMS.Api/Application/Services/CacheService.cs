using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CMS.Api.Application.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken token = default) where T : class;
    Task<T?> GetAsync<T>(string key, Func<Task<T?>> getValue, CancellationToken token = default) where T : class;
    Task SetAsync<T>(string key, T value, CancellationToken token = default) where T : class;
    Task RemoveAsync(string key, CancellationToken token = default);
}
public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken token = default) where T : class
    {
        var value = await cache.GetStringAsync(key, token);

        if (value == null)
            return default;

        return JsonSerializer.Deserialize<T>(value);

    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T?>> getValue, CancellationToken token = default) where T : class
    {
        T? cachedValue = await GetAsync<T>(key, token);
        if (cachedValue is not null)
            return cachedValue;

        cachedValue = await getValue();

        if (cachedValue is not null)
            await SetAsync<T>(key, cachedValue, token);

        return cachedValue;

    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        await cache.RemoveAsync(key, token);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken token = default) where T : class
    {
        var str = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, str, token);
    }
}
