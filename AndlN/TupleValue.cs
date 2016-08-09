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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using Andl.Common;

namespace AndlN {
  ///==========================================================================
  /// <summary>
  /// Define the core interface that allow relational algebra, evaluation and storage
  /// </summary>
   
  // Implement this on a type that can be an argument for the relational algebra extensions
  public interface IRelatable {
    IEnumerator GetEnumerator();
    TupleType TupleType { get; }
  }

  // Specialisation for type matching
  public interface IRelatable<T> : IRelatable where T : class {
    new IEnumerator<T> GetEnumerator();
  }

  // Implement this on a type that can be evaluated to return a value
  public interface IEvaluatable<T> {
    T Evaluate();
  }

  // Implement this on a type that called upon to store a set of tuples
  public interface IStorable<T> {
    void Store();
  }

  ///==========================================================================
  /// <summary>
  /// Implement immutable heading type
  /// 
  /// Keeps a dictionary of field names and types
  /// shared by tuples with the same fields
  /// </summary>
  // an attribute of a heading
  internal struct HeadingAttribute {
    public string Name;
    public Type Type;

    public override string ToString() {
      return $"{Name}:{Type.Name}";
    }

    public override bool Equals(object obj) {
      return obj is HeadingAttribute && ((HeadingAttribute)obj).Name == Name && ((HeadingAttribute)obj).Type == Type;
    }

    public override int GetHashCode() {
      return Name.GetHashCode();
    }
  }

  ///==========================================================================
  /// <summary>
  /// Implement heading for tuple type
  /// </summary>
  internal class TupleHeading {
    // dictionary to reuse existing heading TODO: make it a HashSet
    static Dictionary<TupleHeading, TupleHeading> _headings = new Dictionary<TupleHeading, TupleHeading>();

    HeadingAttribute[] _attribs;
    int _hashcode;

    //--- ctor
    internal static TupleHeading GetInstance(HeadingAttribute[] attribs) {
      var newh = new TupleHeading(attribs);
      if (_headings.ContainsKey(newh)) return _headings[newh];
      _headings[newh] = newh;
      return newh;
    }

    // create heading, attribs assumed valid and ordered
    TupleHeading(HeadingAttribute[] attribs) {
      _attribs = attribs;
      _hashcode = _attribs.Aggregate(-1, (h, k) => h ^ k.Name.GetHashCode());
    }

    // precomputed hash code
    public override int GetHashCode() {
      return _hashcode;
    }

    // value equality for heading
    public override bool Equals(object obj) {
      TupleHeading other = obj as TupleHeading;
      if (other == this) return true;
      if (other == null || other.GetHashCode() != _hashcode || other._attribs.Length != _attribs.Length) return false;
      return Enumerable.Range(0, _attribs.Length).All(x => _attribs[x].Equals(other._attribs[x]));
    }
  }

  ///==========================================================================
  /// <summary>
  /// Implement immutable tuple type for getting field data by name
  /// 
  /// Tuples with a different base type but the same fields will have a
  /// different TupleType, because of the FieldInfo[].
  /// They share the same Heading, and compare equal by value.
  /// </summary>
  public class TupleType {
    // return Heading for this type
    internal TupleHeading Heading { get { return _heading; } }
    internal MemberInfo[] Fields { get { return _members; } }
    TupleHeading _heading;
    MemberInfo[] _members;    // fields in alpha order to make comparisons easier

    static Dictionary<Type, TupleType> _typedict = new Dictionary<Type, TupleType>();

    //--- ctor
    // return previous instance for this type, if it exists
    // else return new instance, possibly with shared heading
    // does not access field data, which may not yet exist
    static public TupleType GetInstance(Type objtype) {
      if (objtype == null) throw Error.NullArg("objtype");
      if (_typedict.ContainsKey(objtype)) return _typedict[objtype];

      // include public non-static fields and properties only
      var result = new TupleType(objtype.GetMembers(BindingFlags.Public | BindingFlags.Instance));
      //var istuple = objtype.IsSubclassOf(typeof(NdlTuple)); CHECK:is this interesting?
      _typedict[objtype] = result;
      return result;
    }

    TupleType(MemberInfo[] members) {
      _members = members.Where(m => m.MemberType == MemberTypes.Field
            || (m.MemberType == MemberTypes.Property && (m as PropertyInfo).GetIndexParameters().Length == 0))
        .OrderBy(f => f.Name).ToArray();
      var attribs = _members.Select(f => new HeadingAttribute {
        Name = f.Name,
        Type = (f.MemberType == MemberTypes.Field) ? (f as FieldInfo).FieldType : (f as PropertyInfo).PropertyType
      }).ToArray();
      _heading = TupleHeading.GetInstance(attribs);
    }

    // override equals, delegate to heading
    public override bool Equals(object obj) {
      var other = obj as TupleType;
      return (other == null) ? false : _heading.Equals(other._heading);
    }

