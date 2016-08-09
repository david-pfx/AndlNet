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
/// the file Licence.txt or at http://andl.org/Licence.txt.
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AndlN;
using Andl.Common;
using System.IO;

// For debugging use:

// sql Northwind "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True"


namespace AndlN.Generator {
  /// <summary>
  /// Mainline for Ndl code generator
  /// </summary>
  class GenMain {
    const string Version = "AndlN.Generator 1.0";
    const string Help = "AndlN.Generator <source> <basename> <connection string> [/options]\n"
      + "\t\tSource must be sql, odbc or oledb.\n"
      + "\t\tBasename is used for output file and class name in generate code.\n"
      + "\t/n\tn=1 to 4, set tracing level";
    static readonly Dictionary<string, Action<string>> _options = new Dictionary<string, Action<string>> {
    };

    static TextWriter Out { get { return _output; } }
    static TextWriter _output;

    class dummy { }

    static void Main(string[] args) {
      try {
        Logger.Open(1);
        _output = Console.Out;
        _output.WriteLine(Version);
        var options = OptionParser.Create(_options, Help);
        if (!options.Parse(args))
          return;
        SourceKind skind;
        if (!Enum.TryParse(options.GetPath(0), true, out skind)) throw Error.Fatal("invalid source kind");
        var rootname = options.GetPath(1);
        var connection = options.GetPath(2);
        if (connection == null) throw Error.Fatal("three arguments required");
        if (rootname == null) throw Error.Fatal("data source must have a name");
        Logger.WriteLine(1, $"{skind} {rootname} {connection}");

        var tw = new StringWriter();
        List<CommonHeading> headings = new List<CommonHeading>();
        switch (skind) {
        case SourceKind.Sql:
          foreach (var table in Relatable.FromSqlSchema(connection)) {
          //foreach (NdlSchemaSql table in new NdlRelationStreamSqlSchema(connection)) {
            if (table.TABLE_TYPE == "SYSTEM TABLE") continue;
            var schema = Relatable.FromSql<dummy>(connection, table.TABLE_NAME) as RelationStream<dummy>;
            //var schema = new NdlRelationStreamSql<dummy>(connection, table.TABLE_NAME);
            headings.Add(new CommonHeading { Name = table.TABLE_NAME, Fields = schema.Fields });
            Logger.WriteLine(2, $"{table.TABLE_NAME} ({table.TABLE_TYPE}) => {headings.Last()}");
          }
          break;
        case SourceKind.Odbc:
          foreach (var table in Relatable.FromOdbcSchema(connection)) {
          //foreach (NdlSchemaOdbc table in new NdlRelationStreamOdbcSchema(connection)) {
            if (table.TABLE_TYPE == "SYSTEM TABLE") continue;
            try {
              var schema = Relatable.FromOdbc<dummy>(connection, table.TABLE_NAME) as RelationStream<dummy>;
              //var schema = new NdlRelationStreamOdbc<dummy>(connection, table.TABLE_NAME);
              headings.Add(new CommonHeading { Name = table.TABLE_NAME, Fields = schema.Fields });
              Logger.WriteLine(2, $"{table.TABLE_NAME} ({table.TABLE_TYPE}) => {headings.Last()}");
            } catch (Exception) {
              Logger.WriteLine(1, $"Table '{table.TABLE_NAME}' of type {table.TABLE_TYPE} skipped.");
            }
          }
          break;
        case SourceKind.Oledb:
          foreach (var table in Relatable.FromOledbSchema(connection)) {
          //foreach (NdlSchemaOledb table in new NdlRelationStreamOledbSchema(connection)) {
            if (table.TABLE_TYPE == "SYSTEM TABLE") continue;
            try {
              var schema = Relatable.FromOledb<dummy>(connection, table.TABLE_NAME) as RelationStream<dummy>;
              //var schema = new NdlRelationStreamOledb<dummy>(connection, table.TABLE_NAME);
              headings.Add(new CommonHeading { Name = table.TABLE_NAME, Fields = schema.Fields });
              Logger.WriteLine(2, $"{table.TABLE_NAME} ({table.TABLE_TYPE}) => {headings.Last()}");
            } catch (Exception) {
              Logger.WriteLine(1, $"Table '{table.TABLE_NAME}' of type {table.TABLE_TYPE} skipped.");
            }
          }
          break;
        }
        TupleTypeGen.Process(tw, skind, rootname, connection, headings.ToArray());
        if (options.Noisy >= 3) _output.WriteLine(tw.ToString());
        using (var fw = new StreamWriter(rootname + ".gen.cs")) {
          fw.Write(tw.ToString());
        }
      } catch (AndlException ex) {
        _output.WriteLine(ex.Message);
        return;
      } catch (Exception ex) {
        _output.WriteLine($"Unexpected exception: {ex.ToString()}");
        return;
      }
    }

  }
}
