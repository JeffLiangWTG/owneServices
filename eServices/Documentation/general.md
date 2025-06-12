# General

## Test and production servers

The eServices team builds and maintains eHub.
It is a system that receives messages, transforms them to the correct format, then sends them again.

Internally eHub is mainly BizTalk, you can interact with it through some web interfaces though:
*	You can see messages going through on the eHubAdmin web interface.
*	And you can view and modify configuration on the eHubPortal web interface.​​

### CI

*	http://crikey.wtg.zone/

### New test server

*   BizTalk/SQL - `eHub-test.db.sand.wtg.zone`
   +   ehubreader account (use this by default)
       -   user: ehubreader
       -   pass: ehubrocks
   +   devadmin account (use this ONLY when you need write access, additionally follow instructions at "Connecting to SQL Servers")
       -   user: devadmin
	   -   pass: 3hubRock$
*   eHubAdmin test  - http://au2sp-shub-401.sand.wtg.zone/eHubAdmin
*   eHubPortal test - http://au2sp-shub-401.sand.wtg.zone/eHubPortal
*   SMTP dummy server - http://sydco-wlkt-1:5000

### Old test server (t​​hub)

*   BizTalk/SQL - syd-thub-1​
*   eHubAdmin test  - http://syd-tweb-1/eHubAdmin
*   eHubPortal test - http://syd-tweb-1/eHubPortal

### Production
*   SQL - `ehubtransactions.db.wisegrid.net​` (sql user: eHubReader, pass: ehubrocks)
   +   ehubreader account
       -   user: ehubreader
       -   pass: ehubrocks
*   eHubAdmin production  - http://ehubadmin.wtg.zone
*   eHubPortal production - http://ehubportal.wtg.zone (WARNING: THIS WILL ALLOW MODIFICATION OF REAL PRODUCTION DATA)

### Old production

Refer to this database for eHubArchive prior to 2019-07-21.
*   SQL - ​`wg1-vsql-1.wg.cargowise.com` (sql user: eHubReader, pass: ehubrocks)
   +   ehubreader account
       -   user: ehubreader
       -   pass: ehubrocks
*   eHubAdmin production  - http://ehubadmin.wtg.zone/OldEhubAdmin

### Connecting to SQL servers

DO NOT connect to ehub test or production servers with write access (devadmin account) from your local machine.
It is too easy to get test/prod and local servers mixed up in one local SSMS session.
Instead RDP into the remote server then set windows to a uniquely identifiable color e.g. bright red or bright yellow
```
Control Panel -> Appearance and Personalization -> Change the color of your taskbar and windows borders
```
Then you can open SSMS in the RDP window and connect to that machines sql server.

## General deployment

Most projects are setup so you can do:

1.  `msbuild` -- build the project
2.  `msbuild -t:bat:package` -- package the project
3.  `msbuild -t:bat:deploy` -- deploy the project to your local machine.

However not everything is setup up like this, and some projects require a lot more setup.

## Creating a new eHub project

Manual process:

1.  Right click solution -> add -> new project -> naming same as folder name first(e.g. Schemas) and rename the project using the eHub naming format(e.g. "CargoWise.eHub.Products.USCustomsAirAMS.Schemas")
2.  Fill out relevant AssemblyInfo.cs follows other existing projects.
3.  Right click project -> Add -> C:/eServices/CommonAssemblyInfo -> Add As Link
4.  Right click project -> Add -> C:/eServices/EDI-Release-private.snk -> Add As Link
5.  (excluding test projects) Right click project -> properties -> Signing -> Tick sign the assembly + choose EDI-Release-private.snk
6.  Right click project -> properties -> Build ->set output path to Bin\1.0.0.0 for both Debug and Release
7.  Right click project -> properties -> Application -> set assemblyname, default namespace to project name from step 1

Alternatively use [these macros](https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/_layouts/15/WopiFrame.aspx?sourcedoc=%7B5933B82E-23EE-4348-875C-D206E555BD41%7D&file=Macros%20to%20Create%20BizTalk%20Projects.docx&action=default)
The new solution instructions are for client specific solutions but you can ignore those and still use the macros.

## Client IDs

All entities that communicate via eHub are assigned an eHub client ID, stored in the eHubClient table.
*	CW1 eHub clients are given a 9 character client ID AAABBBCCC:
	+   AAA: The company code
	+   BBB: The branch code
	+   CCC: The database code
*	Other eHub clients can use any format.