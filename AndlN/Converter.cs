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
using Andl.Common;

  namespace AndlN {
  public class CommonConverter {
    static readonly Dictionary<CommonType, Func<string, object>> ToObjectDict = new Dictionary<CommonType, Func<string, object>> {
      { CommonType.Binary, s => null },
      { CommonType.Bool, s => s.SafeBoolParse() },
      { CommonType.Double, s => s.SafeBoolParse() },
      { CommonType.Integer, s => s.SafeIntParse() },
      { CommonType.None, s => null },
      { CommonType.Number, s => s.SafeDecimalParse() },
      { CommonType.Text, s => s },
      { CommonType.Time, s => s.SafeDatetimeParse() },
    };

    static readonly Dictionary<CommonType, Func<object, string>> ToStringDict = new Dictionary<CommonType, Func<object, string>> {
      { CommonType.Binary, s => null },
      { CommonType.Bool, s => s.ToString() },
      { CommonType.Double, s => s.ToString() },
      { CommonType.Integer, s => s.ToString() },
      { CommonType.None, s => null },
      { CommonType.Number, s => s.ToString() },
      { CommonType.Text, s => s as string },
      { CommonType.Time, s => s.ToString() },
    };

    public static readonly Dictionary<CommonType, Type> ToTypeDict = new Dictionary<CommonType, Type> {
      { CommonType.Binary, typeof(byte[]) },
      { CommonType.Bool, typeof(bool) },
      { CommonType.Double, typeof(double) },
      { CommonType.Integer, typeof(int) },
      { CommonType.Number, typeof(decimal) },
      { CommonType.Text, typeof(string) },
      { CommonType.Time, typeof(DateTime) },
    };

    public static readonly Dictionary<Type, CommonType> ToCommonDict = new Dictionary<Type, CommonType> {
      { typeof(byte[])  , CommonType.Binary },
      { typeof(bool)    , CommonType.Bool   },
      { typeof(double)  , CommonType.Double },
      { typeof(int)     , CommonType.Integer},
      { typeof(decimal) , CommonType.Number },
      { typeof(string)  , CommonType.Text   },
      { typeof(DateTime), CommonType.Time   },
    };

    public static readonly Dictionary<CommonType, object> ToDefaultDict = new Dictionary<CommonType, object> {
      { CommonType.Binary, new byte[0] },
      { CommonType.Bool, false },
      { CommonType.Double, 0.0 },
      { CommonType.Integer, 0 },
      { CommonType.Number, 0.0m },
      { CommonType.Text, "" },
      { CommonType.Time, DateTime.MinValue },
    };

    public static object[] ToObject(object[] row, CommonField[] heading) {
      var values = Enumerable.Range(0, row.Length).Select(x => {
        var rawvalue = row[x] as string;
        if (rawvalue == null) throw Error.Fatal($"bad raw value {heading[x].Name} = {row[x]}");
        var value = ToObjectDict[heading[x].CType](rawvalue);
        if (value == null) throw Error.Fatal($"bad value {heading[x].Name} = {row[x]}");
        return value;
      }).ToArray();
      return values;
    }

    public static object StringToCommon(string rawvalue, CommonType ctype, string name) {
      if (rawvalue == null) throw Error.NullArg("rawvalue");
      var value = ToObjectDict[ctype](rawvalue);
      if (value == null) throw Error.Fatal($"bad value {name} = {value}");
      return value;
    }

    public static CommonType TypeToCommon(Type type) {
      if (ToCommonDict.ContainsKey(type)) return ToCommonDict[type];
      return CommonType.None;
    }

    public static Type CommontoType(CommonType ctype) {
      if (ToTypeDict.ContainsKey(ctype)) return ToTypeDict[ctype];
      return null;
    }

    public static object GetDefault(CommonType ctype) {
      if (ToDefaultDict.ContainsKey(ctype)) return ToDefaultDict[ctype];
      return null;
    }

  }
}
