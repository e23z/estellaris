using System;
using System.Threading.Tasks;
using Estellaris.Core;
using Microsoft.Extensions.Caching.Distributed;

namespace Estellaris.EF.Cache {
  public class DbCache : IDistributedCache {
    readonly ICacheRepository _cacheRepository;
    static readonly object _lock = new object();

    public DbCache(ICacheRepository cacheRepository) {
      _cacheRepository = cacheRepository;
    }

    public byte[] Get(string key) {
      RemoveExpiredItems();
      return _cacheRepository.FindByKey(key)?.Value;
    }

    public Task<byte[]> GetAsync(string key) {
      RemoveExpiredItems();
      return _cacheRepository.FindByKeyAsync(key).ContinueWith(_ => _.Result.Value);
    }

    public void Refresh(string key) {
      _cacheRepository.RemoveExpiredItems();
    }

    public Task RefreshAsync(string key) {
      _cacheRepository.RemoveExpiredItems();
      return Task.FromResult(0);
    }

    public void Remove(string key) {
      _cacheRepository.Delete(key);
      RemoveExpiredItems();
    }

    public Task RemoveAsync(string key) {
      _cacheRepository.Delete(key);
      RemoveExpiredItems();
      return Task.FromResult(0);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) {
      RemoveExpiredItems();
      var expiration = GetExpiration(options);
      var cache = _cacheRepository.FindByKey(key);

      if (cache != null) {
        cache.Value = value;
        _cacheRepository.Update(cache);
      } else {
        cache = new Cache(key, value, expiration);
        _cacheRepository.Save(cache);
      }
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options) {
      RemoveExpiredItems();
      var expiration = GetExpiration(options);
      var cache = _cacheRepository.FindByKey(key);

      if (cache != null) {
        cache.Value = value;
        return _cacheRepository.UpdateAsync(cache);
      } else {
        cache = new Cache(key, value, expiration);
        return _cacheRepository.SaveAsync(cache);
      }
    }

    DateTime GetExpiration(DistributedCacheEntryOptions options) {
      var expiration = DateTime.UtcNow.AddSeconds(Configs.GetValue<int>("CacheExpiration"));

      if (options.AbsoluteExpirationRelativeToNow.HasValue)
        expiration = DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
      else if (options.AbsoluteExpiration.HasValue)
        expiration = options.AbsoluteExpiration.Value.DateTime;
      
      return expiration;
    }

    void RemoveExpiredItems() {
      lock(_lock) {
        _cacheRepository.RemoveExpiredItems();
      }
    }
  }
}