using System;

namespace Andl.Common {
  [Serializable]
  public class AndlException : Exception {
    public AndlException(string msg) : base(msg) { }
  }

  /// <summary>
  /// 
  /// </summary>
  public class NdlError {
    public static Exception NullArg(string arg) {
      return new ArgumentNullException(arg);
    }
    public static Exception Invalid(string msg) {
      return new InvalidOperationException(msg);
    }
    public static Exception MustOverride(string arg) {
      return new NotImplementedException($"must override {arg}");
    }
    public static Exception NotImpl(string msg) {
      return new NotImplementedException(msg);
    }
    public static Exception Argument(string msg) {
      return new ArgumentException(msg);
    }
    public static Exception Fatal(string msg) {
      return new AndlException("Fatal error: " + msg);
    }
    public static Exception Assert(string msg) {
      return new AndlException("Assertion failure: " + msg);
    }
  }
}
