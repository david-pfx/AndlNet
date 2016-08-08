Andl is A New Database Language. See http://andl.org.

Andl is a New Database Language designed to replace SQL and then go beyond.

Andl.NET is a pure implementation of the Relational Algebra for manipulating
data in any .NET language. While Andl itself is a full programming language,
Andl.NET relies on another language such as C# and simply provides the higher
order queries.

Andl.NET can perform relational queries at or beyond the capabilities 
of any SQL dialect. It can do all the ordinary things like select, where 
and join but it can also do generative queries, self-joins, complex 
aggregations, subtotals and running totals (a bit like SQL recursive 
common table expressions and windowing). 

Andl.NET has its own in-memory database so it can provide a complete 
application backend for any kind of user interface on any platform. It can 
easily be used to program a data model as a set of tables just like SQL, but 
including all the access routines, without the need for a Object Relational 
Mapper.

Andl.NET can retrieve data from Csv, Txt, Sql, Odbc and Oledb but does not
provide any persistence mechanism. 

The core feature of Andl.NET is an implementation of the generic interface 
IRelatable<T>. This provides a series of extension methods similar in flavour
to Linq's IEnumerable<T> and IQueryable<T>, but directly implementing the core
features of the Relational Algebra. The main differences from SQL are:
 * all joins are natural (by attribute name)
 * relations (tables) have no duplicate tuples (rows)
 * data values cannot be null.

Sample programs are included to demonstrate these capabilities. Familiarity 
with Linq will help in reading them.

A future release of Andl.NET will generate SQL so that queries can be
executed on a relational database backend.

FIRST DO THIS
=============

Download the binary release and unzip it somewhere.

Go to the Sample folder in a command prompt and run the samples.
    C>AndlN.Samples.exe


BUILDING ANDL
=============

The source code can be downloaded from https://github.com/davidandl/AndlNet.

The project should build 'out of the box' in Visual Studio 2015 with the .NET 
Framework 4.5, and possibly earlier versions. It builds an executable program 
that runs the samples, a class file generator, class library and unit tests.


LICENCE
=======

This version of Andl is free for any kind of experimental use, especially
helping to make it better. For now, the licence does not grant rights for 
distribution or commercial use. That will have to wait until I can choose the 
right licence, which depends a lot on who might want to use it.

Please contact me with any questions or suggestions at david@andl.org.
