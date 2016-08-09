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

using AndlN;
using Andl.Common;
using AndlN.RelatableTest;
using SupplierData;

namespace AndlN.Samples {
  public static class BasicSample {
    public static void Run() {
      SupplierSample.Run();
      CsvTextSample.Run();
    }
  }

  public static class CsvTextSample {
    public static void Run() {
      var scsv1 = Relatable.FromCsv<Stype>(".", "S", "Sno:text,Sname:text,Status:integer,City:text");
      Program.Show("Csv", scsv1);
      var stxt1 = Relatable.FromText(".", "testfile");
      Program.Show("Txt", stxt1);
    }
  }

  public static class SupplierSample {
    public static void Run() {
      var srel = Relatable.FromSource(Stype.Data());
      var prel = Relatable.FromSource(Ptype.Data());
      var sprel = Relatable.FromSource(SPtype.Data());

      Program.Show("S", srel);
      var sw1 = srel.Where(t => t.City == "London");
      Program.Show("Where 1", sw1);
      var sw2 = srel.Where(t => t.City == "Paris");
      Program.Show("Where 2", sw2);
      var su1 = sw1.Union(sw2).Union(sw2).Union(sw1);
      Program.Show("Union", su1);
      var ss1 = srel.Select(t => new { City = t.City, Bigstatus = t.Status * 10 });
      Program.Show("Select", ss1);
      var ss2 = srel.Select(t => new { City = t.City, Bigstatus = t.Status * 10 });
      Program.Show("Select", ss2);
      var sj1 = srel.Join(sprel, (s, sp) => new { Sname = s.Sname, City = s.City, Qty = sp.Qty });
      Program.Show("Join", sj1);
      var sob1 = srel.OrderBy((t, u) => t.Sname.CompareTo(u.Sname) < 0);
      Program.Show("Order", sob1);
      var sgr1 = srel.Group(t => new { City = t.City }, 0, (t, v) => v + t.Status, (g, a) => new { City = g.City, Bigstatus = a });
      Program.Show("Group", sgr1);
    }
  }
}
