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
using System.Text;

namespace Andl.Common {

  ///==========================================================================
  /// <summary>
  /// Implement base class for templating engine.
  /// 
  /// Includes specials for Sql
  /// </summary>

  public abstract class Templater {
    // settable as required
    public string SubDelim = "~";    // delimits alternative sub-templates
    public char LeftDelim = '<';     // tokens are inside <here>
    public char RightDelim = '>';
    public char IdQuoted = '!';   // token is an ident that must be quoted
    public char IdFrom = '@';     // token is FROM target, quote if not subquery (inside parens)
    public char IdAlias = '#';    // token is also FROM target, alias if subquery

    // List of templates to search (instead of inheritance)
    protected List<Dictionary<string, string>> _templatedicts = new List<Dictionary<string, string>>();

    // create a formatter for a particular template
    public TemplateFormatter Create(string key) {
      var t = new TemplateFormatter {
        Templater = this,
        Template = FindTemplate(key),
      };
      return t;
    }

    public string FindTemplate(string name) {
      foreach (var dict in _templatedicts)
        if (dict.ContainsKey(name)) return dict[name];
      Logger.Assert(false, name);
      return null;
    }

    // Create and process a single template
    public string Process(string template, Dictionary<string, Func<int, string>> dict, int index = 0) {
      return Create(template).Process(dict, index).ToString();
    }

    // Create and process a list of items separated by delimiter
    public string Process(string template, Dictionary<string, Func<int, string>> dict, int howmany, string delim) {
      return Create(template).Process(dict, howmany, delim).ToString();
    }
  }

  ///==========================================================================
  /// <summary>
  /// Implement engine to perform a single formatting operation
  /// </summary>
  public class TemplateFormatter {
    //const string _delim = "~";    // delimits alternative sub-templates
    //const char _langle = '<';     // tokens are inside <here>
    //const char _rangle = '>';
    //const char _idquoted = '!';   // token is an ident that must be quoted
    //const char _idfrom = '@';     // token is FROM target, quote if not subquery (inside parens)
    //const char _idalias = '#';    // token is also FROM target, alias if subquery
    public Templater Templater { get; set; }
    public string Template { get; set; }
    protected StringBuilder _builder = new StringBuilder();

    public override string ToString() {
      return _builder.ToString();
    }

    // Single process
    public TemplateFormatter Process(Dictionary<string, Func<int, string>> dict, int index = 0) {
      // special: means not implemented for this dialect, result will be empty string
      if (Templater == null) return this;
      // see if we need to use a subtemplate
      var subtemplx = dict.ContainsKey(Templater.SubDelim) ? Int32.Parse(dict[Templater.SubDelim](index)) : 0;
      var templ = Template.Split(new string[] { Templater.SubDelim }, StringSplitOptions.None)[subtemplx];

      var pos = 0; 
      while (true) {
        var apos = templ.IndexOf(Templater.LeftDelim, pos);
        if (apos == -1) {
          _builder.Append(templ, pos, templ.Length - pos);
          break;
        } else {
          _builder.Append(templ, pos, apos - pos);
          // is this a token needing something special?
          var schar = Char.IsLetter(templ, ++apos) ? '\0' : templ[apos++];
          var bpos = templ.IndexOf(Templater.RightDelim, apos);
          var token = templ.Substring(apos, bpos - apos);
          Logger.Assert(dict.ContainsKey(token), token);
          var repl = dict[token](index);
          var replx = (schar == Templater.IdQuoted) ? IdQuote(repl) 
            : (schar == Templater.IdFrom) ? IdFrom(repl) 
            : (schar == Templater.IdAlias) ? IdAlias(repl) 
            : repl;
          _builder.Append(replx);
          pos = bpos + 1;
        }
      }
      return this;
    }

    // Iterative processing, joined by delimiter
    public TemplateFormatter Process(Dictionary<string, Func<int, string>> dict, int howmany, string delim) {
      for (var i = 0; i < howmany; ++i) {
        if (i > 0)
          _builder.Append(delim);
        Process(dict, i);
      }
      return this;
    }

    public void Append(string text) {
      _builder.Append(text);
    }

    // Quote an Sql identifier
    static string IdQuote(string value) {
      return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    static int _aliasid = 0;

    // This is a FROM target.
    // If it starts with paren, it's a subselect, else it's an ident
    static string IdFrom(string value) {
      return (value.StartsWith("(")) ? value
        : IdQuote(value);
    }

    // Same as FROM, but add alias to subquery
    static string IdAlias(string value) {
      return (value.StartsWith("(")) ? $"{value} AS AA_{++_aliasid}"
        : IdQuote(value);
    }
  }
}