    // override, delegate to heading
    public override int GetHashCode() {
      return _heading.GetHashCode();
    }

    // ToString for type itself
    public override string ToString() {
      return _members.Select(f => $"{f.Name}:{GetMemberType(f).Name}").Join(",");
    }

    // explicit check, but really the same as Equals
    public bool SameHeading(TupleType other) {
      if (other == null) throw Error.NullArg("other");
      return other._heading == _heading; // must actually be the same
    }

    //--- instance methods delegated from or called on behalf of tuple

    // ToString for value of type
    internal string ToString(object tuple) {
      return _members.Select(f => $"{f.Name}:{GetMemberValue(tuple, f)}").Join(",");
    }

    // Calculate hash code for a tuple, which must be of this tuple type
    // Note: must not call until values have been set
    public int GetHashCode(object tuple) {
      if (tuple == null) throw Error.NullArg("tuple");
      // use first value to compute hash code
      return (_members.Length == 0) ? -1 : GetMemberValue(tuple, _members[0]).GetHashCode();
    }

    //--- static methods with tuple type unknown

    // ToString for value of type
    public static string Format(object tuple) {
      return GetInstance(tuple.GetType()).ToString(tuple);
    }

    // Equals check for tuples of unknown type
    public static bool AreEqual(object tuple, object other) {
      if (tuple == null) throw Error.NullArg("tuple");
      if (other == null) throw Error.NullArg("other");
      var tuplet = GetInstance(tuple.GetType());
      var othert = GetInstance(other.GetType());
      if (tuplet.GetHashCode(tuple) != othert.GetHashCode(other)) return false;
      if (!tuplet.SameHeading(othert)) return false;
      var tfields = tuplet._members;
      var ofields = othert._members;
      // same heading and fields were sorted so compare in order
      for (var x = 0; x < tfields.Length; ++x)
        if (!GetMemberValue(tuple, tfields[x]).Equals(GetMemberValue(other, ofields[x])))
          return false;
      return true;
    }

    // Import data from an object with the same fields and types
    // Can only be done before hashcode calculated.
    static internal void ImportData(object tuple, object source) {
      if (tuple == null) throw Error.NullArg("tuple");
      if (source == null) throw Error.NullArg("source");
      //if (tuple._hashcode != 0) throw Error.NullArg("source"); // FIX:how to do?
      var tuplet = GetInstance(tuple.GetType());
      var othert = GetInstance(source.GetType());
      if (!tuplet.SameHeading(othert)) throw Error.Assert("heading mismatch");
      var tfields = tuplet._members;
      var ofields = othert._members;
      // same fields same order
      for (var x = 0; x < tfields.Length; ++x)
        SetMemberValue(tuple, tfields[x], GetMemberValue(source, ofields[x]));
    }

    // Import data from an array of objects of the correct types and a heading
    // Can only be done before hashcode calculated.
    static internal void ImportData(object tuple, object[] source, CommonField[] heading) {
      if (tuple == null) throw Error.NullArg("tuple");
      if (source == null) throw Error.NullArg("source");
      if (heading == null) throw Error.NullArg("heading");
      //if (tuple._hashcode != 0) throw Error.NullArg("source"); // FIX:how to do?
      var tuplet = GetInstance(tuple.GetType());
      var tfields = tuplet._members;
      // same fields but could be different order
      for (var x = 0; x < tfields.Length; ++x) {
        var tfname = tfields[x].Name;
        var pos = Array.FindIndex(heading, h => h.Name == tfname);
        if (pos == -1) throw Error.Invalid($"missing {tfname}");
        SetMemberValue(tuple, tfields[x], source[pos]);
      }
    }

    // get member type which could be field or property
    static internal Type GetMemberType(MemberInfo minfo) {
      if (minfo == null) throw Error.NullArg("tuple");
      return (minfo is FieldInfo) ? (minfo as FieldInfo).FieldType
        : (minfo as PropertyInfo).PropertyType;
    }

    // get member value which could be field or property
    static internal object GetMemberValue(object tuple, MemberInfo minfo) {
      return (minfo is FieldInfo) ? (minfo as FieldInfo).GetValue(tuple)
        : (minfo as PropertyInfo).GetValue(tuple);
    }

    // set member value which could be field or property
    static void SetMemberValue(object tuple, MemberInfo minfo, object value) {
      if (minfo is FieldInfo)
        (minfo as FieldInfo).SetValue(tuple, value);
      else
        (minfo as PropertyInfo).SetValue(tuple, value);
    }
  }

  ///==========================================================================
  /// <summary>
  /// Implement equality check over joined fields matching by name and type
  /// 
  /// All tuples left and right must match the tuple type for that side
  /// </summary>
  internal class JoinMatcher {
    internal TupleType LeftType;
    internal TupleType RightType;
    internal int[] Index;   // index from left to right

