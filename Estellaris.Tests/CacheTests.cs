using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Estellaris.Core.DI;
using Estellaris.Core.Extensions;
using Estellaris.EF;
using Estellaris.EF.Cache;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class CacheTests {
    [TestMethod]
    public void TestCache() {
      var testsAssembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      var connection = new SqliteConnection("DataSource=:memory:");
      connection.Open();

      var factory = DependenciesProvider.Create().AddScopedFromAssemblies(new [] { testsAssembly })
        .AddCache(dbOpts => dbOpts.UseSqlite(connection))
        .Build();

      var dbContext = factory.GetService<CacheDbContext>();
      var dbCache = factory.GetService<IDistributedCache>();
      var cacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
      dbContext.Database.EnsureCreated();

      // test get/set and expiration
      var cacheStr = "My cached value.";
      var cachedStr = "";
      dbCache.SetString("testEntry", cacheStr, cacheOptions);
      cachedStr = dbCache.GetString("testEntry");
      Assert.IsFalse(string.IsNullOrWhiteSpace(cachedStr));
      Assert.AreEqual(cacheStr, cachedStr);
      Thread.Sleep(TimeSpan.FromSeconds(6));
      cachedStr = dbCache.GetString("testEntry");
      Assert.IsTrue(string.IsNullOrWhiteSpace(cachedStr));
      Assert.AreNotEqual(cacheStr, cachedStr);

      // test async
      Task.Factory.StartNew(async() => {
        var asyncCacheStr = "My async cached value.";
        var asyncCachedStr = "";
        await dbCache.SetStringAsync("asyncEntry", asyncCacheStr, cacheOptions);
        asyncCachedStr = await dbCache.GetStringAsync("asyncEntry");
        Assert.IsFalse(string.IsNullOrWhiteSpace(asyncCachedStr));
        Assert.AreEqual(asyncCacheStr, asyncCachedStr);
      }).Wait();

      // test multi-threaded
      var tasks = new Collection<Task>();
      for (var i = 0; i < 100; i++)
        tasks.Add(Task.Factory.StartNew(async() => {
          var key = DateTime.Now.Ticks.ToString();
          await dbCache.SetAsync(key, key.ToBytes());
          await dbCache.GetAsync(key);
        }));
      Task.WaitAll(tasks.ToArray());

      connection.Close();
      factory.Dispose();
    }
  }
}