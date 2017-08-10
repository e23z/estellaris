using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Estellaris.EF.Cache {
  public class CacheRepository : ICacheRepository {
    protected readonly CacheDbContext DbContext;
    protected readonly DbSet<Cache> DbSet;

    public CacheRepository(CacheDbContext context) {
      DbContext = context;
      DbSet = context.Set<Cache>();
    }

    public int Count() {
      return DbSet.AsNoTracking().Count();
    }

    public void Delete(Func<Cache, bool> predicate) {
      using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.Set<Cache>().RemoveRange(DbSet.AsNoTracking().Where(predicate));
        context.SaveChanges();
      }
    }

    public void Delete(string key) {
      Delete(_ => _.Key == key);
    }

    public IEnumerable<Cache> Find(Func<Cache, bool> predicate) {
      return DbSet.AsNoTracking().Where(predicate).ToList();
    }

    public Cache FindByKey(string key) {
      return DbSet.AsNoTracking().FirstOrDefault(_ => _.Key == key);
    }

    public Task<Cache> FindByKeyAsync(string key) {
      return DbSet.AsNoTracking().FirstOrDefaultAsync(_ => _.Key == key);
    }

    public void Save(Cache element) {
      using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.Set<Cache>().Add(element);
        context.SaveChanges();
      }
    }

    public Task SaveAsync(Cache element) {
      return Task.Run(async() => {
        using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
          context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
          context.Set<Cache>().Add(element);
          return await context.SaveChangesAsync();
        }
      });
    }

    public void Update(Cache element) {
      using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.Set<Cache>().Update(element);
        context.SaveChanges();
      }
    }

    public Task UpdateAsync(Cache element) {
      return Task.Run(async() => {
        using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
          context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
          context.Set<Cache>().Update(element);
          return await context.SaveChangesAsync();
        }
      });
    }

    public void RemoveExpiredItems() {
      using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
        var now = DateTime.UtcNow;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.Set<Cache>().RemoveRange(DbSet.AsNoTracking().Where(_ => now > _.ExpiresOn));
        context.SaveChanges();
      }
    }

    public Task RemoveExpiredItemsAsync() {
      return Task.Run(async() => {
        using(var context = new CacheDbContext(DbContext.DbContextOptions)) {
          var now = DateTime.UtcNow;
          context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
          context.Set<Cache>().RemoveRange(DbSet.AsNoTracking().Where(_ => now > _.ExpiresOn));
          return await context.SaveChangesAsync();
        }
      });
    }
  }
}