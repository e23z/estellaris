using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Estellaris.Core.Extensions {
  //TODO: Add Tests
  public static class IEnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> _this) {
      return !_this.Any();
    }
  }
}