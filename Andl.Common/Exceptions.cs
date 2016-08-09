/// Andl is A New Data Language. See andl.org.
///
/// Copyright © David M. Bennett 2015-16 as an unpublished work. All rights reserved.
///
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.
/// 
/// This software is completely free to use for purposes of personal study. 
/// For distribution, modification, commercial use or other purposes you must 
/// comply with the terms of the licence originally supplied with it in 
/// the file Licence.txt or at http://andl.org/Licence.txt.
///
using System;

namespace Andl.Common {
  [Serializable]
  public class AndlException : Exception {
    public AndlException(string msg) : base(msg) { }
  }

  /// <summary>
  /// Shortcut class to throw commonly used exceptions
  /// </summary>
  public class Error {
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
