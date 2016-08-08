// Some sample queries from https://web.njit.edu/~hassadi/Dbase_Courses/CIS631/Ex_03.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndlN;
using SupplierData;

namespace AndlN.Samples {
  public class SPPsample1 {
    public static void Run() {
      var S = Relatable.FromSource(Stype.Data());
      var P = Relatable.FromSource(Ptype.Data());
      var SP = Relatable.FromSource(SPtype.Data());
      var J = Relatable.FromSource(Jtype.Data());
      var SPJ = Relatable.FromSource(SPJtype.Data());

      //----------------------------------------------------------------------
      // Q1. Get suppliers names who supply part 'P2'.
      // SQL>select sname from s, sp where s.s#=sp.s# and sp.p#='P2'; 
      // SQL>select distinct s.sname from s where s.s# IN (select sp.s# from sp where sp.p# ='P2'); 

      Show("Q1. Get suppliers names who supply part 'P2'",
        SP.Where(sp => sp.Pno == "P2")
          .Join(S, (sp, s) => new { s.Sname })
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
        );

      Show("Q1. Get suppliers names who supply part 'P2'",
        S .Where(s => SP
            .Where(sp => sp.Pno == "P2")
            .Select(sp => new { sp.Sno })
            .Contains(new { Sno = s.Sno }))
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
        );

      //----------------------------------------------------------------------
      // Q2. Get suppliers names who supply at least one red part.
      // SQL>select distinct sname from s, sp, p where p.color='Red' and s.s#=sp.s# and p.p#=sp.p# 
      // SQL>select distinct s.sname from s where s.s# IN (select sp.s# from sp where sp.p# IN (select p.p# from p where p.Color = 'Red') );

      Show("Q2. Get suppliers names who supply at least one red part",
        P .Where(p => p.Color == "Red")
          .Join(SP, sp => new { sp.Sno })
          .Join(S, s => new { s.Sname })
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
      );
      Show("Q2. Get suppliers names who supply at least one red part",
        S.Where(s => SP
           .Where(sp => P
             .Where(p => p.Color == "Red")
             .Select(p => new { p.Pno })
             .Contains(new { sp.Pno }))
            .Select(sp => new {sp.Sno})
            .Contains(new { s.Sno}))
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
      );

      // SQL> select distinct s.sname from s where s.s# IN (select sp.s# from sp where sp.p# IN (select p.p# from p where p.Color = 'Red') ); 

      //----------------------------------------------------------------------
      // Q3. Get the supplier names for suppliers who do not supply part ‘P2’.
      // SQL> select distinct s.sname from s where s.s# NOT IN (select sp.s# from sp where sp.p#='P2'); 
      // SQL> select distinct s.sname from s where NOT EXISTS (select * from sp where sp.s# = s.s# and sp.p# = 'P2'); 

      Show("Q3. Get the supplier names for suppliers who do not supply part ‘P2’",
        S .Antijoin(SP
            .Where(sp => sp.Pno == "P2")
            , sp => new { sp.Sname })
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
      );
      Show("Q3. Get the supplier names for suppliers who do not supply part ‘P2’",
        S .Where(s => (SP
            .Where(sp => sp.Sno == s.Sno && sp.Pno == "P2"))
              .IsEmpty())
          .AsEnumerable()
          .Select(s=>$"{s.Sname}")
      );
      //----------------------------------------------------------------------
      // Q4. Get the supplier names for suppliers who supply all parts.
      // SQL> select distinct s.sname from s where NOT EXISTS 
      //        (select * from p where NOT EXISTS 
      //          (select * from sp where sp.s# = s.s# and sp.p# = p.p#) );

      Show("Q4. Get the supplier names for suppliers who supply all parts",
        S .Where(s => P.Select(p => new { p.Pno })
          .Equals(S.Where(sx => sx.Sno == s.Sno)
            .Join(SP, sp => new { sp.Pno })))
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
        );
      Show("Q4. Get the supplier names for suppliers who supply all parts",
        S .Where(s => P
           .Where(p => SP
             .Where(sp => sp.Sno == s.Sno && sp.Pno == p.Pno)
             .IsEmpty())
           .IsEmpty())
          .AsEnumerable()
          .Select(s => $"{s.Sname}")
        );

      //----------------------------------------------------------------------
      //Q5. Get supplier numbers who supply at lease one of the parts supplied by supplier ‘S2’.
      //SQL> select  distinct s.s# from s, sp where s.s# = sp.s# and p# IN (select p# from sp where sp.s# = 'S2')
      Show("Q5. Get supplier numbers who supply at lease one of the parts supplied by supplier ‘S2’",
        S .Join(SP, (s, sp) => new { s.Sno, sp.Pno })
          .Semijoin(SP
            .Where(sp => sp.Sno == "S2")
            .Select(sp => new { sp.Pno }))
          .Select(s => new { s.Sno })
          .AsEnumerable()
          .Select(s => $"{s.Sno}")
        );

      //----------------------------------------------------------------------
      // Q6. Get all pairs of supplier numbers such that two suppliers are “colocated” (located in the same city).
      // SQL> select A.s# AS SA, B.S# AS SB from S  A, S  B where A.city = B. city and A.s# < B.S# 
      Show("Q6. Get all pairs of supplier numbers such that two suppliers are “colocated” (located in the same city)",
        S.Select(s => new { SA = s.Sno, s.City })
        .Join(S
          .Select(s => new { SB = s.Sno, s.City })
          , (sa,sb)=> new { sa.SA, sb.SB })
        .Where(s => s.SA.CompareTo(s.SB) < 0)
          .AsEnumerable()
          .Select(s => $"{s.SA} {s.SB} ")
      );

      //----------------------------------------------------------------------
      // Q7. Join the three tables and find the result of natural join with selected attributes.
      // SQL> select distinct  s.s#, sname, p.p#, p.pname, s.city, status, QTY from s, sp, p where s.s#=sp.s# and p.p#=sp.p# and s.city=p.city 
      Show("Q7. Join the three tables and find the result of natural join with selected attributes",
        S.Join(SP, (s, sp) => new { s.Sno, s.Sname, s.City, s.Status, sp.Pno, sp.Qty })
        .Join(P, (ssp, p) => new { ssp.Sno, ssp.Sname, ssp.City, ssp.Status, ssp.Pno, ssp.Qty, p.Pname })
        .AsEnumerable()
        .Select(t => $"{t.Sno} {t.Sname} {t.Pno} {t.Pname} {t.City} {t.Status} {t.Qty}")
      );

      //----------------------------------------------------------------------
      // Q8. Get all shipments where the quantity is in the range 300 to 750 inclusive.
      // SQL> select spj.* from spj where spj.QTY>=300 and spj.QTY<=750; 
      Show("Q8. Get all shipments where the quantity is in the range 300 to 750 inclusive",
        SPJ.Where(spj => spj.Qty >= 300 && spj.Qty <= 750)
          .AsEnumerable()
          .Select(t => $"{t.Sno} {t.Pno} {t.Jno} {t.Qty}")
      );

      //----------------------------------------------------------------------
      // Q9. Get all supplier-number/part-number/project-number triples such that the indicated supplier, part, and project are all colocated (i.e., all in the same city).
      // SQL> select s.s#, p.p#, J.j# from s, p, j where s.city = p.city and p.city = j.city; 
      // NOTE: mismatch -- 16 rows instead of 12, but looks OK
      Show("Q9. Get all supplier-number/part-number/project-number triples such that the indicated supplier, part, and project are all colocated (i.e., all in the same city)",
        S.Join(P, (s,p) => new { s.Sno, s.City, p.Pno })
          .Join(J, (sp, j) => new { sp.Sno, sp.Pno, j.Jno })
          .AsEnumerable()
          .Select(t => $"{t.Sno} {t.Pno} {t.Jno}")
      );

      //----------------------------------------------------------------------
      // Q10. Get all pairs of city names such that a supplier in the first city supplies a project in the second city.
      // SQL> select distinct s.city as scity, j.city as jcity from s, j where exists (select * from spj where spj.s# = s.s# and spj.j# = j.j#); 
      Show("Q10. Get all pairs of city names such that a supplier in the first city supplies a project in the second city",
        S .Select(s => new { s.Sno, Scity = s.City })
          .Join(J
            .Select(j => new { j.Jno, Jcity = j.City })
            , (s, j) => new { s.Sno, s.Scity, j.Jno, j.Jcity })
            .Where(sj => SPJ
              .Where(spj => spj.Sno == sj.Sno && spj.Jno == sj.Jno)
              .Exists())
          .Select(sj => new { sj.Scity, sj.Jcity })
          .AsEnumerable()
          .Select(t => $"{t.Scity} {t.Jcity}")
      );

      //----------------------------------------------------------------------
      // Q11. Get all cities in which at least one supplier, part, or project is located.
      // SQL> select s.city from s union select p.city from p union select j.city from j; 
      Show("Q11. Get all cities in which at least one supplier, part, or project is located",
        S.Select(s => new { s.City })
          .Union(P
            .Select(p => new { p.City }))
          .Union(J
            .Select(j => new {j.City}))
          .AsEnumerable()
          .Select(t => $"{t.City}")
      );

      //----------------------------------------------------------------------
      // Q12. Get supplier-number/part-number pairs such that the indicated supplier does not supply the indicated part.
      // SQL> select s.s#, p.p# from s, p minus select spj.s#, spj.p# from spj; 
      Show("Q12. Get supplier-number/part-number pairs such that the indicated supplier does not supply the indicated part",
        S .Select(s => new { s.Sno })
          .Join(P, (s, p) => new { s.Sno, p.Pno })
          .Minus(SPJ
            .Select(spj => new { spj.Sno, spj.Pno }))
          .AsEnumerable()
          .Select(t => $"{t.Sno} {t.Pno}")
      );

      //----------------------------------------------------------------------
      // Q13. Get all pairs of part numbers and supplier numbers such that some supplier supplies both indicated parts.
      // SQL> select distinct spjx.s#, spjx.p# as PA, spjy.p# as PB from spj  spjx, spj  spjy where spjx.s# = spjy.s# and spjx.p# < spjy.p#; 
      Show("Q13. Get all pairs of part numbers and supplier numbers such that some supplier supplies both indicated parts",
        SPJ.Select(spj => new { spj.Sno, PA=spj.Pno })
          .Join(SPJ, (spj1, spj2) => new { spj1.Sno, spj1.PA, spj2.Pno })
          .Where(spj => spj.PA.CompareTo(spj.Pno) < 0)
          .AsEnumerable()
          .Select(t => $"{t.Sno} {t.PA} {t.Pno}")
      );
    }

    public static void Show<T>(string msg, IRelatable<T> srel) where T : class {
      Console.WriteLine(msg);
      int count = 0;
      foreach (var s in srel) {
        Console.WriteLine($"  {TupleType.Format(s)}");
        count++;
      }
      Console.WriteLine($"(Rows: {count})");
    }
    public static void Show<T>(string msg, IEnumerable<T> srel) {
      Console.WriteLine(msg);
      int count = 1;
      foreach (var s in srel) {
        Console.WriteLine($"  {count}: {s}");
        count++;
      }
    }
    public static void Show(string msg) {
      Console.WriteLine(msg);
    }
  }

}

