using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Estellaris.Core.Extensions {
  public static class IEnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> _this) {
      return !_this.Any();
    }
  }
}