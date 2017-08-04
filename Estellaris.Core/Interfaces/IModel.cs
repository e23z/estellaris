using System;

namespace Estellaris.Core.Interfaces {
  public interface IModel {
    int Id { get; set; }
    DateTime CreatedAt { get; set; }
  }
}