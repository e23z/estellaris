using Estellaris.EF.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estellaris.EF.Cache {
  public class CacheMapping : IMapping<Cache> {
    public void Map(EntityTypeBuilder<Cache> model) {
      model.ToTable("Caches");
      model.HasKey(_ => _.Key);
      model.Property(_ => _.Value);
      model.Property(_ => _.ExpiresOn);
      model.HasIndex(_ => _.Key);
    }
  }
}