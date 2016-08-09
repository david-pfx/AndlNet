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
using static System.Math;

using AndlN;
using Andl.Common;
using AndlN.RelatableTest;

namespace AndlN.Samples {
  public class WhileSample {
    public static void Run() {
      Fibonacci();
      Mandelbrot();
      OrgChart();
      FamilyTree();
    }

    // Generate Fibonacci numbers, print out 91-99
    public static void Fibonacci() {
      Program.Show("while fib", Relatable
        .FromSource(new[] { new { N = 1, fib = 1m, fibx = 0m } })
        .While(s => Relatable
          .FromSource(new[] { new { N = s.N + 1, fib = s.fib + s.fibx, fibx = s.fib } })
          .Where(t => t.N < 100))
        .Where(t => t.N > 90)
        .AsEnumerable()
        .Select(s => $"{s.N} {s.fib}")
      );
    }

    // Generate print out of Mandelbrot set
    public static void Mandelbrot() {
      var xaxis = Relatable.Sequence(64, n => new { cx = -2.0m + 0.05m * n });
      var yaxis = Relatable.Sequence(21, n => new { cy = -1.0m + 0.1m * n });
      var seed = Relatable
        .FromSource(new[] { new { iter = 0, x = 0.0m, y = 0.0m } })
        .Join(xaxis, (s, t) => new { s.iter, s.x, s.y, t.cx })
        .Join(yaxis, (s, t) => new { s.iter, s.x, s.y, s.cx, t.cy });
      //Program.Show("while Mandelbrot", xaxis.Count(), yaxis.Count(), seed.Count());
      var mand = seed
        .While(t => Relatable
          .FromSource(new[] { new { iter = t.iter + 1, x = t.x * t.x - t.y * t.y + t.cx, y = 2 * t.x * t.y + t.cy, t.cx, t.cy } })
          .Where(u => u.x * u.x + u.y * u.y < 4.0m && u.iter < 28));
      //Program.Show("while Mandelbrot", mand.Count());
      var mand2 = mand.Group(m => new { m.cx, m.cy }, 0, (m, v) => Max(v, m.iter), (g, a) => new { g.cx, g.cy, maxiter = a });
      //Program.Show("while Mandelbrot", mand2.Count());
      Program.Show("while Mandelbrot", mand2
        .Select(m => new { m.cy, m.cx, itch = " .+*#"[m.maxiter / 6] })
        .Group(m => new { m.cy }, "", (m, v) => v + m.itch, (g, a) => new { g.cy, line = a })
        .OrderBy((a, b) => a.cy < b.cy)
        .AsEnumerable()
        .Select(m => $"{m.cy,4} {m.line}")
      );
    }

    // Flatten org chart to show levels
    internal static void OrgChart() {
      var data = new[] {
        new { name = "Alice", boss="" },
        new { name = "Bob"  , boss="Alice" },
        new { name = "Cindy", boss="Alice" },
        new { name = "Dave" , boss="Bob" },
        new { name = "Emma" , boss="Bob" },
        new { name = "Fred" , boss="Cindy" },
        new { name = "Gail" , boss="Cindy" },
      };
      var orgchart = Relatable.FromSource(data);
      var seed = Relatable.FromSource(new[] { new { name = "Alice", level = 0 } });
      Program.Show("orgchart", seed
        .While(t => Relatable.FromSource(new[] { new { boss = t.name, level = t.level + 1 } })
          .Join(orgchart, (u, o) => new { o.name, u.level }))
        );
    }

    // Flatten family tree
    internal static void FamilyTree() {
      var data = new[] {
        new { id= 1, firstname="Karl",   lastname="Miller", yob=1855, yod=1905, father_id=0, mother_id=0 },
        new { id= 2, firstname="Lisa",   lastname="Miller", yob=1851, yod=1912, father_id=0, mother_id=0 },
        new { id= 3, firstname="Ruth",   lastname="Miller", yob=1878, yod=1888, father_id=1, mother_id=2 },
        new { id= 4, firstname="Helen",  lastname="Miller", yob=1880, yod=1884, father_id=1, mother_id=2 },
        new { id= 5, firstname="Carl",   lastname="Miller", yob=1882, yod=1935, father_id=1, mother_id=2 },
        new { id= 6, firstname="John",   lastname="Miller", yob=1883, yod=1900, father_id=1, mother_id=2 },
        new { id= 7, firstname="Emily",  lastname="Newton", yob=1880, yod=1940, father_id=0, mother_id=0 },
        new { id= 8, firstname="Charly", lastname="Miller", yob=1908, yod=1978, father_id=5, mother_id=7 },
        new { id= 9, firstname="Deborah",lastname="Brown",  yob=1910, yod=1980, father_id=0, mother_id=0 },
        new { id=10, firstname="Chess",  lastname="Miller", yob=1948, yod=0,    father_id=9, mother_id=9 },
      };

      var familytree = Relatable.FromSource(data);
      var seed = Relatable.FromSource(new[] { new { father_id= "x", level = 0 } });
      Program.Show("familytree", familytree
        .Where(f=>f.id==1)
        .Select(f => new { f.id,f.firstname,f.lastname,level=0 })
        .While(f => Relatable
          .FromSource(new[] { new { father_id=f.id, level=f.level+1 } })
          .Join(familytree, (s, ff) => new { ff.id,ff.firstname,ff.lastname,s.level }))
        .AsEnumerable()
        .Select(t => $"{t.firstname,-10}{t.lastname,-10}{t.level}")
        );
    }

  }
}
