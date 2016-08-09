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
  public class BasicTests {
    // internal classes for testing
    class TT1 : TupleValue<TT1> {
      public string A2;
      public int A1;
      public decimal A3;
    }

    class TT2 : TupleValue<TT2> {
      public decimal A3;
      public string A2;
      public int A1;
    }

    class TT3 : TupleValue<TT3> {
      public int A1;
      public string A2;
      public bool A3;
    }

    class TT4 : TupleValue<TT4> {
      public int A1;
      public string A2;
      public decimal A3;
      public bool A4;
    }

    [TestCase]
    public void TupleTypeEqual() {
      var t1 = new TT1 { A1 = 42, A2 = "hello", A3 = 1.2m };
      var t2 = new TT2 { A1 = 42, A2 = "hello", A3 = 1.2m };
      var t3 = new TT3 { A1 = 42, A2 = "hello", A3 = true };
      var t4 = new TT4 { A1 = 42, A2 = "hello", A3 = 1.2m, A4 = true };

      Assert.AreEqual(t1.TupleType, t2.TupleType);
      Assert.AreNotSame(t1.TupleType, t2.TupleType);
      Assert.AreEqual(true, t1.TupleType.SameHeading(t1.TupleType));
      Assert.AreEqual(true, t1.TupleType.SameHeading(t2.TupleType));
      Assert.AreEqual(t1, t2);
      Assert.AreNotEqual(t1.TupleType, t3.TupleType);
      Assert.AreNotEqual(t1.TupleType, t4.TupleType);
    }

    [TestCase]
    public void TupleValueEqual() {
      var t1 = new TT1 { A1 = 42, A2 = "hello", A3 = 1.2m };
      var t2 = new TT2 { A1 = 42, A2 = "hello", A3 = 1.2m };
      var t3 = new TT3 { A1 = 42, A2 = "hello", A3 = true };
      var t4 = new TT4 { A1 = 42, A2 = "hello", A3 = 1.2m, A4 = true };

      Assert.AreEqual(t1, t2);
      Assert.AreNotEqual(t1, t3);
      Assert.AreNotEqual(t1, t4);
    }

    [TestCase]
    public void AnonymousEqual() {
      var veq1 = new { };
      var veq2 = new { };
      Assert.IsTrue(TupleType.AreEqual(veq1, veq2));
      var veq3 = new { A1 = 42, A2 = "hello world" };
      var veq4 = new { A1 = 42, A2 = "hello world" };
      Assert.IsTrue(TupleType.AreEqual(veq3, veq4));
      var veq5 = new { A1 = 43, A2 = "hello world" };
      Assert.IsFalse(TupleType.AreEqual(veq3, veq5));
      var veq6 = new { A1 = 42.0, A2 = "hello world" };
      Assert.IsFalse(TupleType.AreEqual(veq3, veq5));
    }

    //[TestCase]
    //public void TupleImport() {
    //  var t1 = new TT1 { A1 = 42, A2 = "hello", A3 = 1.2m };
    //  var t2 = new TT2();

    //  t2.TupleType.ImportData(t1); // not accessible
    //}

    [TestCase]
    public void RelationCount() {
      var ta1 = new TT1[] {
        new TT1 { A1 = 42, A2 = "hello", A3 = 1.2m },
        new TT1 { A1 = 42, A2 = "world", A3 = 1.2m },
      };
      var ta2a = new TT2[] {
        new TT2 { A1 = 42, A2 = "hello", A3 = 1.2m },
        new TT2 { A1 = 42, A2 = "world", A3 = 1.2m },
      };
      var ta2b = new TT2[] {
        new TT2 { A1 = 42, A2 = "hello", A3 = 1.2m },
        new TT2 { A1 = 43, A2 = "world", A3 = 1.2m },
      };

      var r1 = Relatable.FromSource(ta1);
      var r2a = Relatable.FromSource(ta2a);
      var r2b = Relatable.FromSource(ta2b);

      Assert.AreEqual(2, ta1.Count());
      Assert.AreEqual(2, ta2a.Count());
      Assert.AreEqual(2, ta2b.Count());
      Assert.AreEqual(ta1, ta1);
      Assert.AreEqual(ta1, ta2a);
      Assert.AreNotEqual(ta1, ta2b);
      Assert.AreNotEqual(ta2a, ta2b);
    }

  }
}
