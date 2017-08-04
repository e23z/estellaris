using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.Structure;

namespace Estellaris.EF {
  public static class EntityQueryableExtensions {
    static readonly TypeInfo QueryCompilerTypeInfo = typeof (QueryCompiler).GetTypeInfo();
    static readonly FieldInfo QueryCompilerField = typeof (EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
    static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
    static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
    static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
    static readonly FieldInfo QueryCompilationContextFactoryField = typeof (Database).GetTypeInfo().DeclaredFields.Single(x => x.Name == "_queryCompilationContextFactory");

    public static string ToSql<T>(this IQueryable<T> query) where T : class {
      if (!(query is EntityQueryable<T>) && !(query is InternalDbSet<T>)) {
        throw new ArgumentException("Invalid query");
      }

      var queryCompiler = (IQueryCompiler) QueryCompilerField.GetValue(query.Provider);
      var nodeTypeProvider = (INodeTypeProvider) NodeTypeProviderField.GetValue(queryCompiler);
      var parser = (IQueryParser) CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
      var queryModel = parser.GetParsedQuery(query.Expression);
      var database = DataBaseField.GetValue(queryCompiler);
      var queryCompilationContextFactory = (IQueryCompilationContextFactory) QueryCompilationContextFactoryField.GetValue(database);
      var queryCompilationContext = queryCompilationContextFactory.Create(false);
      var modelVisitor = (RelationalQueryModelVisitor) queryCompilationContext.CreateQueryModelVisitor();
      modelVisitor.CreateQueryExecutor<T>(queryModel);
      var sql = modelVisitor.Queries.FirstOrDefault();
      return sql != null ? sql.ToString() : null;
    }
  }
}