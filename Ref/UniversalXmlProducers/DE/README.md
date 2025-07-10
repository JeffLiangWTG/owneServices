German Master data conversion
=============================

Purpose
-------
The program grabs the German masterdata from different sources, and converts 
it to a predefined XML format.

Usage
-----
The startup project is DEReferenceData.CmdLine, which is a console program.

Currently, there are two types of data which are grabbed/converted:
- Tariff data - they are prepared on ZNET side out of the tariff used by zara
- Exchange rates - they are grabbed from http://zoll.de 

The command line parameters are as follows:
1. function: 
- `tariff` to convert German tariff data
- `listedrates` to convert listed rates pulled from http://zoll.de
- `nonlistedrates` to convert non listed rates pulled from http://zoll.de
2. output file name, ex. `GermanTariff.xml`
3. (optional, only applies to rates) start date for rate download/conversion, defaults to the first day of the current month, 
e.g. `2016-01-01`, in a format parsable on the executing system by `DateTime.Parse()`


Compiling
---------
The solution requires Visual Studio 2017 or higher, and uses the "new" .csproj format

Dependencies
------------
- .NET-Framework 4.5.2 or higher
- HtmlAgilityPack 1.6.15




