using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Estellaris.EF {
  public abstract class BaseDbContext : DbContext {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      Mapper.RegisterAllFromAssembly(GetMappingAssembly(), modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      base.OnConfiguring(optionsBuilder);
    }

    protected abstract Assembly GetMappingAssembly();
  }
}