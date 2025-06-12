# Dev Scripts

## Setup

You will need to install powershell 5, enable powershell and include the scripts in your path:

1.  Install management framework 4 https://www.microsoft.com/en-us/download/details.aspx?id=40855 (Windows6.1-KB2819745-x64-MultiPkg.msu)
2.  Install management framework 5 https://www.microsoft.com/en-us/download/details.aspx?id=50395 (Win7AndW2K8R2-KB3134760-x64.msu)
3.  Open powershell and powershell (x86) as administrator. In both windows run "set-executionpolicy unrestricted" confirm with y.​​
4.  Add this directory 'C:\eServices\eHub\DevScripts' to your PATH system environment variable.

Internally some scripts use an $eServicesBin variable to store the path to 'C:\eServices\eHub\bin\'
You may need to change this variable if the path is different for you. e.g. it's on the 'D:/' drive

## qgl.ps1

Is a QuickGetLatest equivalent for eServices.
It fetches the latest code from tfs and then fetches the latest build files from the build server.

To run: `qgl`

## gatewaysend.ps1

Sends messages to ehub in the same way the gateway does i.e. inserts to the eHubInboxMessage table.
Useful for more accurately testing messsages sent from CW1, without having to deal with CW1 + gateway.

Run like:
```
cd C:\folder\message\file\is\in
gatewaysend message.xml -recipient ASYCUDA
```

## gatewaysend-to-script.ps1

Takes the same arguments as gatewaysend.ps1 and allows you to insert to the inbox.
However rather than running immediately, it generates a .sql file that can be run at a later time.

Run like:
```
cd C:\folder\message\file\is\in
gatewaysend-to-script message.xml -recipient ASYCUDA
```

## ehubdeploy.ps1

Deploys dlls to BizTalk and the GAC.
Useful for setting up a new VM or deploying changes to a VM for functional testing.

To deploy dlls for a new VM run: `ehubdeploy all`

To deploy ocm and start the BizTalk host instances: `ehubdeploy ocm start`

To deploy the AIRAEP dll in the current folder: `ehubdeploy currentdir CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Receive.Maps.AIRAEP2UEvent.dll`

You can provide as many arguments as you need.
The script will ensure each deployment step is run in the correct order.
The available arguments are:

*   all (all of the below except core-safe, $ExactDLLname, currentdir and gac)
*   gateway (CargoWise.eHub.Gateway.*)
*   core (CargoWise.eHub.Core.* deploys all core dlls, to achieve this, BizTalk ports are deleted and recreated)
*   core-safe (CargoWise.eHub.Core.* only deploy dlls that dont require deleting/recreating BizTalk ports)
*   products.core (CargoWise.eHub.Products.Core.*)
*   edi (CargoWise.eHub.Clients.EDI.*)
*   sgcustoms (CargoWise.eHub.Products.SGCustoms.*)
*   ocm (CargoWise.eHub.Products.OceanCarrierMessaging.*)
*   railinc (CargoWise.eHub.Products.RailInc.*)
*   $ExactDLLname e.g. CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Receive.Maps.AIRAEP2UEvent.dll
*   currentdir (deploy from the current directory instead of $eServicesBin)
*   start (Start the BizTalk host instances)
*   gac (gac every dll in bin/1.0.0.0) (I don't recommend this but could be useful)