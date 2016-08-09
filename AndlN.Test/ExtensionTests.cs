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
using NUnit.Framework;
using AndlN;

namespace AndlN.RelatableTest {
  [TestFixture]
  public class ExtensionTests {

    // monadic operations on relations
    [TestCase]
    public void CountIsEmpty() {
      CheckCountIsEmpty(TD_None.NDR_0(), 0, true);
      CheckCountIsEmpty(TD_None.NDR_1(), 1, false);
      CheckCountIsEmpty(TD_sA2_iA1_dA3.NDR_0(), 0, true);
      CheckCountIsEmpty(TD_sA2_iA1_dA3.NDR_2xy(), 2, false);
      CheckCountIsEmpty(TD_sA2_iA1_dA3.NDR_2xz(), 2, false);
      CheckCountIsEmpty(TD_sA2_iA1_dA3.NDR_16wxyz(), 16, false);
    }

    void CheckCountIsEmpty<T>(IRelatable<T> rel, int count, bool isempty, int recurse = 0) where T :class {
      Assert.AreEqual(count, rel.Count());
      Assert.AreEqual(count, rel.ToList().Count);
      Assert.AreEqual(count, rel.ToArray().Length);
      Assert.AreEqual(isempty, rel.IsEmpty());
      Assert.AreEqual(!isempty, rel.Exists());
      if (recurse == 0) CheckCountIsEmpty(rel.Where(t => true), count, isempty, 1);
      if (recurse == 0) CheckCountIsEmpty(rel.Where(t => false), 0, true, 1);
      if (recurse == 0) CheckCountIsEmpty(new RelationStore<T>(rel), count, isempty, 1);
      if (recurse == 0) CheckCountIsEmpty(rel.ToStore(), count, isempty, 1);
    }

    [TestCase]
    public void CompareEquals() {
      CheckEqual(TD_sA2_iA1_dA3.NDR_2xy(), TD_dA3_sA2_iA1.NDR_2xy(), true);
      CheckEqual(TD_sA2_iA1_dA3.NDR_2xy(), TD_sA2_iA1_dA3.NDR_0(), false);
      CheckEqual(TD_sA2_iA1_dA3.NDR_2xy(), TD_sA2_iA1_dA3.NDR_16wxyz(), false);
      CheckEqual(TD_dA3_sA2_iA1.NDR_2xy(), TD_sA2_iA1_dA3.NDR_0(), false);
    }

    void CheckEqual<T1,T2>(IRelatable<T1> rel1, IRelatable<T2> rel2, bool isequal, int recurse = 0) 
    where T1 : class where T2 : class {

      Assert.IsTrue(rel1.Equals(rel1));
      Assert.IsTrue(rel1.IsEqual(rel1));
      Assert.AreEqual(rel1, rel1);

      if (isequal) {
        Assert.IsTrue(rel1.Equals(rel2));
        Assert.IsTrue(rel1.IsEqual(rel2));
        Assert.AreEqual(rel1, rel2);
        Assert.AreEqual(rel1.Count(), rel2.Count());
      } else {
        Assert.IsFalse(rel1.Equals(rel2));
        Assert.IsFalse(rel1.IsEqual(rel2));
        Assert.AreNotEqual(rel1, rel2);
        // count unknown
      }

      if (recurse < 1) CheckEqual(rel2, rel1, isequal, 1);
      if (recurse < 2) CheckEqual(rel1.ToStore(), rel2, isequal, 2);
      if (recurse < 2) CheckEqual(rel1.Where(t => true), rel2, isequal, 2);
      if (recurse < 2) CheckEqual(rel1, rel2.Where(t => true), isequal, 2);
      if (recurse < 2) CheckEqual(rel1.Where(t => true), rel2.Where(t => true), isequal, 2);
    }

    [TestCase]
    public void Subset() {
      CheckSubset(TD_sA2_iA1_dA3.NDR_0(), TD_sA2_iA1_dA3.NDR_2xy(), true);
      CheckSubset(TD_sA2_iA1_dA3.NDR_0(), TD_sA2_iA1_dA3.NDR_16wxyz(), true);
      CheckSubset(TD_sA2_iA1_dA3.NDR_2xy(), TD_sA2_iA1_dA3.NDR_16wxyz(), true);

      CheckSubset(TD_sA2_iA1_dA3.NDR_0(), TD_dA3_sA2_iA1.NDR_2xy(), true);
      CheckSubset(TD_dA3_sA2_iA1.NDR_2xy(), TD_sA2_iA1_dA3.NDR_16wxyz(), true);

      CheckSubset(TD_sA2_iA1_dA3.NDR_2xy(), TD_sA2_iA1_dA3.NDR_2xz(), false);
      CheckSubset(TD_sA2_iA1_dA3.NDR_2xy(), TD_iA1_sA2_b_A3.NDR_2xy(), false);
    }

