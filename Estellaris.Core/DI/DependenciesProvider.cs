using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Estellaris.Core.DI {
  public class DependenciesProvider {
    static DependenciesProvider _instance { get; set; }
    static IServiceProvider _serviceProvider { get; set; }
    static readonly ICollection<DependencyInfo> _interfaceImplementations = new Collection<DependencyInfo>();
    internal static ICollection<DependencyInfo> InterfaceImplementations { get { return _interfaceImplementations; } }

    public static DependenciesProvider AddScopedFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Scoped);
    }

    public static DependenciesProvider AddTransientFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Transient);
    }

    public static DependenciesProvider AddSingletonFromAssemblies(Func<AssemblyName, bool> filters) {
      return AddFromAssemblies(filters, DependencyScope.Singleton);
    }

    static DependenciesProvider AddFromAssemblies(Func<AssemblyName, bool> filters, DependencyScope scope) {
      if (_instance == null)
        _instance = new DependenciesProvider();

      var rootAssembly = Assembly.GetEntryAssembly();
      var assemblies = rootAssembly.GetReferencedAssemblies()
        .Where(filters)
        .Select(Assembly.Load).ToList();

      if (filters.Invoke(rootAssembly.GetName()))
        assemblies.Add(rootAssembly);

      var types = assemblies.SelectMany(_ => _.DefinedTypes).ToList();
      var classes = types.Where(_ => _.IsClass).ToList();
      var interfaces = types.Where(_ => _.IsInterface).ToList();

      foreach(var _interface in interfaces) {
        var implementation = classes.FirstOrDefault(_ => _.Name == _interface.Name.TrimStart('I'));
        if (implementation != null) {
          _interfaceImplementations.Add(new DependencyInfo(_interface, implementation, scope));
        }
      }

      return _instance;
    }

    public DependenciesProvider AddScoped(Type implementation) {
      return AddScoped(implementation, implementation);
    }

    public DependenciesProvider AddScoped(Type type, Type implementation) {
      _interfaceImplementations.Add(new DependencyInfo(type, implementation, DependencyScope.Scoped));
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
      _interfaceImplementations.Add(new DependencyInfo(type, implementation, DependencyScope.Transient));
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
      _interfaceImplementations.Add(new DependencyInfo(type, implementation, DependencyScope.Singleton));
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

    public IServiceCollection Build(IServiceCollection serviceCollection) {
      foreach(var interfaceImplemenation in _interfaceImplementations) {
        switch (interfaceImplemenation.Scope) {
          case DependencyScope.Scoped:
            serviceCollection.AddScoped(
              interfaceImplemenation.Interface, interfaceImplemenation.Implementation);
            break;
          case DependencyScope.Transient:
            serviceCollection.AddTransient(
              interfaceImplemenation.Interface, interfaceImplemenation.Implementation);
            break;
          case DependencyScope.Singleton:
            serviceCollection.AddSingleton(
              interfaceImplemenation.Interface, interfaceImplemenation.Implementation);
            break;
        }
      }
      return serviceCollection;
    }

    public void Build() {
      var serviceCollection = Build(new ServiceCollection());
      _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public T GetService<T>() {
      return _serviceProvider != null ? _serviceProvider.GetService<T>() : default(T);
    }
  }
}