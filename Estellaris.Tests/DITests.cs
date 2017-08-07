using System;
using System.Reflection;
using Estellaris.Core.DI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  internal interface IClass { string GetId(); }
  internal interface IClassScoped : IClass { }
  internal interface ITransient : IClass { }
  internal interface ISingleton : IClass { }
  internal class BaseClass {
    string Id { get; set; }
    public BaseClass() { Id = Guid.NewGuid().ToString(); }
    public string GetId() { return Id; }
  }
  internal class ClassScoped : BaseClass, IClassScoped { }
  internal class ClassTransient : BaseClass, ITransient { }
  internal class ClassSingleton : BaseClass, ISingleton { }
  internal class InjectedClass {
    public ISingleton Singleton { get; set; }
    public InjectedClass(ISingleton singleton) {
      Singleton = singleton;
    }
  }

  [TestClass]
  public class DITests {
    [TestInitialize]
    public void InitContainer() {
      var testsAssembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      DependenciesProvider.AddScopedFromAssemblies(new [] { testsAssembly })
        .AddTransient<ITransient, ClassTransient>()
        .AddSingleton<ISingleton, ClassSingleton>()
        .AddSingleton<InjectedClass>()
        .Build();
    }

    [TestMethod]
    public void TestContainer() {
      var scopedOne = DependenciesProvider.GetService<IClassScoped>();
      var scopedTwo = DependenciesProvider.GetService<IClassScoped>();
      var transientOne = DependenciesProvider.GetService<ITransient>();
      var transientTwo = DependenciesProvider.GetService<ITransient>();
      var singletonOne = DependenciesProvider.GetService<ISingleton>();
      var singletonTwo = DependenciesProvider.GetService<ISingleton>();
      var injected = DependenciesProvider.GetService<InjectedClass>();
      var invalid = DependenciesProvider.GetService<IClass>();

      Assert.IsNotNull(scopedOne);
      Assert.IsNotNull(scopedTwo);
      Assert.IsNotNull(transientOne);
      Assert.IsNotNull(transientTwo);
      Assert.IsNotNull(singletonOne);
      Assert.IsNotNull(singletonTwo);
      Assert.IsNotNull(injected);
      Assert.AreSame(scopedOne, scopedTwo);
      Assert.AreNotSame(transientOne, transientTwo);
      Assert.AreNotEqual(transientOne.GetId(), transientTwo.GetId());
      Assert.AreSame(singletonOne, singletonTwo);
      Assert.IsNotNull(injected.Singleton);
      Assert.AreSame(singletonOne, injected.Singleton);
      Assert.IsNull(invalid);
    }
  }
}