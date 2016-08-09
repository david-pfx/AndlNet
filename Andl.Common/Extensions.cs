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
/// the file Licence.txt or at http://andl.org/Licence/.
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andl.Common {
  // General extensions
  public static class UtilExtensions {
    public static string Format(this byte[] value) {
      var s = value.Select(b => String.Format("{0:x2}", b));
      return String.Join("", s);
    }

    // string join that works on any enumerable
    public static string Join<T>(this IEnumerable<T> values, string delim) {
      return String.Join(delim, values.Select(v => v.ToString()));
    }

    public static string Join(this object[] values, string delim) {
      return values.Aggregate("", (a, o) => (a == "" ? "" : a + delim) + o.ToString());
    }

    // truncate a string if too long
    public static string Shorten(this string argtext, int len) {
      var text = argtext.Replace('\n', '.');
      if (text.Length <= len) return text;
      return text.Substring(0, len - 3) + "...";
    }

    // safe parsing routines, return null on error
    public static bool? SafeBoolParse(this string s) {
      bool value;
      return bool.TryParse(s, out value) ? value as bool? : null;
    }

    public static DateTime? SafeDatetimeParse(this string s) {
      DateTime value;
      return DateTime.TryParse(s, out value) ? value as DateTime? : null;
    }

    public static decimal? SafeDecimalParse(this string s) {
      decimal value;
      return Decimal.TryParse(s, out value) ? value as decimal? : null;
    }

    public static double? SafeDoubleParse(this string s) {
      double value;
      return double.TryParse(s, out value) ? value as double? : null;
    }

    public static int? SafeIntParse(this string s) {
      int value;
      return int.TryParse(s, out value) ? value as int? : null;
    }

  }
}
