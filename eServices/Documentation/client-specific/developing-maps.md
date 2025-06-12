# Developing Client Specific Interfaces

## Overview

1.	Create/locate your interface
	*	[How to create new eHub solution or project​](https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/How%20to%20create%20new%20eHub%20client-specific%20solution%20or%20project.aspx)
	*	Once located, ALWAYS "Get Specific Version…" with both "Overwrite" ticked
2.	Follow these [instructions](../setupFunctionalVM.md) to setup a biztalk VM for functional testing your map and SQL
3.	Write the map and unittests. Refer to "Writing XSLT" below
4.	In order to configure production eHubTransactions to run your map, you will need to write a SQL script which will be run on production at deploy time.
	1.	Copy production eHubTransactions data to the database on the VM TODO
	2.	Write and test your SQL script using the VM database
		1.	Start with the template below
		2.  Fill in the eHubClient, eHubTransformationSet etc.
		3.  The code mappings need to provide the same table names and input/output names as mocked out by unittests.
		4.	Provide a csv for the actual code mappings data
5.	Functionally test your map
	1.	Run your SQL script with COMMIT
	2.	Deploy your dlls to biztalk TODO
	3.	Setup new send and receive ports TODO
	4.	Run eHubAdmin TODO
	5.	Put a message in the receive port folder then send it by copying it.
6.	Create a shelf
	*	​Do NOT include *.btm.cs
	*	DO include *.vs*scc (TFS binding files) for new solution/project
7.	Attach your SQL script to eDocs in the WI
8.	Attach code mapping csv file to eDocs if required
9.	In the tasknotes for your development task in the WI, include:
	1.	a link to your shelf (When looking at the shelf in visual studio, you can go Actions -> Open In Browser)
	2.	the name of the SQL script in eDocs
	3.	code mapping csv filename if included
10.	When review has passed, checkin the shelf by changing the CHK task to a CH0 task and setting the name of the task to the name of the shelveset then saving.

## Writing XSLT

*	​Refer to new and similar maps for commonly mapped fields and logic.
*	Use XSLT instead of functoids. BizTalk generates terrible XSLT and it’s hard to track history for functoids.
*	When mapping for X12 or EDIFACT, you can google for a specification of the format.
*	The eHub maps rely on database access, so the unittests mock out the C# code so it doesn't rely on the database.
*	When generating Universal XML, many `*Collection` tags have a `Content` attribute, if it exists ensure it is set to `Content="Partial"`. Partial allows CW1 to handle matching logic.
*	The `*_ext.xml` file specifies classes that can be accessed from the XSLT. This is often used to access "CargoWise.eHub.Core.Transforms.Helper​", which includes the following classes:
	+	CodeMapper for code mappings and stored procedures
	+	DateMapper for date conversions
	+	UnitConverter for length, weight, volume conversions
	+	ContextAccessor for BizTalk context properties​
*	Per-map C# code can be included in userCSharp TODO
*	[Muenchian grouping](https://en.wikipedia.org/wiki/XSLT/Muenchian_grouping)

## Creating SQL for a new interface

1.  Open `C:\eServices\Documentation\client-specific\tools\ClientSpecificSQLFromXLSX\ClientSpecificSQLFromXLSX.sln` in Visual studio 2017
2.  Rebuild all
3.  Right click and Save link as -> [eHub - Client-Specific Interfaces.xlsx](https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/_layouts/15/download.aspx?SourceUrl=/Development/Development%20Team%20Workspace/Shared%20Documents/eServices/eHub%20-%20Client-Specific/eHub%20-%20Client-Specific%20Interfaces.xlsx).
	*	Never open it in your browser unless you need to edit it, since opening locks the file.
	*	Save it to `C:\eServices\Documentation\client-specific\tools\ClientSpecificSQLFromXLSX\ClientSpecificSQLFromXLSX\bin\Debug\interfaces.xlsx`.
4.  Open `C:\eServices\Documentation\client-specific\tools\ClientSpecificSQLFromXLSX\ClientSpecificSQLFromXLSX\bin\Debug` in a terminal.
5.  Run `./ClientSpecificSQLFromXLSX .\interfaces.xlsx XXX > foo.sql`
	*	replace XXX with the 3 letter client code included in the title of the WI.
	*	`foo.sql` will contain the generated script.

An overview of what this script does:
1.  Pulls the `OrgHeader.OH_PK` of the eHubClient from production ediProdCache
2.  Pulls remaining fields of `eHubClient`s and `ehubTransformationSet`s from the interfaces in the spreadsheet that are WIP and have the same 3 character client ID.
3.  Only creates `eHubClient`s for ehub ids that are longer than 9 characters. 9 character ehub ids are created automatically by a service task instead.

Further manual changes:
*	You will need to manually change any fields containing TODO.
*	Code mappings should not be included in the script, instead they should be done in eHubPortal and then exported to csv, then included in the WI seperately.

## Debug xslt

```
cd  `C:\eServices\BizTalk.UnitTestFX\CargoWise.BizTalk.UnitTestFX.sln` in visual studio
paket.exe restore
```

1.  Open `C:\eServices\BizTalk.UnitTestFX\CargoWise.BizTalk.UnitTestFX.sln` in visual studio.
2.  Compile the solution.
3.  In your unittest replace the call to `ExecuteCompiled` with `ExecuteCompiledWithXslDebug`.
    e.g. `mapTester.ExecuteCompiledWithXslDebug<UShipment2AIRAEU>(input, expectedOutput, @"C:\eServices\eHub\Products\SGCustoms\MHAccess\BizTalk\Send\Maps\UShipment2AIRAEU\UShipment2AIRAEU.xsl");`
4.  Add a breakpoint in your xslt file.
5.  Run the unittest as debug.