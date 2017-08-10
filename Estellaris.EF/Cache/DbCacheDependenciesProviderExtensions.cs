using System;
using Estellaris.Core.DI;
using Estellaris.EF.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Estellaris.EF {
  public static class DbCacheDependenciesProviderExtensions {
    public static DependenciesProvider AddCache(this DependenciesProvider dependenciesProvider,
      Action<DbContextOptionsBuilder> dbOptions = null) {
      dependenciesProvider
        .Add((services) => services.AddDbContext<CacheDbContext>(dbOptions))
        .AddSingleton<ICacheRepository, CacheRepository>()
        .AddSingleton<IDistributedCache, DbCache>();
      return dependenciesProvider;
    }
  }
}