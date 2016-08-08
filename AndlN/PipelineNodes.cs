using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlN {
  // internal codes for multi-purpose nodes
  internal enum CompareKind { Equal, Subset, Disjoint };
  internal enum SetKind { Union, Minus, Intersect, Difference };
  internal enum JoinKind { Semijoin, Antijoin };
  internal enum UpdateKind { Insert, Delete };
  internal enum AllAnyKind { All, Any };

  ///---------------------------------------------------------------------------
  /// <summary>
  /// Root for tree of pipeline nodes built by extension functions
  /// 
  /// Holder for source
  /// </summary>
  abstract internal class NdlRootNode<Tsource, Tresult> : IRelatable, IRelatable<Tresult>
    where Tsource : class where Tresult : class {

    public TupleType TupleType { get { return _tupletype; } }
    TupleType _tupletype;

    // holder for source
    protected IRelatable<Tsource> _source;

    //--- ctor
    protected NdlRootNode(IRelatable<Tsource> source) {
      _source = source;
      _tupletype = TupleType.GetInstance(typeof(Tresult));
    }

    //--- interfaces
    public virtual IEnumerator<Tresult> GetEnumerator() {
      throw NdlError.MustOverride("GetEnumerator");
      }

    IEnumerator IRelatable.GetEnumerator() {
      return GetEnumeratorPrivate();
    }

    // trick private function to implement generic and non-generic enumerator
    IEnumerator GetEnumeratorPrivate() {
      return this.GetEnumerator();
    }

    // override
    public override bool Equals(object obj) {
      var other = obj as IRelatable;
      if (other == null) return false;
      return CommonStatic.Compare(this, other, CompareKind.Equal); // CHECK: immediate execution?
    }

    // override
    public override int GetHashCode() {
      return _source.GetHashCode();
    }

    // override
    public override string ToString() {
      return $"node:{_source}";
    }
  }

  ///---------------------------------------------------------------------------
  /// <summary>
  /// Root for end node in tree of pipeline nodes built by extension functions
  /// </summary>
  abstract internal class NdlEndNode<Tsource, Tvalue>
    where Tsource : class {

    protected IRelatable<Tsource> _source;

    //--- ctor
    protected NdlEndNode(IRelatable<Tsource> source) {
      _source = source;
    }

    public abstract Tvalue Evaluate();

    public override string ToString() {
      return $"end:{typeof(Tvalue).Name}";
    }
  }

  //abstract internal class NdlConversionNode<Tsource, Tresult>
  //  where Tsource : class {

  //  protected INdlRelatable<Tsource> _source;

  //  //--- ctor
  //  protected NdlConversionNode(INdlRelatable<Tsource> source) {
  //    _source = source;
  //  }

  //  public abstract Tvalue Evaluate();

  //  public override string ToString() {
  //    return $"end:{typeof(Tvalue).Name}";
  //  }

  //---------------------------------------------------------------------------
  // Count node
  internal class NdlCountNode<Tsource> : NdlEndNode<Tsource, int>
  where Tsource : class {

    public NdlCountNode(IRelatable<Tsource> source) : base(source) {
    }

    public override int Evaluate() {
      return CommonStatic.Count(_source);
    }
  }

  //---------------------------------------------------------------------------
  // IsEmpty/Exists node
  internal class NdlExistsNode<Tsource> : NdlEndNode<Tsource, bool>
  where Tsource : class {

    bool _exists;

    public NdlExistsNode(IRelatable<Tsource> source, bool exists) : base(source) {
      _exists = exists;
    }

    public override bool Evaluate() {
      return CommonStatic.Exists(_source) == _exists;
    }
  }

  //---------------------------------------------------------------------------
  // Single node
  //
  internal class NdlSingleNode<Tsource> : NdlEndNode<Tsource, Tsource>
  where Tsource : class {

    public NdlSingleNode(IRelatable<Tsource> source) : base(source) { }

    public override Tsource Evaluate() {
      return CommonStatic.Single(_source);
    }
  }

  //---------------------------------------------------------------------------
  // Contains node
  //
  internal class NdlContainsNode<Tsource> : NdlEndNode<Tsource, bool>
    where Tsource : class {

    Tsource _tuple;


    public NdlContainsNode(IRelatable<Tsource> source, Tsource tuple) : base(source) {
      _source = source;
      _tuple = tuple;
    }

    public override bool Evaluate() {
      foreach (Tsource item in _source) {
        if (TupleType.AreEqual(item, _tuple))
          return true;
      }
      return false;
    }
  }

  //---------------------------------------------------------------------------
  // IsAllAny node
  //
  internal class NdlAllAnyNode<Tsource> : NdlEndNode<Tsource, bool>
    where Tsource : class {

    Func<Tsource, bool> _predicate;
    AllAnyKind _kind;


    public NdlAllAnyNode(IRelatable<Tsource> source, Func<Tsource, bool> predicate, AllAnyKind kind) : base(source) {
      _source = source;
      _predicate = predicate;
      _kind = kind;
    }

    public override bool Evaluate() {
      var ok = (_kind == AllAnyKind.All);
      foreach (Tsource item in _source) {
        var match = _predicate(item);
        if (match != ok)
          return !ok;
      }
      return ok;
    }
  }

  //---------------------------------------------------------------------------
  // Compare node
  internal class NdlCompareNode<Tsource, Tother> : NdlEndNode<Tsource, bool>
    where Tsource : class where Tother : class {

    IRelatable<Tother> _other;
    CompareKind _kind;

    public NdlCompareNode(IRelatable<Tsource> source, IRelatable<Tother> other, CompareKind kind) : base(source) {
      _other = other;
      _kind = kind;
    }

    public override bool Evaluate() {
      return CommonStatic.Compare(_source, _other, _kind);
    }
  }

  //---------------------------------------------------------------------------
  // Where node
  internal class NdlWhereNode<Tsource> : NdlRootNode<Tsource, Tsource>
    where Tsource : class {

    Func<Tsource, bool> _predicate;

    public NdlWhereNode(IRelatable<Tsource> source, Func<Tsource, bool> predicate) : base(source) {
      _source = source;
      _predicate = predicate;
    }

    public override IEnumerator<Tsource> GetEnumerator() {
      // enumerate, passing or dropping each tuple
      foreach (Tsource item in _source) {
        if (_predicate(item as Tsource))
          yield return item;
      }
    }
  }

  //---------------------------------------------------------------------------
  // Order by node
  internal class NdlOrderByNode<Tsource> : NdlRootNode<Tsource, Tsource>, IComparer<Tsource>
    where Tsource : class {

    Func<Tsource, Tsource, bool> _comparer;

    public NdlOrderByNode(IRelatable<Tsource> source, Func<Tsource, Tsource, bool> comparer) : base(source) {
      _source = source;
      _comparer = comparer;
    }

    // enumerate, putting tuples in order
    public override IEnumerator<Tsource> GetEnumerator() {
      var tuples = new SortedSet<Tsource>(this as IComparer<Tsource>);
      foreach (Tsource item in _source)
        tuples.Add(item);
      foreach (Tsource item in tuples)
        yield return item;
    }

    // Comparer to use in sort
    // Handle equals, call predicate for 'in order'
    int IComparer<Tsource>.Compare(Tsource x, Tsource y) {
      if (x.Equals(y)) return 0;
      return _comparer(x as Tsource, y as Tsource) ? -1 : 1;
    }
  }

  //---------------------------------------------------------------------------
  // Skip take node
  internal class NdlSkipTakeNode<Tsource> : NdlRootNode<Tsource, Tsource>
    where Tsource : class {

    int _skip;
    int _take;

    public NdlSkipTakeNode(IRelatable<Tsource> source, int skip, int take) : base(source) {
      _source = source;
      _skip = skip;
      _take = take;
    }

    public override IEnumerator<Tsource> GetEnumerator() {

      // enumerate, passing or dropping each tuple
      int nskip = 0;
      int ntake = 0;
      foreach (Tsource item in _source) {
        if (nskip < _skip) ++nskip;
        else if (ntake < _take) {
          yield return item;
          ntake++;
        }
      }
    }
  }

  ///---------------------------------------------------------------------------
  /// Select node
  /// 
  /// Transform source tuple to result, de-dupe
  /// 
  internal class NdlSelectNode<Tsource, Tresult> : NdlRootNode<Tsource, Tresult>
    where Tsource : class where Tresult : class {
    Func<Tsource, Tresult> _select;

    public NdlSelectNode(IRelatable<Tsource> source, Func<Tsource, Tresult> select) : base(source) {
      _select = select;
    }

    public override IEnumerator<Tresult> GetEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      // enumerate, transforming each tuple and handling duplicates
      foreach (Tsource item in _source) {
        var newitem = _select(item as Tsource) as Tresult;
        if (set.Add(newitem)) yield return newitem;
      }
    }
  }

  ///---------------------------------------------------------------------------
  /// Select node with aggregation
  /// 
  /// Group source tuples; optionally perform aggregation
  /// 
  internal class NdlGroupNode<Tsource, Tgroup, Taggregate, Tresult> : NdlRootNode<Tsource, Tresult>
    where Tsource : class where Tgroup : class where Tresult : class {

    Func<Tsource, Tgroup> _grouper;
    Taggregate _seed;
    Func<Tsource, Taggregate, Taggregate> _aggregator;
    Func<Tgroup, Taggregate, Tresult> _selector;

    public NdlGroupNode(IRelatable<Tsource> source, Func<Tsource, Tgroup> group, Taggregate seed,
      Func<Tsource, Taggregate, Taggregate> aggregate, Func<Tgroup, Taggregate, Tresult> select) : base(source) {
      _grouper = group;
      _seed = seed;
      _aggregator = aggregate;
      _selector = select;
    }

    public override IEnumerator<Tresult> GetEnumerator() {
      // no grouper? Just an aggregation
      if (_grouper == null) {
        if (_aggregator == null) throw NdlError.NullArg("grouper and aggregator");
        Taggregate aggregate = _seed;
        foreach (var item in _source) {
          aggregate = _aggregator(item as Tsource, aggregate);
          if (aggregate == null) throw NdlError.NullArg("aggregate result");
        }
        var newitem = _selector(null, aggregate) as Tresult;
        if (newitem == null) throw NdlError.NullArg("selector result");
        yield return newitem;
      } else {
        // grouper: keep groups and aggregates in dictionary
        Dictionary<Tgroup, Taggregate> dict = new Dictionary<Tgroup, Taggregate>();

        // enumerate, transforming each tuple and handling duplicates
        foreach (var item in _source) {
          Taggregate aggregate = _seed;
          var grouped = _grouper(item as Tsource);
          if (grouped == null) throw NdlError.NullArg("group result");
          if (_aggregator == null) {
            dict[grouped] = aggregate;
          } else {
            if (dict.TryGetValue(grouped, out aggregate))
              dict[grouped] = _aggregator(item as Tsource, aggregate);
            else {
              aggregate = _aggregator(item as Tsource, _seed);
              if (aggregate == null) throw NdlError.NullArg("aggregate result");
              dict.Add(grouped, aggregate);
            }
          }
        }
        foreach (var entry in dict) {
          var newitem = _selector(entry.Key, entry.Value) as Tresult;
          if (newitem == null) throw NdlError.NullArg("selector result");
          yield return newitem;
        }
      }
    }
  }

  ///---------------------------------------------------------------------------
  /// While node
  /// 
  /// Iterate over source until no more to add
  /// 
  internal class NdlWhileNode<Tsource> : NdlRootNode<Tsource, Tsource>
    where Tsource : class {
    Func<Tsource, IRelatable<Tsource>> _select;

    public NdlWhileNode(IRelatable<Tsource> source, Func<Tsource, IRelatable<Tsource>> select) : base(source) {
      _select = select;
    }

    public override IEnumerator<Tsource> GetEnumerator() {
      // fill store with seed
      RelationStore<Tsource> store = new RelationStore<Tsource>(_source);

      // enumerate store, transforming each tuple and adding back to store
      // done when enumeration exhausted
      foreach (Tsource item in store) {
        var newitems = _select(item as Tsource);
        foreach (var newitem in newitems)
          store.DirectAdd(newitem);
        yield return item;
      }
    }
  }

  ///---------------------------------------------------------------------------
  /// Set dyadic node
  /// 
  /// Set operations on two streams. On output, other must be converted to same type as source.
  /// 
  internal class NdlSetNode<Tsource, Tother> : NdlRootNode<Tsource, Tsource>
    where Tsource : class where Tother : class {

    IRelatable<Tother> _other;
    Func<Tother, Tsource> _converter;
    Func<Tother, Tsource> _nullconverter = (t) => t as Tsource;
    SetKind _kind;

    public NdlSetNode(IRelatable<Tsource> source, IRelatable<Tother> other, SetKind kind, Func<Tother, Tsource> converter) : base(source) {
      _other = other;
      _converter = converter ?? _nullconverter;
      _kind = kind;
    }

    public override IEnumerator<Tsource> GetEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      switch (_kind) {
      case SetKind.Union: return GetUnionEnumerator();
      case SetKind.Minus: return GetMinusEnumerator();
      case SetKind.Intersect: return GetIntersectEnumerator();
      case SetKind.Difference: return GetDifferenceEnumerator();
      }
      throw NdlError.Invalid("set kind");
    }

    // Union
    public IEnumerator<Tsource> GetUnionEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      foreach (var item in _source)
        if (set.Add(item)) yield return item;
      foreach (var item in _other)
        if (set.Add(item)) yield return _converter(item);
    }

    // Minus
    public IEnumerator<Tsource> GetMinusEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      foreach (var item in _other)
        set.Add(item);
      foreach (var item in _source)
        if (!set.Contains(item)) yield return item;
    }

    // Intersect
    IEnumerator<Tsource> GetIntersectEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      foreach (var item in _other)
        set.Add(item);
      foreach (var item in _source)
        if (set.Contains(item)) yield return item as Tsource;
    }

    // Symmetrical difference
    IEnumerator<Tsource> GetDifferenceEnumerator() {
      TupleSet<Tsource> set = new TupleSet<Tsource>();

      foreach (var item in _source)
        set.Add(item);
      foreach (var item in _other)
        if (set.Contains(item)) set.Remove(item);
        else yield return _converter(item);
      foreach (var item in set)
        yield return item as Tsource;
    }

  }

  /// <summary>
  /// A node to implement a naive join algorithm
  /// </summary>
  /// <typeparam name="Tsource">Type of source (left) argument</typeparam>
  /// <typeparam name="Tother">Type of other (right) argument</typeparam>
  /// <typeparam name="Tresult">Type of result</typeparam>
  internal class NdlJoinNode<Tsource, Tother, Tresult> : NdlRootNode<Tsource, Tresult>
    where Tsource : class where Tresult : class where Tother : class {

    IRelatable<Tother> _other;
    Func<Tother, Tresult> _result_o;
    Func<Tsource, Tother, Tresult> _result_so;

    public NdlJoinNode(IRelatable<Tsource> source, IRelatable<Tother> other,
                      Func<Tsource, Tother, Tresult> result_so, Func<Tother, Tresult> result_o) : base(source) {
      if (other == null) throw NdlError.Invalid("other");
      if (result_so == null && result_o == null) throw NdlError.Invalid("result");
      _other = other;
      _result_so = result_so;
      _result_o = result_o;
    }

    // Join enumerator, with duplicate elimination
    public override IEnumerator<Tresult> GetEnumerator() {
      TupleSet<Tresult> resultset = new TupleSet<Tresult>();

      foreach (var pair in CommonStatic.JoinIterator(_source, _other)) { 
        var result = (_result_so == null) ? _result_o(pair.Item2) as Tresult
          : _result_so(pair.Item1, pair.Item2) as Tresult;
        if (result == null) throw NdlError.Invalid("result");
        if (resultset.Add(result)) yield return result;
      }
    }
  }

  internal class NdlSemijoinNode<Tsource, Tother, Tresult> : NdlRootNode<Tsource, Tresult>
    where Tsource : class where Tother : class where Tresult : class {

    IRelatable<Tother> _other;
    Func<Tsource, Tresult> _result_func;
    JoinKind _kind;

    public NdlSemijoinNode(IRelatable<Tsource> source, IRelatable<Tother> other,
                      Func<Tsource, Tresult> result, JoinKind kind) : base(source) {
      _other = other;
      _result_func = result;
      _kind = kind;
    }

    // Join enumerator, with duplicate elimination
    public override IEnumerator<Tresult> GetEnumerator() {
      HashSet<JoinKey> keyset = new HashSet<JoinKey>();
      var joiner = new JoinMatcher(typeof(Tsource), typeof(Tother));
      TupleSet<Tresult> resultset = new TupleSet<Tresult>();

      // Fill set with right tuples
      foreach (Tother right in _other) {
        var key = joiner.GetKeyRight(right);
        keyset.Add(key);
      }
      // Iterate over left checking each for match
      // output depends on semijoin vs antijoin
      foreach (Tsource left in _source) {
        var key = joiner.GetKeyLeft(left);
        var ismatch = keyset.Contains(key);
        if (ismatch ? _kind == JoinKind.Semijoin : _kind == JoinKind.Antijoin) {
          var result = (_result_func == null) ? left as Tresult : _result_func(left);
          if (result == null) throw NdlError.Invalid("result");
          if (resultset.Add(result)) yield return result;
        }
      }
    }
  }

  ///---------------------------------------------------------------------------
  /// Store nodes which update contents of relation variable on commit
  /// 

  ///---------------------------------------------------------------------------
  /// <summary>
  /// Root for store node at end of tree of pipeline to make changes to a relation variable
  /// </summary>
  abstract internal class NdlStoreNode<Tstore> : IStorable<Tstore>
    where Tstore : class {
    protected RelationStore<Tstore> _store;

    //--- ctor
    protected NdlStoreNode(RelationStore<Tstore> store) {
      _store = store;
    }

    public abstract void Store();

    public override string ToString() {
      return $"store:{typeof(Tstore).Name}";
    }
  }

  // Node to perform set updates: insert and delete
  internal class NdlSetUpdateNode<Tstore> : NdlStoreNode<Tstore>
  where Tstore : class {

    IRelatable<Tstore> _source;
    UpdateKind _kind;

    public NdlSetUpdateNode(RelationStore<Tstore> store, IRelatable<Tstore> source, UpdateKind kind) : base(store) {
      _source = source;
      _kind = kind;
    }

    // Do it
    public override void Store() {
      foreach (var item in _source)
        if (_kind == UpdateKind.Insert)
          _store.DirectAdd(item);
        else
          _store.DirectDelete(item);
    }
  }

  // Node to delete tuples that satisfy predicate
  internal class NdlDeleteNode<Tstore> : NdlStoreNode<Tstore>
    where Tstore : class {

    Func<Tstore, bool> _predicate;

    public NdlDeleteNode(RelationStore<Tstore> store, Func<Tstore, bool> predicate) : base(store) {
      _predicate = predicate;
    }

    // Do it
    public override void Store() {
      foreach (Tstore item in _store)
        if (_predicate(item))
          _store.DirectDelete(item);
    }
  }

  // Node to replace tuples that satisfy predicate
  internal class NdlUpdateNode<Tstore> : NdlStoreNode<Tstore>
    where Tstore : class {

    Func<Tstore, bool> _predicate;
    Func<Tstore, Tstore> _update;

    public NdlUpdateNode(RelationStore<Tstore> store, Func<Tstore, bool> predicate,
      Func<Tstore, Tstore> update) : base(store) {
      _predicate = predicate;
      _update = update;
    }

    // Make it happen
    public override void Store() {
      foreach (Tstore item in _store)
        if (_predicate(item))
          _store.DirectReplace(item, _update(item));
    }
  }
}
