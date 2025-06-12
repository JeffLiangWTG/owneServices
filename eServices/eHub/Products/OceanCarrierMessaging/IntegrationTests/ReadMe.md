## Summary
These Integration tests are run with the Infrastructure XML data.

Testing against the real database :

If you want to test with the real database to see if a functiontality works, please use TestRealDatabase/TestRules.cs or TestRealDatabase/TestMappings.cs
To do so you have to remove the [Ignore(...)] attribute from the class and run your test. Remember to put back the Ignore attribute after you finished testing against real database

Testing a specific provider:

If you want to test specific provider or check your change won't break production, You can use or create new *RoutingRuleTest (for example INTTRA_RoutingRuleTest.cs).
Specific provider test classes are derived from RoutingRuleIntegrationTestBase which is responsible for preparing the test database on your machine before the test runs and does the following :
	inserts the contents of the common eHubTransactions_Infrastructure.xml file located in TestDatabase/TestData folder into your local databse. This file should contain the mandatory data records for the whole system to work.
	inserts the contents of the eHubTransactions_Infrastructure.xml located in the provider specific folder (for example INTTRA/INTTRA_RoutingRuleTestData) into your local databse. This file should contain the mandatory records for the specific provider to work.
	inserts the contents of the eHubTransactions.xml located in the provider specific folder (for example INTTRA/INTTRA_RoutingRuleTestData) file into your local databse. This file should contain the new data that you are adding to the database and you want to test.

To update */eHubTransactions_Infrastructure_infrastucture.xml, check [OCM-IT-Manager](#ocm-it-manager)
To update */eHubTransactions.xml, check [Generate Test Data](http://tfs.wtg.zone:8080/tfs/CargoWise/eServices/_wiki/wikis/eServices.wiki?pagePath=%2FProducts%2FeHub%2FIntegration%20Tests%2FTest%20Data&wikiVersion=GBwikiMaster&pageId=97)

Note: Please disable ReSharper's NUnit Test(Resharper/Options/Tools/Unit Testing/NUnit > Untick all) and Install "NUnit 3 Test Adapter v3.11.2" (Tools/Extensions and Updates...) to run these integration tests. You will able to find integration tests in Test/Windows/Test Explorer  

## OCM-IT-Manager

This is a script that helps you synchronize data from production or any other server you want to test, it will override your local common and provider-specific eHubTransactions_infranstructure.xml files.
The script uses TestBase.sql to create the common eHubTransactions_Infrastructure.xml file and TestProvider.sql to create the provider specific *_infrastructure.xml file

Note: Please check-out *_infrastucture.xml files before running the scripts or remove the readonly attribute from the files. 

If you want to add a new one, please create a new folder naming the Provider and create eHubTransactions.xml and eHubTransactions_Infrastructure.xml under [Provider]_RoutingRuleTestData and add in OCM-IT-Manager.ps1's provider list and the below:
Currently, supported Providers only contains:
```Easipass```; ```INTTRA```; ```NINGBO```; ```SouthEast```; ```Base```

Run the following command in your local machine, changing Server to the name of your Machine, and Provider to the name of TestProvider.

```
cd C:\git\eServices\eHub\Products\OceanCarrierMessaging\IntegrationTests
.\OCM-IT-Manager.ps1 -Server SYDCO-WXXX-X -Provider All
i.e. .\OCM-IT-Manager.ps1 -Server ehubtransactions.db.wisegrid.net -userName ehubreader -password ehubrocks -Provider All
```

*	Type -help for More information, Here are other examples you may want to try 
	-	.\OCM-IT-Manager.ps1 -help
	-	.\OCM-IT-Manager.ps1 -Provider All
	-	.\OCM-IT-Manager.ps1 -Provider INTTRA
	-	.\OCM-IT-Manager.ps1 -Server SYDCO-WJFL-2 -Provider Base
	-	.\OCM-IT-Manager.ps1 -Server ehubtransactions.db.wisegrid.net -userName ehubreader -password ehubrocks
	-	.\OCM-IT-Manager.ps1 -Server SYDCO-WJFL-2


