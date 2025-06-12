# Billing Development

There are two solutions for billing, both are opened with Visual Studio 2017:
*	The main solution is: `C:\eServices\CargoWise.eServices.Billing\CargoWise.eServices.Billing.sln`. It contains the web service, the common windows service dependency and the misc + eHubRouter windows services.
*	The ehub specific solution is `C:\eServices\eHub\Billing\CargoWise.eServices.Billing.sln`. It contains the ehub windows service.

## Running Tests

*   Unittests will just work
*   Need to manually setup the environment to run integration tests:
    1.  Open windows explorer to C:\inetpub ->  Right click wwwroot -> properties -> Security -> Advanced -> Change permissions -> Tick Replace all child object permissions entries with inheritable permission entries form this object. -> OK -> Yes
    2.  Open the main solution in Visual Studio -> In solution explorer -> Right click `CargoWise.eServices.Billing.Database` -> publish -> Edit -> Set fields to:
        1.  Server Name: localhost
		2.  All other fields default
	3.  Ok -> Publish
    4.  From `C:\eServices\CargoWise.eServices.Billing\CargoWise.eServices.Billing.WcfService\Web.config`
        Remove all instances of: `<security><ipSecurity allowUnlisted="false"><add allowed="true" ipAddress="127.0.0.1" subnetMask="255.255.255.255"/></ipSecurity></security>`
    5.  Rebuild solution
	6.  Run tests

## Deploy Web Service to local machine

1.  Follow instructions to setup for running integration tests.
2.  Open Developer Powershell for VS 2019 as admin, run: `cd C:\eServices\CargoWise.eServices.Billing\CargoWise.eServices.Billing.WcfService; msbuild; msbuild -t:bat:package; msbuild -t:bat:deploy`

## Deploy Windows Service to local machine

1.  Follow instructions to setup for running integration tests.
2.  Open `C:\eServices\CargoWise.eServices.Billing\CargoWise.eServices.Billing.WindowsService\BillingService.cs`, then insert the following at the start of OnStart()
```
			System.Net.ServicePointManager.ServerCertificateValidationCallback +=
				(se, cert, chain, sslerror) =>
				{
					return true;
				};
```
3.  To deploy any billing windows service, open Developer Powershell for VS 2019 as admin, cd to its project folder, then package and deploy it e.g. `cd C:\eServices\CargoWise.eServices.Billing\CargoWise.eServices.Billing.Collector.Misc.WindowsService; msbuild; msbuild -t:bat:package; msbuild -t:bat:deploy`