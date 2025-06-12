# Have you tried turning it off and on again?

A list of extremely silly things that happen from time to time, and their sometimes sillier solutions.

## Changes to code are not used in unittests (GAC)

BizTalk stores deployed assemblies in the Global Assembly Cache (GAC).
While running cmd as admin, you can use the `gacutil` command to interact with gac.

On your local machine you do NOT want the eHub assemblies in the GAC.
Unittests will attempt to use these GAC'd assemblies ignoring your actual compiled assemblies.
It is easy to accidentally deploy an assembly to the GAC from visual studio.
If your unittests are doing weird things, you can check for any accidentally gac'd assemblies `gacutil -l | find "CargoWise.eHub"`
To remove, run the command with the name of the assembly that you need to remove. `gacutil -u CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN_INTTRA`

In your biztalk VM when you deploy assemblies they will be added to the GAC.
This is correct.

## Strange connection issues

There might be issues getting through the company proxy after changing password: Just restart your machine

## Can no longer login to VM with CORP\username:

1.  Log in as DevAdmin
2.  Search for program "rename this computer" -> Change
3.  Change to a dummy workgroup .e.g. "A" (log in)
4.  Change to the domain "sand.wtg.zone" (log in with corp account)
5.  restart

If that doesnt work also try: 

1.  Search for program "select users who can use remote desktop" -> Select Users -> Add
2.  Login with CORP\First.Last account
3.  Add your CORP\First.Last account (click Check Names after typing it in)
4.  After restart VM

## Biztalk messages are no longer sending?

After restarting biztalk VM, all biztalk service instances are stopped.
In BizTalk Server Admin -> Groups -> Applications -> CargoWise.eHub -> right click -> start

## VM Wont Start

An error occured while attemping to start the selected virtual machine.
'vm-name' failed to change state.

To resolve, restart your local machine (not your VM)

## Vm cant connect to the internet.

1.  Log into VM via hyper V using username: DevAdmin password: 3hubRock$
2.  Search proxy into start menu, select first result.
3.  Open LAN settings
4.  Press OK (Yes this is dumb, yes it worked)
5.  Restart VM from the DevAdmin account

##CredSSP Encryption Oracle Remediation Error

Follow instructions at to work around it. https://blogs.technet.microsoft.com/mckittrick/unable-to-rdp-to-virtual-machine-credssp-encryption-oracle-remediation/

## Object reference not set to an instance of an object.

When setting up biztalk to run on my local VM, I try to send a message and get this error message:
```
A message received by adapter "WCF-SQL" on receive location "Gateway_SelectInboxMessageByStatus_OCM" with URI "mssql://localhost//eHubTransactions?InboundId=SelectInboxMessagesByStatus&Category=OCM" is suspended. 
 Error details: There was a failure executing the receive pipeline: "CargoWise.eHub.Products.OceanCarrierMessaging.Pipelines.OCM_ReceiveGatewayMessage, CargoWise.eHub.Products.OceanCarrierMessaging.Pipelines, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" Source: "Inbox Disassemble and Route" Receive Port: "Gateway_SelectInboxMessageByStatus_OCM" URI: "mssql://localhost//eHubTransactions?InboundId=SelectInboxMessagesByStatus&Category=OCM" Reason: Object reference not set to an instance of an object.  
 MessageId:  {B7541885-A1A4-4373-9D03-D368C3E461D2}
 InstanceID: {D385CCDD-A2D4-4BAA-B9A7-7E9F3394DA61}
```

The error was due to C:\Program Files (x86)\Microsoft BizTalk Server 2010\BTSNTSvc.exe.config and C:\Program Files (x86)\Microsoft BizTalk Server 2010\BTSNTSvc64.exe.config pointing to localhost\ehubtest while biztalk transport configs were pointing at localhost.
Or maybe the error was because of localhost/ehubtest being out of date. Either way I meant to have everything point at localhost.

## The underlying provider failed on Open

When setting up biztalk to run on my local VM with ehubtest, I try to send a message and get this error message:

```
A message received by adapter "WCF-SQL" on receive location "Gateway_SelectInboxMessageByStatus_OCM" with URI "mssql://localhost//eHubTransactions?InboundId=SelectInboxMessagesByStatus&Category=OCM" is suspended. 
 Error details: There was a failure executing the receive pipeline: "CargoWise.eHub.Products.OceanCarrierMessaging.Pipelines.OCM_ReceiveGatewayMessage, CargoWise.eHub.Products.OceanCarrierMessaging.Pipelines, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" Source: "Inbox Disassemble and Route" Receive Port: "Gateway_SelectInboxMessageByStatus_OCM" URI: "mssql://localhost//eHubTransactions?InboundId=SelectInboxMessagesByStatus&Category=OCM" Reason: The underlying provider failed on Open.  
 MessageId:  {587F47A6-E151-4926-93FF-5ADAEB66C041}
 InstanceID: {31A4C67F-6258-43C5-905F-1DB52FB062C2}
```
​
Its because I used localhost/ehubtest instead of localhost\ehubtest in the connection strings. 