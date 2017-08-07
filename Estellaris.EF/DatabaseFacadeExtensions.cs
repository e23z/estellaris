using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Estellaris.EF {
  public static class RDFacadeExtensions {
    public static RelationalDataReader FromSql(this DatabaseFacade databaseFacade, string sql, params object[] parameters) {
      var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
      using(concurrencyDetector.EnterCriticalSection()) {
        var rawSqlCommand = databaseFacade
          .GetService<IRawSqlCommandBuilder>()
          .Build(sql, parameters);

        return rawSqlCommand
          .RelationalCommand
          .ExecuteReader(
            databaseFacade.GetService<IRelationalConnection>(),
            parameterValues : rawSqlCommand.ParameterValues);
      }
    }

    public static async Task<RelationalDataReader> FromSqlAsync(this DatabaseFacade databaseFacade, string sql,
      CancellationToken cancellationToken = default(CancellationToken), params object[] parameters) {
      var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();
      using(concurrencyDetector.EnterCriticalSection()) {
        var rawSqlCommand = databaseFacade
          .GetService<IRawSqlCommandBuilder>()
          .Build(sql, parameters);

        return await rawSqlCommand
          .RelationalCommand
          .ExecuteReaderAsync(
            databaseFacade.GetService<IRelationalConnection>(),
            parameterValues : rawSqlCommand.ParameterValues,
            cancellationToken : cancellationToken);
      }
    }

    public static IEnumerable<T> FromSql<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters) where T : new() {
      using(var dataReader = databaseFacade.FromSql(sql, parameters).DbDataReader) {
        var entries = new List<T>();
        var properties = typeof (T).GetProperties();
        while (dataReader.Read()) {
          var model = new T();
          var columnNames = dataReader.GetColumnSchema().Select(o => o.ColumnName);
          foreach(var property in properties) {
            if (!property.CanWrite)
              continue;

            var columnAttr = property.GetCustomAttribute(typeof (ColumnAttribute)) as ColumnAttribute;
            var columnName = columnAttr?.Name ?? property.Name;

            if (property == null)
              continue;

            if (!columnNames.Contains(columnName))
              continue;

            var value = dataReader[columnName];
            var propertyType = property.DeclaringType;
            var nullable = propertyType.GetTypeInfo().IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>);
            if (value == DBNull.Value)
              value = null;

            if (value == null && propertyType.GetTypeInfo().IsValueType && !nullable)
              value = Activator.CreateInstance(propertyType);

            property.SetValue(model, value);
          }
          entries.Add(model);
        }
        return entries;
      }
    }
  }
}