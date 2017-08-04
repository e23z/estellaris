using System.Reflection;

namespace Estellaris.EF {
  internal class MappingInfo {
    public object Map { get; set; }
    public MethodInfo Method { get; set; }
    public object Entity { get; set; }

    public MappingInfo() { }
    public MappingInfo(object map, MethodInfo method, object entity) {
      Map = map;
      Method = method;
      Entity = entity;
    }
  }
}