    internal JoinMatcher(Type left, Type right) {
      LeftType = TupleType.GetInstance(left);
      RightType = TupleType.GetInstance(right);
      // for each field in left find index of matching name in right, or -1
      Index = Enumerable.Range(0, LeftType.Fields.Length)
        .Select(x => Array.FindIndex(RightType.Fields, f => f.Name == LeftType.Fields[x].Name
                && TupleType.GetMemberType(f) == TupleType.GetMemberType(LeftType.Fields[x])))
        .ToArray();
    }

    internal JoinKey GetKeyLeft(object tuple) {
      var values = Enumerable.Range(0, Index.Length).Where(x => Index[x] != -1)
        .Select(x => TupleType.GetMemberValue(tuple, LeftType.Fields[x])).ToArray();
      return new JoinKey(values);
    }

    internal JoinKey GetKeyRight(object tuple) {
      var values = Enumerable.Range(0, Index.Length).Where(x => Index[x] != -1)
        .Select(x => TupleType.GetMemberValue(tuple, RightType.Fields[Index[x]])).ToArray();
      return new JoinKey(values);
    }
  }

  ///==========================================================================
  /// <summary>
  /// Key used for indexing and comparing during join
  /// </summary>
  internal class JoinKey {
    object[] _values;
    internal JoinKey(object[] values) {
      _values = values;
    }

    public override bool Equals(object obj) {
      var other = obj as JoinKey;
      return other != null && _values.SequenceEqual(other._values);
    }

    public override int GetHashCode() {
      return _values.Length > 0 ? _values[0].GetHashCode() : -1;
    }

    public override string ToString() {
      return _values.Join(",");
    }
  }

  ///==========================================================================
  /// <summary>
  /// Derived hashset with special comparer
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class TupleSet<T> : HashSet<object> where T : class {
    internal TupleSet() : base(new TupleComparer<T>()) { }

    internal TupleSet(IRelatable<T> source) : this() {
      foreach (var item in source)
        Add(item);
    }
  }

  ///==========================================================================
  /// <summary>
  /// Implement a comparer that delegates to the correct tuple type
  /// </summary>
  internal class TupleComparer<T> : IEqualityComparer<object> {

    public new bool Equals(object tuple, object other) {
      return TupleType.AreEqual(tuple, other);
    }

    public int GetHashCode(object tuple) {
      return TupleType.GetInstance(tuple.GetType()).GetHashCode(tuple);
    }
  }

  ///==========================================================================
  /// <summary>
  /// Simplified multi value dictionary, mostly for joins
  /// </summary>
  internal class SimpleMultiDictionary<Tkey, Tvalue> {
    Dictionary<Tkey, List<Tvalue>> _dict = new Dictionary<Tkey, List<Tvalue>>();

    internal bool Add(Tkey key, Tvalue value) {
      List<Tvalue> values;
      if (_dict.TryGetValue(key, out values)) {
        values.Add(value);
        return false;
      }
      _dict.Add(key, new List<Tvalue> { value });
      return true;
    }

    internal IEnumerable<Tkey> Keys() {
      foreach (var item in _dict.Keys)
        yield return item;
    }

    internal IEnumerable<Tvalue> Values(Tkey key) {
      List<Tvalue> values;
      if (_dict.TryGetValue(key, out values)) {
        foreach (var item in values)
          yield return item;
      }
    }
  }

  ///==========================================================================
  /// <summary>
  /// Base class for an immutable tuple type
  /// 
  /// Supports hash code and equals so we don't have to compute it every time
  /// To be specialised by template
  /// </summary>
  public class TupleValue {
    public TupleType TupleType { get { return _tupletype; } }
    internal TupleHeading Heading { get { return _tupletype.Heading; } }

    TupleType _tupletype;
    protected int _hashcode;

    // Compute tuple type and underlying heading
    protected TupleValue(Type type) {
      _tupletype = TupleType.GetInstance(type);
    }

    // Check that hash code was computed on first use
    // Deferred until data is available
    public override int GetHashCode() {
      if (_hashcode == 0) throw Error.Invalid("GetHashCode");
      return _hashcode;
    }

    // Compute hash code on first use when called by specialisation
    protected int GetHashCode(TupleValue tuple) {
      if (_hashcode == 0) _hashcode = TupleType.GetHashCode(tuple);
      return _hashcode;
    }

    // Override Equals and delegate
    public override bool Equals(object obj) {
      var other = obj as TupleValue;
      if (other == null) throw Error.Invalid("obj");
      return TupleType.AreEqual(this, other);
    }

    // Delegate
    public override string ToString() {
      return TupleType.ToString(this);
    }
  }

  ///==========================================================================
  /// <summary>
  /// template specialisation for tuple
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TupleValue<T> : TupleValue, IEquatable<TupleValue<T>> where T : class {

    public TupleValue() : base(typeof(T)) { }

    public bool Equals(TupleValue<T> other) {
      return Equals(this, other as TupleValue);
    }

    // Will compute hash code on first use
    public override int GetHashCode() {
      return GetHashCode(this);
    }
  }

}

