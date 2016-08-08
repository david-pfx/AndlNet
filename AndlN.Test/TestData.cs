using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndlN;

namespace AndlN.RelatableTest {
  // internal classes for testing
  public class TD_None {
    public static IRelatable<TD_None> NDR_0() { // DUM
      return Relatable.FromSource(new TD_None[] {
      });
    }
    public static IRelatable<TD_None> NDR_1() { // DEE
      return Relatable.FromSource(new TD_None[] {
        new TD_None {  },
      });
    }
  }
  public class TD_sA2_iA1_dA3 {
    public string A2;
    public int A1;
    public decimal A3;
    public static IRelatable<TD_sA2_iA1_dA3> NDR_0() {
      return Relatable.FromSource(new TD_sA2_iA1_dA3[] {
      });
    }
    public static IRelatable<TD_sA2_iA1_dA3> NDR_2xy() {
      return Relatable.FromSource(new TD_sA2_iA1_dA3[] {
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "hello", A3 = 0.0m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "world", A3 = 0.1m },
      });
    }
    public static IRelatable<TD_sA2_iA1_dA3> NDR_2xz() {
      return Relatable.FromSource(new TD_sA2_iA1_dA3[] {
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "hello", A3 = 0.0m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "world!!!", A3 = 0.1m },
      });
    }
    public static IRelatable<TD_sA2_iA1_dA3> NDR_16wxyz() {
      return Relatable.FromSource(new TD_sA2_iA1_dA3[] {
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "hello",   A3 = 0.0m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "world",   A3 = 0.1m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "goodbye", A3 = 0.2m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "tuesday", A3 = 0.3m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "hello",   A3 = 0.4m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "world",   A3 = 0.5m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "goodbye", A3 = 0.6m },
        new TD_sA2_iA1_dA3 { A1 = 42, A2 = "tuesday", A3 = 0.7m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "hello",   A3 = 0.8m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "world",   A3 = 0.9m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "goodbye", A3 = 1.0m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "tuesday", A3 = 1.1m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "hello",   A3 = 1.2m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "world",   A3 = 1.3m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "goodbye", A3 = 1.4m },
        new TD_sA2_iA1_dA3 { A1 = 88, A2 = "tuesday", A3 = 1.5m },
      });
    }
    public static TD_sA2_iA1_dA3 NDR_16_in() {
      return new TD_sA2_iA1_dA3 { A1 = 42, A2 = "hello", A3 = 0.0m };
    }
    public static TD_sA2_iA1_dA3 NDR_16_out() {
      return new TD_sA2_iA1_dA3 { A1 = 88, A2 = "hello", A3 = 0.0m };
    }
  }

  // same values different type same heading
  public class TD_dA3_sA2_iA1 {
    public decimal A3;
    public string A2;
    public int A1;
    public static IRelatable<TD_dA3_sA2_iA1> NDR_2xy() {
      return Relatable.FromSource(new TD_dA3_sA2_iA1[] {
        new TD_dA3_sA2_iA1 { A1 = 42, A2 = "hello", A3 = 0.0m },
        new TD_dA3_sA2_iA1 { A1 = 42, A2 = "world", A3 = 0.1m },
      });
    }
  }

  // same values different type different heading
  public class TD_iA1_sA2_b_A3 {
    public int A1;
    public string A2;
    public bool A3;
    public static IRelatable<TD_iA1_sA2_b_A3> NDR_2xy() {
      return Relatable.FromSource(new TD_iA1_sA2_b_A3[] {
        new TD_iA1_sA2_b_A3 { A1 = 42, A2 = "hello", A3 = true },
        new TD_iA1_sA2_b_A3 { A1 = 42, A2 = "world", A3 = false },
      });
    }
  }

  public class TD_iA1_sA2_iA3_dA4 {
    public int A1;
    public string A2;
    public int A3;
    public decimal A4;
    public static IRelatable<TD_iA1_sA2_iA3_dA4> NDR_2xy() {
      return Relatable.FromSource(new TD_iA1_sA2_iA3_dA4[] {
        new TD_iA1_sA2_iA3_dA4 { A1 = 42, A2 = "hello", A3 = 1, A4 = 0.0m },
        new TD_iA1_sA2_iA3_dA4 { A1 = 42, A2 = "world", A3 = 2, A4 = 0.0m }
      });
    }
  }

  // one join on A1
  public class TD_sA6_iA1_dA5 {
    public string A6;
    public int A1;
    public decimal A5;
    public static IRelatable<TD_sA6_iA1_dA5> NDR_2vw() {
      return Relatable.FromSource(new TD_sA6_iA1_dA5[] {
        new TD_sA6_iA1_dA5 { A6 = "first", A1 = 42, A5 = 99.2m },
        new TD_sA6_iA1_dA5 { A6 = "second", A1 = 99, A5 = 99.2m }
      });
    }
  }
}
