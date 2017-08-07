using System;
using System.Reflection;
using Estellaris.EF.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Estellaris.EF {
  public abstract class BaseDbContext : DbContext {
    static bool _logAdded { get; set; }
    static readonly object _lock = new object();

    public BaseDbContext() {
      var serviceProvider = this.GetInfrastructure<IServiceProvider>();
      var loggerFactory = (ILoggerFactory) serviceProvider.GetService(typeof (ILoggerFactory));
      lock(_lock) {
        if (!_logAdded) {
          loggerFactory.AddProvider(new EFLoggerProvider());
          _logAdded = true;
        }
      }
    }

    public BaseDbContext(DbContextOptions options) : base(options) { }

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