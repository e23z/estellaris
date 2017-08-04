using System;
using System.Collections.Generic;
using System.Linq;
using Estellaris.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Estellaris.EF.Logging {
  internal class EFLogger : ILogger {

    public bool IsEnabled(LogLevel logLevel) {
      return Configs.GetValueOrDefault<bool>("TraceQueries", false);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
      if (eventId.Id == (int) RelationalEventId.ExecutedCommand) {
        var data = state as IEnumerable<KeyValuePair<string, object>>;
        if (data != null) {
          var commandText = data.Single(p => p.Key == "CommandText").Value;
          Console.WriteLine(commandText);
          Console.WriteLine();
        }
      }
    }

    public IDisposable BeginScope<TState>(TState state) {
      return null;
    }
  }
}