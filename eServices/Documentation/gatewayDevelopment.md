# Gateway Development

*   The actual gateway solution is: `C:\eServices\eHub\Gateway\CargoWise.eHub.Gateway.Web.sln`
*   The solution with biztalk projects, that eHub uses to talk to the gateway is: `C:\eServices\eHub\Gateway\CargoWise.eHub.Gateway.sln`

## Running Tests

*   Unittests are in `CargoWise.eHub.Gateway.Tests`.
    +    They can be run normally through Visual Studio.
*   Integration tests are in `CargoWise.eHub.Gateway.IntegrationTests`.
    +   First ensure `NUnit 3 Test Adapter` is installed via Tools -> Extensions and Updates. Then they can be run normally through Visual Studio.
    +   Running an integration test will automatically deploy the web services `BillingWcfService`, `GatewayService` and `AuthenticationWebService` to your local machine.
        -   When the test completes the web services are automatically entirely removed.
    +   Running an integration test will deploy the databases `eHubTransactions`, `eHubScavenging`, `ediProd`, `ediProdCache`, `CargoWise.eServices.Billing` and `AuthenticationWebService` to your local machine if they do not exist.
        It will also deploy the eHub server level SQL script.
        -   When the test completes the databases are cleaned, so they can be used for the next test run.

## Fix Broken Integration Test Setup

DO NOT stop a debug attached test or otherwise kill a running test process.
Allow it to finish naturally so that the web services are undeployed and the database is cleaned up.

If you accidentally kill a running test, you will need to manually cleanup:
1.  In IIS manager, locate the 3 GUID named sites, `right click -> Explore` to open their folders.
2.  Delete the opened folders.
3.  Delete the 3 GUID named sites in IIS.
4.  In SSMS, delete the databases created by the integration tests.
    DO NOT rename databases instead of deleting them, this will cause an incomprehensible DB error.

## Debugging Integration Test

1.  Open Visual Studio as admin and open the `CargoWise.eHub.Gateway.Web` solution
2.  Put a breakpoint in your integration test on a line before a request is sent to the Gateway
3.  Run the integration test as debug.
4.  Wait till execution hits the breakpoint.
5.  Force start the lazily loaded IIS process: In IIS select the site you need to attach to -> Browse localhost on *:XXXXX (http).
6.  Attach to the IIS processes in visual studio. Ctrl-Alt-P -> Select all w3wp.exe
7.  Set your breakpoints in The gateway or one of its dependencies.
8.  Continue execution of the test.