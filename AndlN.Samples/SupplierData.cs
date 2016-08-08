using AndlN;

// Supplier data set in C# format
// This is the standard data set used in books by Date and Darwen

// Each class must inherit from NdlTuple<> and must declare a default constructor
namespace SupplierData {
  public class Stype {
    public readonly string Sno;
    public readonly string Sname;
    public readonly int Status;
    public readonly string City;
    public Stype() { }
    public Stype(string sno, string sname, int status, string city) {
      Sno = sno;
      Sname = sname;
      Status = status;
      City = city;
    }
    public static Stype[] Data() {
      return new Stype[] {
        new Stype( "S1", "Smith", 20, "London" ),
        new Stype( "S2", "Jones", 10, "Paris" ),
        new Stype( "S3", "Blake", 30, "Paris" ),
        new Stype( "S4", "Clark", 20, "London" ),
        new Stype( "S5", "Adams", 30, "Athens" ),
      };
    }
  }
  public class Ptype {
    public readonly string Pno;
    public readonly string Pname;
    public readonly string Color;
    public readonly decimal Weight;
    public readonly string City;
    public Ptype() { }
    public Ptype(string pno, string pname, string color, decimal weight, string city) {
      Pno = pno;
      Pname = pname;
      Color = color;
      Weight = weight;
      City = city;
    }

    public static Ptype[] Data() {
      return new Ptype[] {
        new Ptype( "P1","Nut",   "Red",  12.0m,"London" ),
        new Ptype( "P2","Bolt",  "Green",17.0m,"Paris"  ),
        new Ptype( "P3","Screw", "Blue", 17.0m,"Oslo"   ),
        new Ptype( "P4","Screw", "Red",  14.0m,"London" ),
        new Ptype( "P5","Cam",   "Blue", 12.0m,"Paris"  ),
        new Ptype( "P6","Cog",   "Red",  19.0m,"London" ),
      };
    }
  }

  public class SPtype {
    public readonly string Sno;
    public readonly string Pno;
    public readonly int Qty;

    public SPtype() { }
    public SPtype(string sno, string pno, int qty) {
      Sno = sno;
      Pno = pno;
      Qty = qty;
    }

    public static SPtype[] Data() {
      return new SPtype[] {
        new SPtype( "S1","P1",300 ),
        new SPtype( "S1","P2",200 ),
        new SPtype( "S1","P3",400 ),
        new SPtype( "S1","P4",200 ),
        new SPtype( "S1","P5",100 ),
        new SPtype( "S1","P6",100 ),
        new SPtype( "S2","P1",300 ),
        new SPtype( "S2","P2",400 ),
        new SPtype( "S3","P2",200 ),
        new SPtype( "S4","P2",200 ),
        new SPtype( "S4","P4",300 ),
        new SPtype( "S4","P5",400 ),
      };
    }

  }
  public class Jtype {
    public readonly string Jno;
    public readonly string Jname;
    public readonly string City;
    public Jtype() { }
    public Jtype(string jno, string jname, string city) {
      Jno = jno;
      Jname = jname;
      City = city;
    }
    public static Jtype[] Data() {
      return new Jtype[] {
        new Jtype("J1","Sorter","Paris"),
        new Jtype("J2","Display","Rome"),
        new Jtype("J3","OCR","Athens"),
        new Jtype("J4","Console","Athens"),
        new Jtype("J5","RAID","London"),
        new Jtype("J6","EDS","Oslo"),
        new Jtype("J7","Tape","London"),
      };
  }
  }

  public class SPJtype {
    public readonly string Sno;
    public readonly string Pno;
    public readonly string Jno;
    public readonly int Qty;

    public SPJtype() { }
    public SPJtype(string sno, string pno, string jno, int qty) {
      Sno = sno;
      Pno = pno;
      Jno = jno;
      Qty = qty;
    }

    public static SPJtype[] Data() {
      return new SPJtype[] {
        new SPJtype("S1","P1","J1",200),
        new SPJtype("S1","P1","J4",700),
        new SPJtype("S2","P3","J1",400),
        new SPJtype("S2","P3","J2",200),
        new SPJtype("S2","P3","J3",200),
        new SPJtype("S2","P3","J4",500),
        new SPJtype("S2","P3","J5",600),
        new SPJtype("S2","P3","J6",400),
        new SPJtype("S2","P3","J7",800),
        new SPJtype("S2","P5","J2",100),
        new SPJtype("S3","P3","J1",200),
        new SPJtype("S3","P4","J2",500),
        new SPJtype("S4","P6","J3",300),
        new SPJtype("S4","P6","J7",300),
        new SPJtype("S5","P2","J2",200),
        new SPJtype("S5","P2","J4",100),
        new SPJtype("S5","P5","J5",500),
        new SPJtype("S5","P5","J7",100),
        new SPJtype("S5","P6","J2",200),
        new SPJtype("S5","P1","J4",100),
        new SPJtype("S5","P3","J4",200),
        new SPJtype("S5","P4","J4",800),
        new SPJtype("S5","P5","J4",400),
        new SPJtype("S5","P6","J4",500),
      };
  }
  }
}

