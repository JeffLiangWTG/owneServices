# Customer Setup - TNPA


The [$/eServices/Operation/Bindings/AddTNPAClient.ps1](../../../Operation/Bindings/AddTNPAClient.ps1) script will generate all the binding files needed for a new TNPA interface.

You should receive the details for the new client in a table like this:

|Customer|eHUB ID (Client)|Sender ID (Code)|FTP Server|User Name|Password|Outbound folder|Inbound folder|
|--------|----------------|----------------|----------|---------|--------|---------------|--------------|
|GAC LASER INTERNATIONAL LOGISTICS (PTY)|GA3CPTPRD|GACLSR|sftp.transnet.net|gac001wh|Gac001wh#001|/gac001wh/in|/gac001wh/out

If so then paste them into the `$data` text block at the start of the script replacing the existing data rows (header row is required):

```powershell
$data = @"
Customer	eHUB ID (Client)	Sender ID (Code)	FTP Server	User Name	Password	Outbound folder	Inbound folder
BLUE STRATA SUPPLY CHAIN (PTY) LTD (INVESTEC IMPORT SOLUTIONS)	 INVBSSPRD	BLUSTR	sftp.transnet.net	blu001dg	Sftp001#	/blu001dg/in	/blu001dg/out
"@
```

If you receive the client's details in a different format you will need to manually format them into the tab-delimited row for this column layout.

Run the script and it will add and update the required binding files in TFS.

Copy those files to a deployment folder on the production server as `\\sydwp-sbts-1.wisecloud.zone\Deploy\[yyyyMMdd]\[IncidentNumber]\Bindings`.

While logged onto the server, drag the `[IncidentNumber]` folder onto the `CargoWise.eHub.BizTalk.DeployRelease.WG-PROD.DeployOnly` shortcut in `C:\Deploy`. The deployment script will check if there is a folder called `Bindings` in the directory and if there is then it will run the `C:\Deploy\Scripts\BuildBindings.ps1` script and then import the generated `CargoWise.eHub.BindingInfo.xml` file into BizTalk.

After the deployment completes you will need to manually start the new send port via the BizTalk Administration Console.

Check the receive location log at `\\sydwp-sbts-1.wisecloud.zone\c$\Logs\BizTalk\Interfaces\FPM\FPM_TNPA_Rcv.Receive.log` to ensure the new connection is working.

Check-in the binding files and the updated script (if you haven't already done so).

