using Estellaris.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estellaris.EF.Interfaces {
  public interface IMapping<T> where T : class, IModel {
    void Map(EntityTypeBuilder<T> model);
  }
}