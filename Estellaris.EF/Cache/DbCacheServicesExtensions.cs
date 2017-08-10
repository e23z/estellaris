using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Estellaris.EF.Cache {
  public static class DbCacheServicesExtensions {
    public static IServiceCollection AddDbCache(this IServiceCollection services,
      Action<DbContextOptionsBuilder> dbOptions = null) {
      services
        .AddDbContext<CacheDbContext>(dbOptions)
        .AddSingleton<ICacheRepository, CacheRepository>()
        .AddSingleton<IDistributedCache, DbCache>();
      return services;
    }
  }
}