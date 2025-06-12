# Removing Client Specific Maps

## Finding the transformation set

1.  Right click and Save link as -> [eHub - Client-Specific Interfaces.xlsx](https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/_layouts/15/download.aspx?SourceUrl=/Development/Development%20Team%20Workspace/Shared%20Documents/eServices/eHub%20-%20Client-Specific/eHub%20-%20Client-Specific%20Interfaces.xlsx). Never open it unless you need to edit it, since opening locks the file.
2.  In the WI you are provided with the "Billing - Interface Name", use the spreadsheet to locate the "Sender", "Recipient" and "Management Portal - Interface Name".
3.  Use the sender and recipient to locate the transformation set on [production ehubAdmin](http://ehubportal.wtg.zone/TransformationSet/Index)
4.  Save a list of the URLs, or just keep each page open in a seperate tab.
5.  Identify the transformations that are unique to the transformation set, these will need to be deleted.

## Code changes

Each transformation identified as unique will need its code deleted.
If every transformation in the client's solution is removed then the entire solution can be removed, otherwise follow these steps to remove an individual project.

1.  Open the solution in `C:\eServices\eHub\Clients`
2.  Right click the project in the Solution Explorer -> Remove
3.  Locate the folder for the deleted project in Source Control Explorer -> right click -> delete
4.  In the Solution Explorer remove the tests for the deleted project.
5.  In the Solution Explorer remove any schemas that were used only by the deleted project.
6.  Ensure the solution continues to build and all tests pass.

## SQL changes

Each transformation set and its related data will need to be deleted from the production SQL server.

1.  Setup a local clone of production data to test your script on.
2.  Modify the below template to delete your map and related unused data.
	1.  Replace the names used in the `CHANGE THESE` section. Refer to the transformation set pages found earlier on eHubAdmin.
	2.  Modify the template appropriately to handle all transformation sets that need to be removed. For example:
		*	Use `%` in TS_NAME or CC_ID to match multiple rows via `LIKE` matching
		*	Repeat part of or all of the template
3.  Change `DELETE` to `SELECT * ` during development to see the values in the rows that will be deleted.
4.  Change back to `DELETE` to verify that a constraint doesnt break your script.
5.  When your script is finished you can do one final check by changing the `ROLLBACK` to `COMMIT`, then you can run the ehub portal locally to help verify you have deleted the correct data. `C:\eServices\eHub\Portal\Portal\Portal.sln`
6.  Add your script to eDocs in your WI. Make sure your script ends in `ROLLBACK` and your statements use `DELETE`.

```sql
USE eHubTransactions
BEGIN tran

-- CHANGE THESE
DECLARE @TS_Name nvarchar(50) = 'Intelligent Audit/eShop 110 - Send A/R Invoices'
DECLARE @TT varchar(512) = 'CargoWise.eHub.Clients.CAS.Transforms.UniTrans_2_X12_110.UniTrans_2_X12_110, CargoWise.eHub.Clients.CAS.Transforms.UniTrans_2_X12_110, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'
DECLARE @CC_ID varchar(36) = 'CASADLADL_ESI'
DECLARE @DT_Code varchar(200) = 'http://cargowise.com/ehub/clients/foo/2017/02#bar'

-- KEEP THIS THE SAME
DECLARE @TS table (TS_PK uniqueidentifier)
INSERT INTO @TS SELECT TS_PK FROM eHubTransformationSet WHERE TS_Name LIKE @TS_Name

DECLARE @CC table (CC_PK uniqueidentifier)
INSERT INTO @CC SELECT CC_PK FROM eHubClient WHERE CC_ID LIKE @CC_ID

DELETE FROM eHubCodeMapValue
	WHERE CV_CK IN (SELECT CK_PK FROM eHubCodeMapKey
		WHERE CK_CS IN (SELECT CS_PK FROM eHubCodeSet WHERE CS_TS IN (SELECT TS_PK FROM @TS)))
DELETE FROM eHubCodeMapKey WHERE CK_CS IN (SELECT CS_PK FROM eHubCodeSet WHERE CS_TS IN (SELECT TS_PK FROM @TS))
DELETE FROM eHubCodeSetResult WHERE CR_CS IN (SELECT CS_PK FROM eHubCodeSet WHERE CS_TS IN (SELECT TS_PK FROM @TS))
DELETE FROM eHubCodeSet WHERE CS_TS IN (SELECT TS_PK FROM @TS)

DELETE FROM eHubTransformationMapping WHERE TM_TS_PK IN (SELECT TS_PK FROM @TS)
DELETE FROM eHubTransformationType WHERE TT_TransformationType IN (@TT)
DELETE FROM eHubTransformationSet WHERE TS_PK IN (SELECT TS_PK FROM @TS)
DELETE FROM eHubMessageType WHERE DT_CODE LIKE @DT_Code
DELETE FROM eHubClient WHERE CC_PK IN (SELECT CC_PK FROM @CC)

--COMMIT
ROLLBACK
```

## Deploy to test

If the map dll is added to the test biztalk server, remove it.
You may need to modify your SQL script to run it on the test server as the PKs may be different to production.
To find out, run it with `ROLLBACK` first. (Make sure it reports that rows were deleted for every `DELETE` statement)