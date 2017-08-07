using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Estellaris.Core.Extensions;
using Estellaris.EF.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Estellaris.EF {
  public static class Mapper {
    static readonly ICollection<MappingInfo> _mappings = new Collection<MappingInfo>();

    public static void RegisterAllFromAssembly(Assembly assembly, ModelBuilder modelBuilder) {
      if (_mappings.IsEmpty())
        LoadMappingsFromAssembly(assembly, modelBuilder);

      foreach(var mapInfo in _mappings)
        mapInfo.Method.Invoke(mapInfo.Map, new [] { mapInfo.Entity });
    }

    static void LoadMappingsFromAssembly(Assembly assembly, ModelBuilder modelBuilder) {
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
        _mappings.Add(new MappingInfo(map, mapMethod, entityTypeBuilder));
      }
    }
  }
}