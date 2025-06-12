[CmdletBinding(PositionalBinding=$false)]

param (
   [Parameter(Mandatory=$true)][string]$server,
   [Parameter(Mandatory=$false)][string[]]$bt_dlls,
   [Parameter(Mandatory=$false)][string[]]$gac_dlls,
   [Parameter(Mandatory=$false)][switch]$currentdir,
   [Parameter(Mandatory=$false)][switch]$all,
   [Parameter(Mandatory=$false)][switch]$adapters,
   [Parameter(Mandatory=$false)][switch]$hosts,
   [Parameter(Mandatory=$false)][switch]$bindings,
   [Parameter(Mandatory=$false)][switch]$start
)

$ErrorActionPreference = 'Stop'

$scripts='C:\eServices\eHub\DevScripts\'
$eServicesBin='C:\eServices\eHub\bin\'

function new_session() {
	$session = New-PSSession -ComputerName $server

	Invoke-Command -Session $session -ScriptBlock {
		$ErrorActionPreference = 'Stop'
		# The sessions keeps track of the remote current directory, so set it once now.
		cd $Using:eServicesBin
	}
	if ($session.State -ne "Opened") { exit }
	return $session
}

# Create a session for general use.
# For finally blocks that rollback temp changes, create a new session in case this one is closed.
$session = new_session

function deploy_bt_dlls([string] $app, [string[]] $dlls) {
	# Transfer dlls to server
	foreach ($dll in $dlls) {
		Copy-Item -Path $dll -ToSession $session $eServicesBin
	}

	# Deploy dlls to biztalk
	Invoke-Command  -Session $session -ScriptBlock {
		foreach ($dll in $Using:dlls) {
			if (-not ($dll -like "*.dll")) {
				Write-Host "Not a DLL:" $dll
				exit
			}
			elseif (-not (Test-Path $dll)) {
				Write-Host "DLL does not exist:" $dll
				exit
			}
			elseif ($dll.length -gt 93) {
				Write-Host 'Failed to deploy: DLL filename is longer then 93 characters'
				exit
			}
			else {
				$result = btstask AddResource -ApplicationName:$Using:app -Type:BizTalkAssembly -Overwrite -source:$dll -Options:GacOnAdd | Out-String
				if ($result.Contains('Error: ')) {
					Write-Host $result
					Write-Host 'Failed to deploy:' $dll
					exit
				}
			}
		}
	}
	if ($session.State -ne "Opened") { exit }
}

function deploy_gac_dlls([string[]] $dlls) {
	# Transfer dlls to server
	foreach ($dll in $dlls) {
		Copy-Item -Path $dll -ToSession $session $eServicesBin
	}

	# Deploy dlls to GAC
	Invoke-Command -Session $session -ScriptBlock {
		foreach ($dll in $Using:dlls) {
			if (-not ($dll -like "*.dll")) {
				Write-Host "Not a DLL:" $dll
				exit
			}
			elseif (-not (Test-Path $dll)) {
				Write-Host "DLL does not exist:" $dll
				exit
			}
			elseif ($dll.length -gt 93) {
				Write-Host 'Failed to deploy: DLL filename is longer then 93 characters'
				exit
			}
			else {
				$result = gacutil -i $dll | Out-String
				if ($result.Contains('Failure adding assembly to the cache:')) {
					Write-Host $result
					Write-Host 'Failed to deploy:' $dll
					exit
				}
			}
		}
	}
	if ($session.State -ne "Opened") { exit }
}

function remove_bt_dlls([string] $app, [string[]] $dlls) {
	Invoke-Command -Session $session -ScriptBlock {
		foreach ($dll in $Using:dlls) {
			$luid = $dll.subString(0, $dll.length-4) + ", Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350"
			$result = btstask RemoveResource -ApplicationName:$Using:app -Luid:$luid | Out-String
			# silently handle when the dll is not already deployed
			if ($result.Contains('Error: ') -and !$result.Contains('The resource could not be found in the application')) {
				Write-Host $result
				Write-Host 'Failed to remove:' $dll
				exit
			}
		}
	}
	if ($session.State -ne "Opened") { exit }
}

