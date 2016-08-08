using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AndlN;

namespace AndlN.Samples {
  public class NorthwindSample {
    public static void Run() {
      var suppliers = Northwind.SuppliersRelation();
      var products = Northwind.ProductsRelation();
      var categories = Northwind.CategoriesRelation();

      Program.Show("Suppliers", suppliers.Count());
      Program.Show("Categories", categories.Count());
      Program.Show("Products", products.Count());
      Program.Show("Suppliers",
        suppliers.AsEnumerable()
          .Select(s => $"{s.SupplierID}: {s.ContactName}, {s.CompanyName}, {s.City}")
        );
      Program.Show("Categories",
        categories.AsEnumerable()
          .Select(c => $"{c.CategoryID}: {c.CategoryName}")
        );
      Program.Show("Products",
        products.AsEnumerable()
          .Select(p => $"{p.ProductID}: {p.ProductName}, {p.CategoryID}, {p.SupplierID}, {p.UnitsInStock}, {p.UnitPrice}")
        );
      // Group and sum value by company and product category
      Program.Show("Products",
        products
          .Join(suppliers, (p, s) => new { p.ProductID, p.ProductName, p.CategoryID, Value = p.UnitsInStock * p.UnitPrice, s.CompanyName })
          .Join(categories, (p, c) => new { p.ProductID, p.ProductName, p.CompanyName, p.Value, c.CategoryName })
          .Group(p => new { p.CompanyName,p.CategoryName }, 0m, (p, v) => v + p.Value, (p, v) => new { p.CompanyName, p.CategoryName, v })
          .AsEnumerable()
          .Select(p => String.Format("{0,40}: {1,15} {2:c2}", p.CompanyName, p.CategoryName, p.v))
        );

    }
  }
}
