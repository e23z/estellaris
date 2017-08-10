using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Estellaris.Core;
using Estellaris.Core.DI;
using Microsoft.Extensions.Options;
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
  internal class MyOptions {
    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
    public string StringValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public IEnumerable<string> StringArray { get; set; }
    public int[] IntArray { get; set; }
    public MySubOptions MySubOptions { get; set; }
  }
  internal class MySubOptions {
    public string Option1 { get; set; }
    public string Option2 { get; set; }
    public string Option3 { get; set; }
  }

  [TestClass]
  public class DITests {
    static DependenciesProvider _factory { get; set; }

    [TestInitialize]
    public void InitContainer() {
      var testsAssembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      _factory = DependenciesProvider.Create().AddScopedFromAssemblies(new [] { testsAssembly })
        .AddTransient<ITransient, ClassTransient>()
        .AddSingleton<ISingleton, ClassSingleton>()
        .AddSingleton<InjectedClass>()
        .AddConfig<MyOptions>("MyOptions")
        .Build();
    }

    [TestMethod]
    public void TestContainer() {
      var scopedOne = _factory.GetService<IClassScoped>();
      var scopedTwo = _factory.GetService<IClassScoped>();
      var transientOne = _factory.GetService<ITransient>();
      var transientTwo = _factory.GetService<ITransient>();
      var singletonOne = _factory.GetService<ISingleton>();
      var singletonTwo = _factory.GetService<ISingleton>();
      var injected = _factory.GetService<InjectedClass>();
      var invalid = _factory.GetService<IClass>();

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

    [TestMethod]
    public void TestConfigs() {
      var scopedOne = _factory.GetService<IClassScoped>();
      var intArray = new [] { 1, 2, 3 };
      var strArray = new [] { "Value1", "Value2" };
      var model = _factory.GetService<IOptions<MyOptions>>().Value;

      Assert.AreEqual(10, model.IntValue);
      Assert.IsTrue(model.BoolValue);
      Assert.AreEqual("Test", model.StringValue);
      Assert.AreEqual(new DateTime(2017, 01, 02, 12, 13, 14), model.DateTimeValue);
      Assert.IsNotNull(model.StringArray);
      Assert.AreEqual(2, model.StringArray.Count());
      Assert.IsTrue(strArray.SequenceEqual(model.StringArray));
      Assert.IsNotNull(model.IntArray);
      Assert.AreEqual(3, model.IntArray.Count());
      Assert.IsTrue(intArray.SequenceEqual(model.IntArray));
      Assert.IsNotNull(model.MySubOptions);
      Assert.AreEqual("Lorem", model.MySubOptions.Option1);
      Assert.AreEqual("Ipsum", model.MySubOptions.Option2);
      Assert.IsNull(model.MySubOptions.Option3);
    }

    [TestCleanup]
    public void CleanUp() {
      _factory.Dispose();
    }
  }
}