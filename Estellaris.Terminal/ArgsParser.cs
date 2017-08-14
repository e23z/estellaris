using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace Estellaris.Terminal {
  public static class ArgsParser {
    static readonly Parser _parser;
    static readonly ICollection<Type> _types = new Collection<Type>();
    static ParserResult<object> _parseResult { get; set; }

    static ArgsParser() {
      _parser = new Parser(with => {
        with.ParsingCulture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.CurrentCulture;
        with.HelpWriter = Console.Out;
      });
    }

    public static void LoadOptionsFromAssembly(Assembly assembly) {
      var types = assembly.DefinedTypes.Where(
        t => typeof(BaseOptions).IsAssignableFrom(t.Assembly.GetType(t.FullName))
      ).Select(t => t.Assembly.GetType(t.FullName));

      foreach (var type in types) {
        if (!_types.Contains(type))
          _types.Add(type);
      }
    }

    public static void Parse(IEnumerable<string> args) {
      _parseResult = _parser.ParseArguments(args, _types.ToArray());
    }

    public static T Parse<T>(IEnumerable<string> args) {
      T options = default(T);
      _parser.ParseArguments<T>(args).WithParsed<T>(o => options = o);
      return options;
    }

    public static T GetParsed<T>() {
      T options = default(T);
      _parseResult.WithParsed<T>(o => options = o);
      return options;
    }

    public static void Bind<T>(Action<T> optionsAction) {
      _parseResult.WithParsed(optionsAction);
    }
  }
}