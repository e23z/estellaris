using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Estellaris.EF.Cache {
  public class CacheDbContext : BaseDbContext {
    public readonly DbContextOptions DbContextOptions;

    public CacheDbContext() { }
    public CacheDbContext(DbContextOptions options) : base(options) {
      DbContextOptions = options;
    }

    protected override Assembly GetMappingAssembly() {
      return this.GetType().GetTypeInfo().Assembly;
    }
  }
}