using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlN {
  ///==========================================================================
  /// <summary>
  /// Extension methods providing the visible part of the relational algebra
  /// and associated functions
  /// </summary>
  public static class Relatable {

    ///------------------------------------------------------------------------
    /// Source functions.
    /// 
    /// Take various arguments and return a Relatable tuple source.

      // generate source from IEnumerable
    public static IRelatable<Tsource> FromSource<Tsource>(IEnumerable<Tsource> source)
    where Tsource : class {

      return new RelationValue<Tsource>(source);
    }

    // generate source as sequence of values
    public static IRelatable<Tresult> Sequence<Tresult>(int count, Func<int, Tresult> select)
    where Tresult : class {

      return new RelationValue<Tresult>(Enumerable.Range(0, count).Select(n=>select(n)));
    }

    // Generate source from Csv 
    public static IRelatable<T> FromCsv<T>(string connector, string name, string heading = null) 
    where T : class, new() {
      return new RelationStream<T>(SourceKind.Csv, connector, name, heading);
    }

    // Generate source as lines of Text
    public static IRelatable<TupleText> FromText(string connector, string name, string heading = null) {
      return new RelationStream<TupleText>(SourceKind.Text, connector, name, heading);
    }

    public class TupleText {
      public string Line;
    }

    // Generate source from Sql
    public static IRelatable<T> FromSql<T>(string connector, string name, string heading = null)
    where T : class, new() {
      return new RelationStream<T>(SourceKind.Sql, connector, name, heading);
    }

    // Generate source from Sql schema
    public static IRelatable<NdlSchemaSql> FromSqlSchema(string connector) {
      return new RelationStream<NdlSchemaSql>(SourceKind.Sql, connector, null);
    }

    public class NdlSchemaSql {
      public string TABLE_CATALOG;
      public string TABLE_SCHEMA;
      public string TABLE_NAME;
      public string TABLE_TYPE;
    }

    // Generate source from Odbc
    public static IRelatable<T> FromOdbc<T>(string connector, string name, string heading = null)
    where T : class, new() {
      return new RelationStream<T>(SourceKind.Odbc, connector, name, heading);
    }

    // Generate source from Odbc schema
    public static IRelatable<NdlSchemaOdbc> FromOdbcSchema(string connector) {
      return new RelationStream<NdlSchemaOdbc>(SourceKind.Odbc, connector, null);
    }

    public class NdlSchemaOdbc {
      public string TABLE_CAT;
      public string TABLE_SCHEM;
      public string TABLE_NAME;
      public string TABLE_TYPE;
      public string REMARKS;
    }

    // Generate source from Oledb
    public static IRelatable<T> FromOledb<T>(string connector, string name, string heading = null)
    where T : class, new() {
      return new RelationStream<T>(SourceKind.Oledb, connector, name, heading);
    }

    // Generate source from Oledb schema
    public static IRelatable<NdlSchemaOledb> FromOledbSchema(string connector) {
      return new RelationStream<NdlSchemaOledb>(SourceKind.Oledb, connector, null);
    }

    public class NdlSchemaOledb {
      public string TABLE_CATALOG;
      public string TABLE_SCHEMA;
      public string TABLE_NAME;
      public string TABLE_TYPE;
      //public string TABLE_GUID;  // TODO: conversion
      public string DESCRIPTION;
      //public string TABLE_PROPID; // TODO: conversion
      public DateTime DATE_CREATED;
      public DateTime DATE_MODIFIED;
    }

    ///------------------------------------------------------------------------
    /// Endpoint collection functions
    /// 
    /// Will enumerate and convert to a collection 
    /// 

    // convert to IEnumerable<T>
    public static IEnumerable<T> AsEnumerable<T>(this IRelatable<T> source)
    where T : class {

      return CommonStatic.AsEnumerable(source);
    }

    // convert to List<> of tuples
    public static List<T> ToList<T>(this IRelatable<T> source)
    where T : class {

      return CommonStatic.ToList(source);
    }

    // convert to Array of tuples
    public static T[] ToArray<T>(this IRelatable<T> source)
    where T : class {

      return CommonStatic.ToList(source).ToArray();
    }

    // convert to Relation store
    public static RelationStore<T> ToStore<T>(this IRelatable<T> source)
    where T : class {

      return CommonStatic.ToStore(source);
    }

    ///------------------------------------------------------------------------
    /// Endpoint value functions
    /// 
    /// Immediate functions create a node and evaluate now (will cause enumeration)
    /// 

    //--- monadic node functions

    // Return count of source
    public static int Count<T>(this IRelatable<T> source)
    where T : class {

      return new NdlCountNode<T>(source).Evaluate();
    }

    // Return first tuple of source or null
    public static T Single<T>(this IRelatable<T> source)
    where T : class {

      return new NdlSingleNode<T>(source).Evaluate();
    }

    // Return true if source is empty
    public static bool IsEmpty<T>(this IRelatable<T> source)
    where T : class {

      return new NdlExistsNode<T>(source, false).Evaluate();
    }

    // Return true if source is not empty
    public static bool Exists<T>(this IRelatable<T> source)
    where T : class {

      return new NdlExistsNode<T>(source, true).Evaluate();
    }

    // Return true if source contains tuple value
    public static bool Contains<T>(this IRelatable<T> source, T tuple)
    where T : class {

      return new NdlContainsNode<T>(source, tuple).Evaluate();
    }

    // Return true if predicate is true for every tuple
    public static bool All<T>(this IRelatable<T> source, Func<T, bool> predicate)
    where T : class {

      return new NdlAllAnyNode<T>(source, predicate, AllAnyKind.All).Evaluate();
    }

    // Return true if predicate is true for any tuple
    public static bool Any<T>(this IRelatable<T> source, Func<T, bool> predicate)
    where T : class {

      return new NdlAllAnyNode<T>(source, predicate, AllAnyKind.Any).Evaluate();
    }

    //--- dyadic node functions

    // True if source and another are equal
    public static bool IsEqual<Tsource, Tother>(this IRelatable<Tsource> source, IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      return new NdlCompareNode<Tsource, Tother>(source, other, CompareKind.Equal).Evaluate();
    }

    // True if source is subset of other
    public static bool IsSubset<Tsource, Tother>(this IRelatable<Tsource> source, IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      // note: reversed order for subset (because we really only check superset)
      return new NdlCompareNode<Tother, Tsource>(other, source, CompareKind.Subset).Evaluate();
    }

    // True if source is superset of other
    public static bool IsSuperset<Tsource, Tother>(this IRelatable<Tsource> source, IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      return new NdlCompareNode<Tsource, Tother>(source, other, CompareKind.Subset).Evaluate();
    }

    // True if source and other have no tuples in common
    public static bool IsDisjoint<Tsource, Tother>(this IRelatable<Tsource> source, IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      return new NdlCompareNode<Tsource, Tother>(source, other, CompareKind.Disjoint).Evaluate();
    }

    ///------------------------------------------------------------------------
    /// Deferred pipeline functions
    /// 
    /// Create a node for later enumeration
    /// 

    // Return source tuples only if predicate returns true
    public static IRelatable<Tresult> Where<Tresult>(this IRelatable<Tresult> source, Func<Tresult, bool> predicate)
    where Tresult : class { 

      return new NdlWhereNode<Tresult>(source, predicate);
    }

    // Return source tuples in sorted order
    // predicate function returns true if left and right tuples are in order
    public static IRelatable<Tresult> OrderBy<Tresult>(this IRelatable<Tresult> source, Func<Tresult, Tresult, bool> predicate)
    where Tresult : class {

      return new NdlOrderByNode<Tresult>(source, predicate);
    }

    // Return take tuples after skipping skip
    public static IRelatable<Tresult> SkipTake<Tresult>(this IRelatable<Tresult> source, int skip, int take)
    where Tresult : class {

      return new NdlSkipTakeNode<Tresult>(source, skip, take);
    }

    // Return tuples in new format
    // Encompasses operations of rename, project, extend
    public static IRelatable<Tresult> Select<Tsource, Tresult>(this IRelatable<Tsource> source, 
      Func<Tsource, Tresult> select)
    where Tsource : class where Tresult : class {

      return new NdlSelectNode<Tsource, Tresult>(source, select);
    }

    // Return tuples in new format in groups
    // Supports aggregation
    // Tgroup are the grouping fields, Taggregate is the aggregation type and Tresult is the result
    public static IRelatable<Tresult> Group<Tsource, Tgroup, Taggregate, Tresult>(this IRelatable<Tsource> source,
      Func<Tsource, Tgroup> group, Taggregate seed, Func<Tsource, Taggregate, Taggregate> aggregate, 
      Func<Tgroup, Taggregate, Tresult> select)
    where Tsource : class where Tgroup : class where Tresult : class {

      return new NdlGroupNode<Tsource, Tgroup, Taggregate, Tresult>(source, group, seed, aggregate, select);
    }

    // Recursive evaluation until all done and no new tuples added by the iterator
    public static IRelatable<Tsource> While<Tsource>(this IRelatable<Tsource> source, Func<Tsource, IRelatable<Tsource>> iterator)
    where Tsource : class {

      return new NdlWhileNode<Tsource>(source, iterator);
    }

    ///------------------------------------------------------------------------
    /// Set operations on matching types
    /// 

    // Return tuples from source and other merged
    public static IRelatable<Tsource> Union<Tsource>(this IRelatable<Tsource> left, IRelatable<Tsource> right)
    where Tsource : class {

      return new NdlSetNode<Tsource, Tsource>(left, right, SetKind.Union, null);
    }

    // Return tuples from source except those in other
    public static IRelatable<Tsource> Minus<Tsource>(this IRelatable<Tsource> left, IRelatable<Tsource> right)
    where Tsource : class {

      return new NdlSetNode<Tsource, Tsource>(left, right, SetKind.Minus, null);
    }

    // Return tuples common to source and other
    public static IRelatable<Tsource> Intersect<Tsource>(this IRelatable<Tsource> left, IRelatable<Tsource> right)
    where Tsource : class {

      return new NdlSetNode<Tsource, Tsource>(left, right, SetKind.Intersect, null);
    }

    // Return symmetric difference of source and other
    public static IRelatable<Tsource> Difference<Tsource>(this IRelatable<Tsource> left, IRelatable<Tsource> right)
    where Tsource : class {

      return new NdlSetNode<Tsource, Tsource>(left, right, SetKind.Difference, null);
    }

    ///------------------------------------------------------------------------
    /// Set operations on non-matching types with conversion

    // Return tuples from source and other merged
    public static IRelatable<Tsource> Union<Tsource, Tother>(this IRelatable<Tsource> left, 
      IRelatable<Tother> right, Func<Tother, Tsource> converter)
    where Tsource : class where Tother : class {

      return new NdlSetNode<Tsource, Tother>(left, right, SetKind.Union, converter);
    }

    // Return tuples from source except those in other
    public static IRelatable<Tsource> Minus<Tsource, Tother>(this IRelatable<Tsource> left, 
      IRelatable<Tother> right, Func<Tother, Tsource> converter)
    where Tsource : class where Tother : class {

      return new NdlSetNode<Tsource, Tother>(left, right, SetKind.Minus, converter);
    }

    // Return tuples common to source and other
    public static IRelatable<Tsource> Intersect<Tsource, Tother>(this IRelatable<Tsource> left, 
      IRelatable<Tother> right, Func<Tother, Tsource> converter)
    where Tsource : class where Tother : class {

      return new NdlSetNode<Tsource, Tother>(left, right, SetKind.Intersect, converter);
    }

    // Return symmetric difference of source and other
    public static IRelatable<Tsource> Difference<Tsource, Tother>(this IRelatable<Tsource> left, 
      IRelatable<Tother> right, Func<Tother, Tsource> converter)
    where Tsource : class where Tother : class {

      return new NdlSetNode<Tsource, Tother>(left, right, SetKind.Difference, converter);
    }

    ///------------------------------------------------------------------------
    /// Joins: natural, semijoin, antijoin
    /// 

    // Natural join (by name and value), return tuples derived from source and other
    public static IRelatable<Tresult> Join<Tsource, Tother, Tresult>(this IRelatable<Tsource> source,
      IRelatable<Tother> other, Func<Tsource, Tother, Tresult> result)
    where Tsource : class where Tresult : class where Tother : class {

      return new NdlJoinNode<Tsource, Tother, Tresult>(source, other, result, null);
    }

    // Natural join (by name and value), return tuples derived from other only
    public static IRelatable<Tresult> Join<Tsource, Tother, Tresult>(this IRelatable<Tsource> source,
      IRelatable<Tother> other, Func<Tother, Tresult> result)
    where Tsource : class where Tresult : class where Tother : class {

      return new NdlJoinNode<Tsource, Tother, Tresult>(source, other, null, result);
    }

    // Natural semijoin (by name and value) in new tuples derived from source
    public static IRelatable<Tresult> Semijoin<Tsource, Tother, Tresult>(this IRelatable<Tsource> source,
      IRelatable<Tother> other, Func<Tsource, Tresult> result)
    where Tsource : class where Tresult : class where Tother : class {

      return new NdlSemijoinNode<Tsource, Tother, Tresult>(source, other, result, JoinKind.Semijoin);
    }

    // Natural semijoin (by name and value) as source tuples
    public static IRelatable<Tsource> Semijoin<Tsource, Tother>(this IRelatable<Tsource> source,
      IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      return new NdlSemijoinNode<Tsource, Tother, Tsource>(source, other, null, JoinKind.Semijoin);
    }

    // Natural antijoin (by name and value) in new tuples derived from source
    public static IRelatable<Tresult> Antijoin<Tsource, Tother, Tresult>(this IRelatable<Tsource> source,
      IRelatable<Tother> other, Func<Tsource, Tresult> result)
    where Tsource : class where Tresult : class where Tother : class {

      return new NdlSemijoinNode<Tsource, Tother, Tresult>(source, other, result, JoinKind.Antijoin);
    }

    // Natural antijoin (by name and value) as source tuples
    public static IRelatable<Tsource> Antijoin<Tsource, Tother>(this IRelatable<Tsource> source,
      IRelatable<Tother> other)
    where Tsource : class where Tother : class {

      return new NdlSemijoinNode<Tsource, Tother, Tsource>(source, other, null, JoinKind.Antijoin);
    }

    ///------------------------------------------------------------------------
    /// Deferred update functions
    /// 
    /// Create a node for later update
    /// 

    // Insert tuples
    public static IStorable<Tstore> Insert<Tstore>(this RelationStore<Tstore> store, IRelatable<Tstore> source)
    where Tstore : class {

      return new NdlSetUpdateNode<Tstore>(store, source, UpdateKind.Insert);
    }

    // Delete tuples
    public static IStorable<Tstore> Delete<Tstore>(this RelationStore<Tstore> store, IRelatable<Tstore> source)
    where Tstore : class {

      return new NdlSetUpdateNode<Tstore>(store, source, UpdateKind.Delete);
    }

    // Delete by predicate
    public static IStorable<Tstore> Delete<Tstore>(this RelationStore<Tstore> store, Func<Tstore, bool> predicate)
    where Tstore : class {

      return new NdlDeleteNode<Tstore>(store, predicate);
    }

    // Update by predicate
    public static IStorable<Tstore> Update<Tstore>(this RelationStore<Tstore> store,
      Func<Tstore, bool> predicate, Func<Tstore, Tstore> update)
    where Tstore : class {

      return new NdlUpdateNode<Tstore>(store, predicate, update);
    }
  }
}
