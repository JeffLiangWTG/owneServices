# Test Server Deployment

## Deploy eHubPortal to test server (au2sp-shub-401) 

On your local machine, run the following as admin in Developer Command Prompt for VS 2019 (powershell doesn't work)

```cmd
cd C:\eServices\eHub\Portal\Portal
msbuild .\Portal.csproj -t:bat:package
msbuild .\Portal.csproj -t:bat:deploy -p:Profile=test;UserName="corp\first.last";Password="putYourPasswordHere"
```

## Deploy eHubGateway to test server (au2sp-shub-401) 

On your local machine, run the following as admin in Developer Command Prompt for VS 2019 (powershell doesn't work)

```cmd
cd C:\eServices\eHub\Gateway\Gateway.Host
msbuild .\CargoWise.eHub.Gateway.Host.csproj -t:bat:package
msbuild .\CargoWise.eHub.Gateway.Host.csproj -t:bat:deploy -p:Profile=test;UserName="CORP\first.last";Password="putYourPasswordHere"
```

TODO: this is awful, change so we dont need to enter a password ;-; 