function deploy_bindings([string] $bindings_name) {
	# Transfer bindings to server
	Copy-Item -Path $scripts\$bindings_name -ToSession $session $eServicesBin

	Invoke-Command  -Session $session -ScriptBlock {
		Write-Host 'Unenlisting all orchestrations'
		# This is a little obscure so here are some docs:
		# *   https://msdn.microsoft.com/en-us/library/aa561991.aspx
		# *   https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/get-wmiobject?view=powershell-5.1
		# OrchestrationStatus 4 is Start
		# OrchestrationStatus 3 is Stop
		# OrchestrationStatus 2 is Unenlisted (Unbound)
		# OrchestrationStatus 1 is Unenlisted

		# TODO: Might be able to use MSBTS_Orchestration or MSBTS_ReceiveLocationOrchestration to filter out ports belonging to orchestratons
		$stop_first_orchestrations = @(
			'CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Send.eHub2MHAccess',
			'CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Receive.MHAccess2eHub'
		)
		get-wmiobject MSBTS_Orchestration ` -namespace 'root\MicrosoftBizTalkServer' ` | Where-Object { $stop_first_orchestrations.contains($_.Name) -and $_.OrchestrationStatus -eq 4 } | Foreach-Object Stop | Out-Null
		get-wmiobject MSBTS_Orchestration ` -namespace 'root\MicrosoftBizTalkServer' ` | Where-Object { $_.OrchestrationStatus -eq 4 } | Foreach-Object Stop | Out-Null
		get-wmiobject MSBTS_Orchestration ` -namespace 'root\MicrosoftBizTalkServer' ` | Where-Object { $_.OrchestrationStatus -eq 3 } | Foreach-Object Unenlist | Out-Null

		# deploy bindings
		$result = btstask ImportBindings -GroupLevel -Source:$Using:eServicesBin\$Using:bindings_name | Out-String
		if ($result.Contains('Error: ')) {
			Write-Host $result
			Write-Host 'Failed to deploy bindings:' $bindings_name
			exit
		}
	}
	if ($session.State -ne "Opened") { exit }
}