    [TestCase]
    public void Contains() {
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_2xy().Contains(TD_sA2_iA1_dA3.NDR_16_in()));
      Assert.AreEqual(false, TD_sA2_iA1_dA3.NDR_2xy().Contains(TD_sA2_iA1_dA3.NDR_16_out()));
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_16wxyz().Contains(TD_sA2_iA1_dA3.NDR_16_in()));
      Assert.AreEqual(false, TD_sA2_iA1_dA3.NDR_16wxyz().Contains(TD_sA2_iA1_dA3.NDR_16_out()));

      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_16wxyz().All(t => TD_sA2_iA1_dA3.NDR_16wxyz().Contains(t)));
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_2xy().All(t => TD_sA2_iA1_dA3.NDR_16wxyz().Contains(t)));
      Assert.AreEqual(false, TD_sA2_iA1_dA3.NDR_2xz().All(t => TD_sA2_iA1_dA3.NDR_16wxyz().Contains(t)));
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_2xz().Any(t => TD_sA2_iA1_dA3.NDR_16wxyz().Contains(t)));

    }

    // Call with rel1 proper subset of rel2 or disjoint, never equal
    void CheckSubset<T1, T2>(IRelatable<T1> rel1, IRelatable<T2> rel2, bool issub, int recurse = 0)
    where T1 : class where T2 : class {

      Assert.AreEqual(true, rel1.IsSubset(rel1));
      Assert.AreEqual(true, rel2.IsSubset(rel2));
      Assert.AreEqual(false, rel2.IsSubset(rel1));
      Assert.AreEqual(issub, rel1.IsSubset(rel2));

      if (recurse < 1) CheckSubset(rel1.ToStore(), rel2, issub, 1);
      if (recurse < 1) CheckSubset(rel1.Where(t => true), rel2, issub, 1);
      if (recurse < 1) CheckSubset(rel1, rel2.Where(t => true), issub, 1);
      if (recurse < 1) CheckSubset(rel1.Where(t => true), rel2.Where(t => true), issub, 1);
    }

    [TestCase]
    public void AllAny() {
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_16wxyz().All(t => t.A1 >= 42));
      Assert.AreEqual(false, TD_sA2_iA1_dA3.NDR_16wxyz().All(t => t.A1 == 42));
      Assert.AreEqual(true, TD_sA2_iA1_dA3.NDR_16wxyz().Any(t => t.A1 == 42));
      Assert.AreEqual(false, TD_sA2_iA1_dA3.NDR_16wxyz().Any(t => t.A1 < 42));
    }

    [TestCase]
    public void WhereFilterCount() {
      var v1 = TD_sA2_iA1_dA3.NDR_16wxyz();

      CheckCountEqual(0, TD_None.NDR_0());
      CheckCountEqual(0, TD_None.NDR_1().Where(t => false));
      CheckCountEqual(16, v1);
      CheckCountEqual(8, v1.Where(t => t.A1 == 42));
      CheckCountEqual(8, v1.Where(t => t.A2.Length == 5));
      CheckCountEqual(4, v1.Where(t => t.A2 == "goodbye"));
      CheckCountEqual(5, v1.Where(t => t.A3 > 1.0m));
    }

    [TestCase]
    public void Select() {
      var v1 = TD_sA2_iA1_dA3.NDR_16wxyz();
      var v2 = v1.Select(t => new { t.A2, t.A1, t.A3 });  // anon, same order
      var v3 = v1.Select(t => new { t.A1, t.A2, t.A3 });  // anon, different order
      var v4 = v1.Select(t => new { t.A1, t.A2, A4 = t.A3 });  // anon, different

      CheckEqual(v1, v2, true);
      CheckEqual(v1, v3, true);
      CheckEqual(v2, v3, true);
      CheckEqual(v1, v4, false);
      CheckEqual(v2, v4, false);
      CheckEqual(v3, v4, false);
      //CheckCountEqual(16, v1);
      //CheckCountEqual(16, v2);
      //Assert.AreEqual(true, v1.IsEqual(v2));
      //Assert.AreEqual(true, v1.Equals(v2));
      //Assert.AreEqual(v1, v2); 
      CheckCountEqual(8, v1.Select(t => new { A1 = t.A1, A2 = t.A2, A3 = 0m }));
      CheckCountEqual(2, v1.Select(t => new { A1 = t.A1, A2 = "xxx", A3 = 0m }));
      CheckCountEqual(4, v1.Select(t => new { A2 = t.A2 }));

      var v0b = TD_None.NDR_1();
      Assert.AreEqual(v0b, v1.Select(t => new { }));
    }

    [TestCase]
    public void SetToSet() {
      var v1 = TD_sA2_iA1_dA3.NDR_2xy();
      var v3 = TD_sA2_iA1_dA3.NDR_16wxyz();
      var v2 = v3.Where(t => t.A1 == 88);

      CheckCountEqual(2, v1);
      CheckCountEqual(8, v2);
      CheckCountEqual(16, v3);

      CheckTwoSubsets(v3, v1, v2);
      //CheckTwoSubsets(v3, v3.Where(t => t.A2 == "tuesday"), v3.Where(t => t.A2 != "tuesday")); TODO:
    }

    void CheckTwoSubsets<T>(IRelatable<T> set, IRelatable<T> subset1, IRelatable<T> subset2, int recurse = 0) where T : class {
      CheckSubset(subset1, set, true);
      CheckSubset(subset2, set, true);
      CheckSubset(subset1, subset2, false);
      var subset3 = set.Minus(subset1).Minus(subset2);

      Assert.AreEqual(set.Count(), subset1.Count() + subset2.Count() + subset3.Count());
      CheckEqual(set, subset1.Union(subset2).Union(subset3), true);
      CheckEqual(set, subset1.Union(subset2).Union(subset3).Union(subset1).Union(subset2).Union(subset3), true);

      CheckEqual(subset1, set.Minus(subset2).Minus(subset3), true);
      CheckEqual(subset1, set.Minus(subset2.Union(subset3)), true);
      CheckEqual(subset1, (subset2.Union(subset1)).Intersect(subset3.Union(subset1)), true);
      CheckEqual(subset1.Union(subset2), subset1.Difference(subset2), true);
      CheckEqual(subset1.Union(subset3), subset1.Union(subset2).Difference(subset3.Union(subset2)), true);

      if (recurse < 1) CheckTwoSubsets(set, subset2, subset3, 1);
      if (recurse < 1) CheckTwoSubsets(set, subset1.Where(t => true), subset2.ToStore(), 1);
    }

    [TestCase]
    public void JoinAntijoin() {
      var v0_0 = TD_None.NDR_0();
      var v0_1 = TD_None.NDR_1();
      var v1_3 = TD_sA2_iA1_dA3.NDR_16wxyz();
      var v5_1 = TD_sA6_iA1_dA5.NDR_2vw();

      CheckCountEqual(8, v1_3.Join(v5_1, (a, b) => new { A2 = a.A2, A3 = a.A3, A5 = b.A5 }));
      Assert.AreEqual(v1_3.Join(v5_1, (a, b) => new { A2 = a.A2, A3 = a.A3, A5 = b.A5 }),
                      v5_1.Join(v1_3, (b, a) => new { A2 = a.A2, A3 = a.A3, A5 = b.A5 }));
      CheckCountEqual(4, v1_3.Join(v5_1, (a, b) => new { A2 = a.A2 }));
      CheckCountEqual(1, v1_3.Join(v5_1, (a, b) => new { A2 = b.A6 }));

      CheckCountEqual(8, v1_3.Semijoin(v5_1, a => new { A1 = a.A1, A2 = a.A2, A3 = a.A3 }));
      CheckCountEqual(1, v5_1.Semijoin(v1_3, a => new { A1 = a.A1, A5 = a.A5, A6 = a.A6 }));
      CheckCountEqual(8, v1_3.Antijoin(v5_1, a => new { A1 = a.A1, A2 = a.A2, A3 = a.A3 }));
      CheckCountEqual(1, v5_1.Antijoin(v1_3, a => new { A1 = a.A1, A5 = a.A5, A6 = a.A6 }));

      CheckCountEqual(4, v1_3.Antijoin(v5_1, a => new { A2 = a.A2 }));
      CheckCountEqual(1, v5_1.Antijoin(v1_3, a => new { A2 = a.A6 }));
    }

    [TestCase]
    public void JoinAntijoin2() {
      var v1 = TD_sA2_iA1_dA3.NDR_16wxyz();
      var v2 = v1.Select(t => new { t.A3, AA1 = t.A3 * 10 });

      var vj1 = v1.Join(v2, (t, u) => new { t.A1, t.A2, A3 = u.AA1 / 10 });
      var v1x = v1.Where(t => t.A3 < 0.5m);  // 5 of these
      var v1y = v1.Where(t => t.A3 >= 0.5m);  // 11 of these
      var v2x = v2.Where(t => t.A3 < 0.5m);  // 5 of these

      var vj2 = v1.Join(v2x, (t, u) => new { t.A1, t.A2, A3 = u.AA1 / 10 });
      Assert.AreEqual(true, vj2.Equals(v1x));

      var vj3 = v1x.Join(v2, (t, u) => new { t.A1, t.A2, A3 = u.AA1 / 10 });
      Assert.AreEqual(true, vj3.Equals(v1x));

      var vsj1 = v1.Semijoin(v2, t => new { t.A1, t.A2, t.A3 });
      Assert.AreEqual(true, vsj1.Equals(v1));
      var vsj1a = v1.Semijoin(v2);
      Assert.AreEqual(true, vsj1a.Equals(v1));

      var vsj2 = v1.Semijoin(v2x, t => new { t.A1, t.A2, t.A3 });
      Assert.AreEqual(true, vsj2.Equals(v1x));
      var vsj2a = v1.Semijoin(v2x);
      Assert.AreEqual(true, vsj2a.Equals(v1x));

      var vaj1 = v1.Antijoin(v2, t => new { t.A1, t.A2, t.A3 });
      Assert.AreEqual(true, vaj1.Equals(v1.Where(t => false)));
      var vaj1a = v1.Antijoin(v2);
      Assert.AreEqual(true, vaj1a.Equals(v1.Where(t => false)));

      var vaj2 = v1.Antijoin(v2x, t => new { t.A1, t.A2, t.A3 });
      Assert.AreEqual(true, vaj2.Equals(v1y));
      var vaj2a = v1.Antijoin(v2x);
      Assert.AreEqual(true, vaj2a.Equals(v1y));
    }

    [TestCase]
    public void GroupByAggregation() {
      var vag1 = TD_sA2_iA1_dA3.NDR_16wxyz().Group(t => new { A2 = t.A2 }, 0, null, (t, v) => new { A2 = t.A2 });
      CheckCountEqual(4, vag1);
      var vag2 = TD_sA2_iA1_dA3.NDR_16wxyz().Group(t => new { A2 = t.A2 }, 0, (t,v) => v + t.A1, (t, v) => new { A2 = t.A2, A7 = v });
      CheckCountEqual(4, vag2);
      var vag3 = TD_sA2_iA1_dA3.NDR_16wxyz().Group(t => new { }, 0, (t, v) => v + t.A1, (t, v) => new { A7 = v });
      CheckCountEqual(1, vag3);
      Assert.AreEqual(8 * 88 + 8 * 42, vag3.ToArray()[0].A7);
      Assert.AreEqual(8 * 88 + 8 * 42, vag3.Single().A7);
      //vag3.fi
    }

    [TestCase]
    public void Update() {
      var v1 = TD_None.NDR_0();
      var v2 = TD_None.NDR_1();
      var v3 = TD_sA2_iA1_dA3.NDR_2xy();
      var v4 = TD_sA2_iA1_dA3.NDR_16wxyz();
      var v5 = TD_sA6_iA1_dA5.NDR_2vw();

      var ud1 = new RelationStore<TD_sA2_iA1_dA3>();
      CheckCountEqual(0, ud1);
      var ud2 = new RelationStore<TD_sA2_iA1_dA3>(v4);
      CheckCountEqual(16, ud2);

      var ud3 = new RelationStore<TD_sA2_iA1_dA3>();
      ud3.Insert(v4).Store();
      CheckCountEqual(16, ud3);

      var ud4 = new RelationStore<TD_sA2_iA1_dA3>(v4);
      ud4.Delete(v4).Store();
      CheckCountEqual(0, ud4);

      var ud5 = new RelationStore<TD_sA2_iA1_dA3>(v4);
      ud5.Delete(t => true).Store();
      CheckCountEqual(0, ud5);

      var ud6 = new RelationStore<TD_sA2_iA1_dA3>(v4);
      ud6.Delete(t => t.A2 == "tuesday").Store();
      CheckCountEqual(12, ud6);

      var ud7 = new RelationStore<TD_sA2_iA1_dA3>(v4);
      ud7.Update(t => t.A2 == "tuesday", u => new TD_sA2_iA1_dA3 { A1 = u.A1, A2 = "Wednesday", A3 = u.A3 }).Store();
      CheckCountEqual(16, ud7);
      CheckCountEqual(4, ud7.Where(t => t.A2 == "Wednesday"));
    }

    void CheckCountEqual<T>(int count, IRelatable<T> rel) where T : class {
      Assert.AreEqual(count, rel.Count());
    }
    void CheckCount(int count, int actual) {
      Assert.AreEqual(count, actual);
    }
  }
}
