using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlN {
  ///==========================================================================
  /// Static methods with algorithms called from pipeline nodes but not used directly
  /// 

  internal static class CommonStatic {

    // Compare two streams, return true if other is equal to, or subset of, or disjoint from source
    internal static bool Compare(IRelatable source, IRelatable other, CompareKind kind) {
      TupleSet<object> set = new TupleSet<object>();

      if (!source.TupleType.SameHeading(other.TupleType)) return false;

      // left argument
      foreach (var item in source)
        set.Add(item);

      // right argument
      var count = 0;
      if (kind == CompareKind.Disjoint) {
        foreach (var item in other)
          if (set.Contains(item)) return false;
        return true;
      }
      foreach (var item in other) {
        if (++count > set.Count) break;  // enough to decide
        if (!set.Contains(item)) return false;
      }
      return (kind == CompareKind.Equal) ? count == set.Count : count <= set.Count;
    }

    // algorithm for natural join
    internal static IEnumerable<Tuple<Tsource, Tother>> JoinIterator<Tsource, Tother>(
      IRelatable<Tsource> source, IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      var joiner = new JoinMatcher(typeof(Tsource), typeof(Tother));
      SimpleMultiDictionary<JoinKey, Tother> _joinindex = new SimpleMultiDictionary<JoinKey, Tother>();

      // Enumerate, transforming each matching pair into one output
      foreach (Tother right in other) {
        var key = joiner.GetKeyRight(right);
        _joinindex.Add(key, right);
      }

      foreach (Tsource left in source) {
        var key = joiner.GetKeyLeft(left);
        foreach (var right in _joinindex.Values(key)) {
          yield return Tuple.Create(left, right);
        }
      }
    }

    // return true if source is not empty
    internal static bool Exists<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {
      foreach (var item in source)
        return true;
      return false;
    }

    // return count of source
    internal static int Count<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {

      int count = 0;
      foreach (var item in source)
        count++;
      return count;
    }

    // Single returns first tuple of source, or null
    internal static Tsource Single<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {

      foreach (var item in source)
        return item;
      return null;
    }

    // convert to IEnumerable
    internal static IEnumerable<Tsource> AsEnumerable<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {

      var list = new List<Tsource>();
      foreach (var item in source)
        yield return item;
    }

    // convert to list
    internal static List<Tsource> ToList<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {

      var list = new List<Tsource>();
      foreach (var item in source)
        list.Add(item);
      return list;
    }

    // convert to store
    internal static RelationStore<Tsource> ToStore<Tsource>(IRelatable<Tsource> source)
    where Tsource : class {

      return new RelationStore<Tsource>(source);
    }
  }
}