Invoke-Command -Session $session -ScriptBlock {
	# constant used in some deploy functions
	$put_options = New-Object System.Management.PutOptions
	$put_options.Type = [System.Management.PutType]::CreateOnly

	# This is done with the C# class ManagementClass + ManagementObject
	# https://msdn.microsoft.com/en-us/library/system.management.managementclass(v=vs.110).aspx
	function deploy_host([string] $name) {
		# Delete existing
		get-wmiobject 'MSBTS_ReceiveHandler' -namespace 'root\MicrosoftBizTalkServer' | Where-Object { $_.HostName -eq $name } | Foreach-Object Delete | Out-Null
		get-wmiobject 'MSBTS_SendHandler2' -namespace 'root\MicrosoftBizTalkServer' | Where-Object { $_.HostName -eq $name } | Foreach-Object Delete | Out-Null
		get-wmiobject 'MSBTS_Host' -namespace 'root\MicrosoftBizTalkServer' | Where-Object { $_.Name -eq $name } | Foreach-Object Delete | Out-Null

		# Create Host
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_HostSetting", $null)
		$new_host = $management_class.CreateInstance()
		$new_host["Name"] = $name
		$new_host["HostType"] = 1
		$new_host["NTGroupName"] = "BizTalk Application Users"
		$new_host["AuthTrusted"] = $FALSE
		$new_host.Put($put_options) | Out-Null

		# Create Server Host (I think the biztalk GUI abstracts over this)
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_ServerHost", $null)
		$new_server_host = $management_class.CreateInstance()
		$new_server_host["ServerName"] = $env:computername
		$new_server_host["HostName"] = $name
		$new_server_host.InvokeMethod("Map", $null)
		
		# Create Host Instance
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_HostInstance", $null)
		$new_host_instance = $management_class.CreateInstance()
		$new_host_instance["Name"] = "Microsoft BizTalk Server $name $env:computername"

		[object[]] $args = New-Object System.Object[] 2
		$args[0] = "$server_name\DevAdmin"
		$args[1] = "3hubRock$"

		$new_host_instance.InvokeMethod("Install", $args);
	}

	function deploy_send_handler([string]$host_name, [string]$adapter_name) {
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_SendHandler2", $null)
		$new_handler = $management_class.CreateInstance()
		$new_handler["AdapterName"] = $adapter_name
		$new_handler["HostName"] = $host_name
		$new_handler["IsDefault"] = $FALSE
		$new_handler.Put($put_options) | Out-Null
	}

	function deploy_receive_handler([string]$host_name, [string]$adapter_name) {
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_ReceiveHandler", $null)
		$new_handler = $management_class.CreateInstance()
		$new_handler["AdapterName"] = $adapter_name
		$new_handler["HostName"] = $host_name
		$new_handler.Put($put_options) | Out-Null
	}

	function deploy_adapter([string]$name, [string]$clsid) {
		# uncomment this to dump AdapterSetting's to find CLSID values
		#get-wmiobject 'MSBTS_AdapterSetting' -namespace 'root\MicrosoftBizTalkServer' | Foreach-Object { $_ | Select-Object *; Write-Host "" }
		# TODO: Is there a way to lookup up COM components so I can avoid this nonsense?

		# Delete existing
		get-wmiobject 'MSBTS_AdapterSetting' -namespace 'root\MicrosoftBizTalkServer' | Where-Object { $_.Name -eq $name } | Foreach-Object Delete | Out-Null

		# Create AdapterSetting
		$management_class = New-Object System.Management.ManagementClass("root\MicrosoftBizTalkServer", "MSBTS_AdapterSetting", $null)
		$new_adapter = $management_class.CreateInstance()
		$new_adapter["Name"] = $name
		$new_adapter["MgmtCLSID"] = $clsid # I have no idea if these guids are consistent across installations.
		$new_adapter.Put($put_options) | Out-Null
	}
}
if ($session.State -ne "Opened") { exit }

if (-Not $currentdir) {
	Push-Location -Path $eServicesBin
}

