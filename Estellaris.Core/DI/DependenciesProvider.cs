using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Estellaris.Core.DI {
  public class DependenciesProvider : IDisposable {
    static DependenciesProvider _instance { get; set; }
    static readonly object _lock = new object();
    IServiceProvider _serviceProvider { get; set; }
    readonly IServiceCollection _serviceCollection = new ServiceCollection();

    public DependenciesProvider Current {
      get {
        lock(_lock) {
          if (_instance == null)
            _instance = new DependenciesProvider();
        }
        return _instance;
      }
    }

    public static DependenciesProvider Create() {
      return new DependenciesProvider();
    }

    public DependenciesProvider AddScopedFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Scoped);
    }

    public DependenciesProvider AddTransientFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Transient);
    }

    public DependenciesProvider AddSingletonFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Singleton);
    }

    public DependenciesProvider AddScopedFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Scoped);
    }

    public DependenciesProvider AddTransientFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Transient);
    }

    public DependenciesProvider AddSingletonFromAssemblies(IEnumerable<Assembly> assemblies) {
      return AddFromAssemblies(assemblies, DependencyScope.Singleton);
    }

    DependenciesProvider AddFromAssemblies(Func<AssemblyName, bool> filters, DependencyScope scope) {
      var rootAssembly = Assembly.GetEntryAssembly();
      var assemblies = rootAssembly.GetReferencedAssemblies()
        .Where(filters)
        .Select(Assembly.Load).ToList();

      if (filters.Invoke(rootAssembly.GetName()))
        assemblies.Add(rootAssembly);

      return AddFromAssemblies(assemblies, scope);
    }

    DependenciesProvider AddFromAssemblies(IEnumerable<Assembly> assemblies, DependencyScope scope) {
      if (_serviceProvider != null)
        return this;

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

      return this;
    }

    public DependenciesProvider AddScoped(Type implementation) {
      return AddScoped(implementation, implementation);
    }

    public DependenciesProvider AddScoped(Type type, Type implementation) {
      if (_serviceProvider != null)
        return this;

      _serviceCollection.AddScoped(type, implementation);
      return this;
    }

    public DependenciesProvider AddScoped<TImplementation>() {
      AddScoped(typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddScoped<TInterface, TImplementation>() {
      AddScoped(typeof (TInterface), typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddTransient(Type implementation) {
      return AddTransient(implementation, implementation);
    }

    public DependenciesProvider AddTransient(Type type, Type implementation) {
      if (_serviceProvider != null)
        return this;

      _serviceCollection.AddTransient(type, implementation);
      return this;
    }

    public DependenciesProvider AddTransient<TImplementation>() {
      AddTransient(typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddTransient<TInterface, TImplementation>() {
      AddTransient(typeof (TInterface), typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddSingleton(Type implementation) {
      return AddSingleton(implementation, implementation);
    }

    public DependenciesProvider AddSingleton(Type type, Type implementation) {
      if (_serviceProvider != null)
        return this;

      _serviceCollection.AddSingleton(type, implementation);
      return this;
    }

    public DependenciesProvider AddSingleton<TImplementation>() {
      AddSingleton(typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddSingleton<TInterface, TImplementation>() {
      AddSingleton(typeof (TInterface), typeof (TImplementation));
      return this;
    }

    public DependenciesProvider AddConfig<T>(string key) where T : class {
      if (_serviceProvider != null)
        return this;

      if (Configs.Configuration == null)
        Configs.Init();

      return AddConfig<T>(opts => Configs.Configuration.GetSection(key).Bind(opts));
    }

    public DependenciesProvider AddConfig<T>(Action<T> configureOptions) where T : class {
      _serviceCollection.AddOptions().Configure<T>(configureOptions);
      return this;
    }

    public DependenciesProvider Add(Action<IServiceCollection> addAction) {
      addAction?.Invoke(_serviceCollection);
      return this;
    }

    public DependenciesProvider Build(IServiceCollection serviceCollection) {
      foreach(var serviceDescriptor in _serviceCollection)
      serviceCollection.Add(serviceDescriptor);
      return this;
    }

    public DependenciesProvider Build() {
      _serviceProvider = _serviceCollection.BuildServiceProvider();
      return this;
    }

    public T GetService<T>() {
      return _serviceProvider != null ? _serviceProvider.GetService<T>() : default(T);
    }

    public void Dispose() {
      _serviceProvider = null;
      _serviceCollection.Clear();
    }
  }
}