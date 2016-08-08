using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using AndlN;
using Andl.Common;

namespace AndlN.Samples {
  /// <summary>
  /// Mainline for Samples
  /// </summary>
  class Program {
    const string Version = "Andl Samples";
    const string Help = "Samples <sample> [/options]\n"
      + "\t\tSample is 1 to 9.\n"
      + "\t/n\tn=1 to 4, set tracing level";
    static readonly Dictionary<string, Action<string>> _options = new Dictionary<string, Action<string>> {
    };

    static TextWriter Out { get { return _output; } }
    static TextWriter _output;

    static Action[] _samples = new Action[] {
      ()=>BasicSample.Run(),
      ()=>SPPsample1.Run(),
      ()=>DbixCdSample.Run(),
      ()=>NorthwindSample.Run(),
      ()=>WhileSample.Run(),
      //()=>OtherSample.Run(),
    };

    static void Main(string[] args) {
      Logger.Open(1);
      _output = Console.Out;
      _output.WriteLine(Version);
      var options = OptionParser.Create(_options, Help);
      if (!options.Parse(args))
        return;
      if (options.GetPath(0) == "all") {
        for (int i = 0; i < _samples.Length; ++i) {
          _samples[i]();
          Console.Write("...");
          Console.ReadLine();
        }
      } else {
        var n = options.GetPath(0).SafeIntParse();
        if (n.HasValue && n >= 0 && n < _samples.Length)
          _samples[n ?? 0]();
        else Console.WriteLine(Help);
      }
    }

    public static void Show<T>(string msg, IRelatable<T> srel) where T : class {
      Console.WriteLine(msg);
      int count = 0;
      foreach (var s in srel) {
        Console.WriteLine($"  {TupleType.Format(s)}");
        count++;
      }
      Console.WriteLine($"(Rows: {count})");
    }
    public static void Show<T>(string msg, IEnumerable<T> srel) {
      Console.WriteLine(msg);
      int count = 1;
      foreach (var s in srel) {
        Console.WriteLine("{0,4}: {1}", count, s);
        count++;
      }
    }
    public static void Show(string msg) {
      Console.WriteLine(msg);
    }

    public static void Show(string msg, params object[] arglist) {
      Console.WriteLine(msg + ": " + arglist.Join(" :: "));
    }
  }
}
