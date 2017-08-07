using System.Collections.ObjectModel;
using Estellaris.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class IEnumerableExtensionsTests {
    [TestMethod]
    public void TestIsEmpty() {
      var collection = new Collection<object>();
      Assert.IsTrue(collection.IsEmpty());

      collection.Add(new object());
      Assert.IsFalse(collection.IsEmpty());
    }
  }
}