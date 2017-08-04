using System;
using System.Linq;
using System.Reflection;
using Estellaris.Core.Interfaces;
using Estellaris.EF.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estellaris.EF {
  public static class Mapper {
    public static void RegisterAllFromAssembly(Assembly assembly, ModelBuilder modelBuilder) {
      var mappingTypeFullname = typeof (IMapping<>).FullName;
      var mappingTypes = assembly.GetTypes().Where(t => !string.IsNullOrWhiteSpace(t.Namespace) &&
        t.GetTypeInfo().GetInterface(mappingTypeFullname) != null).ToList();
      foreach(var mappingType in mappingTypes) {
        var map = Activator.CreateInstance(mappingType);
        var mapMethod = mappingType.GetTypeInfo().GetMethod("Map");
        var entityType = mapMethod.GetParameters()
          .First()
          .ParameterType
          .GenericTypeArguments
          .FirstOrDefault();
        var modelBuilderMethod = modelBuilder.GetType()
          .GetTypeInfo()
          .GetMethods()
          .Where(x => x.Name.Equals("Entity") && x.IsGenericMethod && x.GetParameters().Length == 0)
          .First()
          .MakeGenericMethod(entityType);
        var entityTypeBuilder = modelBuilderMethod.Invoke(modelBuilder, null);
        mapMethod.Invoke(map, new [] { entityTypeBuilder });
      }
    }
  }
}