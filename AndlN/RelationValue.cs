using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlN {
  ///==========================================================================
  /// <summary>
  /// Implement base class for a data relation
  /// 
  /// Null enumerator.
  /// </summary>
  public abstract class RelationValueBase : IRelatable {

    public TupleType TupleType { get { return _tupletype; } }
    TupleType _tupletype;
    protected IEnumerable _source;

    // Create a relation from a tuple type
    public RelationValueBase(Type type) {
      if (type == null) throw NdlError.NullArg("type");
      _tupletype = TupleType.GetInstance(type);
    }

    public IEnumerator GetEnumerator() {
      throw NdlError.MustOverride("GetEnumerator");
    }

    // use hash code of type because it must not change
    public override int GetHashCode() {
      return _tupletype.GetHashCode();
    }

    // delegate to extension method for most cases
    public override bool Equals(object obj) {
      var other = obj as IRelatable;
      if (other == null) return false;
      if (other == this) return true;
      return CommonStatic.Compare(this, other, CompareKind.Equal);
    }
  }

  ///==========================================================================
  /// <summary>
  /// template specialisation for tuple source
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class RelationValue<T> : RelationValueBase, IRelatable<T>, IRelatable, IEquatable<T> 
  where T : class {

    //--- ctor
    // Note importance of passing type through to base
    internal RelationValue(IEnumerable<T> source) : base(typeof(T)) {
      if (source == null) throw NdlError.NullArg("source");
      _source = source;
    }

    // implement interface
    bool IEquatable<T>.Equals(T other) {
      return base.Equals(other);
    }

    // implement interface
    IEnumerator<T> IRelatable<T>.GetEnumerator() {
      foreach (var item in _source)
        yield return item as T;
    }

    // implement interface
    IEnumerator IRelatable.GetEnumerator() {
      foreach (var item in _source)
        yield return item;
    }

  }

  ///==========================================================================
  /// <summary>
  /// External stream source for tuples
  /// </summary>
  public class RelationStream<T> : RelationValueBase, IRelatable<T>, IRelatable, IEquatable<T> 
  where T : class, new() {

    public string TableName { get { return _stream.Table; } }
    public CommonField[] Fields { get { return _stream.Fields; } }
    DataSourceStream _stream;

    //--- ctor
    // Note importance of passing type through to base
    internal RelationStream(SourceKind kind, string connector, string name, string heading = null) : base(typeof(T)) {
      if (connector == null) throw NdlError.NullArg("connector");
      _stream = DataSourceStream.Create(kind, connector);
      if (_stream == null) throw NdlError.Fatal("cannot connect to {kind} with {connector}");
      if (!_stream.SelectTable(name)) throw NdlError.Fatal("cannot find {name} on {connector}");
      if (heading != null)
        _stream.SetHeading(heading);
    }

    // implement interface
    bool IEquatable<T>.Equals(T other) {
      return base.Equals(other);
    }

    // implement interface
    public new IEnumerator<T> GetEnumerator() {
      foreach (var item in _stream) {
        var tuple = new T();
        TupleType.ImportData(tuple, item, _stream.Fields);
        yield return tuple;
      }
    }

    // implement interface
    IEnumerator IRelatable.GetEnumerator() {
      throw NdlError.NotImpl("GetEnumerator");
    }
  }

  ///==========================================================================
  /// <summary>
  /// Specialisation for a relation store, that allows updates
  /// 
  /// Implementation uses a List for the data and a Dictionary as the index
  /// Allows Add and Remove interleaved with Enumeration
  /// </summary>
  public class RelationStore<T> : RelationValueBase, IRelatable<T> 
  where T : class {

    //new NdlTupleType TupleType { get { return TupleType; } }

    List<T> _tuples = new List<T>();                        // list of tuples
    Dictionary<T, int> _index = new Dictionary<T, int>();   // index
    int _curpos = 0;                                        // current position

    //--- ctor
    // Note importance of passing type through to base
    public RelationStore() : base(typeof(T)) { }

    // initialise from IEnumerable
    public RelationStore(IEnumerable<T> source) : base(typeof(T)) {
      if (source == null) throw NdlError.NullArg("source");
      foreach (var item in source)
        DirectAdd(item);
    }

    // initialise from IRelatable
    public RelationStore(IRelatable<T> source) : base(typeof(T)) {
      if (source == null) throw NdlError.NullArg("source");
      foreach (var item in source)
        DirectAdd(item);
    }

    // implement interface
    // uses index to allow mutable collection
    public new IEnumerator GetEnumerator() {
      for (_curpos = 0; _curpos < _tuples.Count; ++_curpos)
        yield return _tuples[_curpos];
    }

    // implement interface
    // uses index to allow mutable collection
    IEnumerator<T> IRelatable<T>.GetEnumerator() {
      for (_curpos = 0; _curpos < _tuples.Count; ++_curpos)
        yield return _tuples[_curpos];
    }

    //--- implementation

    // Add item to end of list. Update index.
    // Does not affect iterator.
    internal bool DirectAdd(T item) {
      if (item == null) throw NdlError.NullArg("item");
      if (_index.ContainsKey(item)) return false;
      _tuples.Add(item);
      _index.Add(item, _tuples.Count - 1);
      return true;
    }

    // Delete item; replace by last to avoid reindex
    internal bool DirectDelete(T item) {
      if (item == null) throw NdlError.NullArg("item");
      int pos;
      if (!_index.TryGetValue(item, out pos)) return false;
      var postop = _tuples.Count - 1;
      if (pos == postop) { // remove last
        _tuples.RemoveAt(pos);
        _index.Remove(item);
      } else {  // shuffle it down
        _tuples[pos] = _tuples[postop];
        _tuples.RemoveAt(postop);
        _index.Remove(item);
        _index[_tuples[pos]] = pos;
        if (_curpos == pos) _curpos--;
      }
      return true;
    }

    // replace item -- don't get cute
    internal void DirectReplace(T item, T newitem) {
      if (item == null) throw NdlError.NullArg("item");
      if (newitem == null) throw NdlError.NullArg("newitem");
      DirectDelete(item);
      DirectAdd(newitem);
    }
  }
}