Try {
	Write-Host 'Will deploy from:' $pwd

	Invoke-Command -Session $session -ScriptBlock {
		cd $Using:eServicesBin
		Write-Host "Stopping BizTalk instances"
		$bt_instances = get-wmiobject MSBTS_HostInstance -namespace 'root\MicrosoftBizTalkServer' -filter HostType=1
		$bt_instances | Foreach-Object Stop | Out-Null

		# Ensure both biztalk applications exist
		$result = btstask AddApp -ApplicationName:CargoWise.eHub.Products | Out-String
		if ($result.Contains('Error: ') -and (-not ($result.Contains("already exists. Specify a unique Application name")))) {
			Write-Host $result
			exit
		}
		$result = btstask AddApp -ApplicationName:CargoWise.eHub | Out-String
		if ($result.Contains('Error: ') -and (-not ($result.Contains("already exists. Specify a unique Application name")))) {
			Write-Host $result
			exit
		}
	}
	if ($session.State -ne "Opened") { exit }

	# Deploy an arbitrary list of dlls from the user
	deploy_bt_dlls 'CargoWise.eHub' $bt_dlls
	deploy_gac_dlls $gac_dlls

	if ($all -or $hosts) {
		Invoke-Command -Session $session -ScriptBlock {
			Write-Host 'Deploying Hosts'

			# File Adapter 
			deploy_host "ReceiveHost_FILE"
			deploy_receive_handler "ReceiveHost_FILE" "FILE"

			# FTPEx Adapter
			deploy_adapter "FTPEx" "{A164BEE6-4EEC-4A72-BFF0-221A75C18EF0}"
			deploy_host "SendHost_FTPEx"
			deploy_send_handler "SendHost_FTPEx" "FTPEx"

			deploy_host "ReceiveHost_FTPEx"
			deploy_receive_handler "ReceiveHost_FTPEx" "FTPEx"

			# NULL Adapter
			deploy_adapter "NULL" "{D6F3A77C-B641-45DC-8AA5-3EE3503F59FC}"
			deploy_host "SendHost_NULL"
			deploy_send_handler "SendHost_NULL" "NULL"

			# WCF-BasicHttp adapter
			deploy_host "SendHost_WCF"
			deploy_send_handler "SendHost_WCF" "WCF-BasicHttp"

			# WCF-SQL adapter
			deploy_adapter "WCF-SQL" "{59B35D03-6A06-4734-A249-EF561254ECF7}"

			deploy_host "AlertSend"
			deploy_receive_handler "AlertSend" "WCF-SQL"

			deploy_host "DISABLED"
			deploy_receive_handler "DISABLED" "WCF-SQL"

			deploy_host "ErrorReceive"
			deploy_send_handler "ErrorReceive" "WCF-SQL"

			deploy_host "InboxReceive"
			deploy_receive_handler "InboxReceive" "WCF-SQL"

			deploy_host "InboxReceive_APO"
			deploy_receive_handler "InboxReceive_APO" "WCF-SQL"

			deploy_host "InboxReceive_APO_EPR"
			deploy_receive_handler "InboxReceive_APO_EPR" "WCF-SQL"

			deploy_host "InboxReceive_SYS"
			deploy_receive_handler "InboxReceive_SYS" "WCF-SQL"

			deploy_host "InboxSend"
			deploy_send_handler "InboxSend" "WCF-SQL"

			deploy_host "OutboxReceive"
			deploy_send_handler "OutboxReceive" "WCF-SQL"
			deploy_receive_handler "OutboxReceive" "WCF-SQL"

			deploy_host "OutboxSend"
			deploy_send_handler "OutboxSend" "WCF-SQL"

			deploy_host "OutboxSend_APO"
			deploy_send_handler "OutboxSend_APO" "WCF-SQL"

			deploy_host "OutboxSend_APO_EPR"
			deploy_send_handler "OutboxSend_APO_EPR" "WCF-SQL"

			deploy_host "OutboxSend_SYS"
			deploy_send_handler "OutboxSend_SYS" "WCF-SQL"

			deploy_host "ReceiveHost_WCFSQL"
			deploy_receive_handler "ReceiveHost_WCFSQL" "WCF-SQL"

			deploy_host "SendHost_WCFSQL"
			deploy_send_handler "SendHost_WCFSQL" "WCF-SQL"

			# WCF-WebHttp adapter
			deploy_host "SendHost_WCFTls12"
			deploy_send_handler "SendHost_WCFTls12" "WCF-WebHttp"

			# Loopback adapter
			deploy_adapter "Loopback" "{EF707BC2-02A7-4D2E-B4BA-A204D813A295}"

			#deploy_adapter "MQSC" "{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}"

			# products
			deploy_host "ProductHost"
			deploy_send_handler "ProductHost" "WCF-SQL"
			deploy_receive_handler "ProductHost" "WCF-Custom"

			deploy_host "AUCustomsSendHost"
			#deploy_send_handler "AUCustomsSendHost" "MQSC"

			deploy_host "AUCustomsReceiveHost"
			#deploy_receive_handler "AUCustomsReceiveHost" "MQSC"

			deploy_host "NZCustoms"
			deploy_send_handler "NZCustoms" "WCF-Custom"
			deploy_receive_handler "NZCustoms" "WCF-Custom"
		}
		if ($session.State -ne "Opened") { exit }
	}

	if ($bindings) {
		Write-Host 'Deploying Bindings'

		deploy_bindings "local_bindings.xml"
	}

	if ($all -or $start) {
		Invoke-Command -Session $session -ScriptBlock {
			Write-Host "Starting BizTalk instances"

			# TODO: Only start needed instances. So, what is needed?
			$bt_instances | Foreach-Object Start | Out-Null
		}
		if ($session.State -ne "Opened") { exit }
	}
}
Finally {
	# powershell doesnt reset current directory on exit, so we handle that manually
	Pop-Location
}