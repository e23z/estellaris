using System;
using System.Reflection;

namespace Estellaris.Core.DI {
  internal class DependencyInfo {
    public Type Interface { get; set; }
    public Type Implementation { get; set; }
    public DependencyScope Scope { get; set; }

    public DependencyInfo() { }
    public DependencyInfo(Type _interface, Type implementation) {
      Interface = _interface;
      Implementation = implementation;
    }
    public DependencyInfo(TypeInfo _interface, TypeInfo implementation) {
      Interface = _interface.Assembly.GetType(_interface.FullName);
      Implementation = implementation.Assembly.GetType(implementation.FullName);
    }
    public DependencyInfo(Type _interface, Type implementation, DependencyScope scope) : this(_interface, implementation) {
      Scope = scope;
    }
    public DependencyInfo(TypeInfo _interface, TypeInfo implementation, DependencyScope scope) : this(_interface, implementation) {
      Scope = scope;
    }
  }
}