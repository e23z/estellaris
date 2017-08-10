using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Estellaris.EF.Cache {
  public interface ICacheRepository {
    int Count();
    IEnumerable<Cache> Find(Func<Cache, bool> predicate);
    Cache FindByKey(string key);
    Task<Cache> FindByKeyAsync(string key);
    void Delete(Func<Cache, bool> predicate);
    void Delete(string key);
    void Save(Cache element);
    Task SaveAsync(Cache element);
    void Update(Cache element);
    Task UpdateAsync(Cache element);
    void RemoveExpiredItems();
    Task RemoveExpiredItemsAsync();
  }
}