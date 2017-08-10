using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Estellaris.Core.DI {
  public class DependenciesProvider {
    static DependenciesProvider _instance { get; set; }
    static IServiceProvider _serviceProvider { get; set; }
    static readonly IServiceCollection _serviceCollection = new ServiceCollection();

    public static DependenciesProvider AddScopedFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Scoped);
    }

    public static DependenciesProvider AddTransientFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Transient);
    }

    public static DependenciesProvider AddSingletonFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Singleton);
    }

    public static DependenciesProvider AddScopedFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Scoped);
    }

    public static DependenciesProvider AddTransientFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Transient);
    }

    public static DependenciesProvider AddSingletonFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Singleton);
    }

    static DependenciesProvider AddFromAssemblies(Func<AssemblyName, bool> filters, DependencyScope scope) {
      var rootAssembly = Assembly.GetEntryAssembly();
      var assemblies = rootAssembly.GetReferencedAssemblies()
        .Where(filters)
        .Select(Assembly.Load).ToList();

      if (filters.Invoke(rootAssembly.GetName()))
        assemblies.Add(rootAssembly);

      return AddFromAssemblies(assemblies, scope);
    }

    static DependenciesProvider AddFromAssemblies(IEnumerable<Assembly> assemblies, DependencyScope scope) {
      if (_serviceProvider != null)
        return _instance;
      
      if (_instance == null)
        _instance = new DependenciesProvider();

      var types = assemblies.SelectMany(_ => _.DefinedTypes).ToList();
      var classes = types.Where(_ => _.IsClass).ToList();
      var interfaces = types.Where(_ => _.IsInterface).ToList();

      foreach(var _interface in interfaces) {
        var implementation = classes.FirstOrDefault(_ => _.Name == _interface.Name.TrimStart('I'));
        if (implementation != null) {
          var interfaceType = _interface.Assembly.GetType(_interface.FullName);
          var implementationType = implementation.Assembly.GetType(implementation.FullName);
          switch (scope) {
            case DependencyScope.Scoped:
              _serviceCollection.AddScoped(interfaceType, implementationType);
              break;
            case DependencyScope.Transient:
              _serviceCollection.AddTransient(interfaceType, implementationType);
              break;
            case DependencyScope.Singleton:
              _serviceCollection.AddSingleton(interfaceType, implementationType);
              break;
          }
        }
      }

      return _instance;
    }

    public DependenciesProvider AddScoped(Type implementation) {
      return AddScoped(implementation, implementation);
    }

    public DependenciesProvider AddScoped(Type type, Type implementation) {
      if (_serviceProvider != null)
        return _instance;
      
      _serviceCollection.AddScoped(type, implementation);
      return _instance;
    }

    public DependenciesProvider AddScoped<TImplementation>() {
      AddScoped(typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddScoped<TInterface, TImplementation>() {
      AddScoped(typeof (TInterface), typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddTransient(Type implementation) {
      return AddTransient(implementation, implementation);
    }

    public DependenciesProvider AddTransient(Type type, Type implementation) {
      if (_serviceProvider != null)
        return _instance;
      
      _serviceCollection.AddTransient(type, implementation);
      return _instance;
    }

    public DependenciesProvider AddTransient<TImplementation>() {
      AddTransient(typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddTransient<TInterface, TImplementation>() {
      AddTransient(typeof (TInterface), typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddSingleton(Type implementation) {
      return AddSingleton(implementation, implementation);
    }

    public DependenciesProvider AddSingleton(Type type, Type implementation) {
      if (_serviceProvider != null)
        return _instance;

      _serviceCollection.AddSingleton(type, implementation);
      return _instance;
    }

    public DependenciesProvider AddSingleton<TImplementation>() {
      AddSingleton(typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddSingleton<TInterface, TImplementation>() {
      AddSingleton(typeof (TInterface), typeof (TImplementation));
      return _instance;
    }

    public DependenciesProvider AddConfig<T>(string key) where T : class {
      if (_serviceProvider != null)
        return _instance;
      
      if (Configs.Configuration == null)
        Configs.Init();
      
      _serviceCollection.AddOptions().Configure<T>(opts => Configs.Configuration.GetSection(key).Bind(opts));
      return _instance;
    }

    public IServiceCollection Build(IServiceCollection serviceCollection) {
      foreach (var serviceDescriptor in _serviceCollection)
        serviceCollection.Add(serviceDescriptor);
      return serviceCollection;
    }

    public void Build() {
      _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    public static T GetService<T>() {
      return _serviceProvider != null ? _serviceProvider.GetService<T>() : default(T);
    }
  }
}