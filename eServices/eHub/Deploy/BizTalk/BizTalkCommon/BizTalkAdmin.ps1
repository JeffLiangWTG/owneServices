# BizTalk Administration PowerShell Module
# http://powershell.codeplex.com

$BizTalkServerWmiNamespace = "root\MicrosoftBizTalkServer"
$MSBTS = @{
  "AdapterSetting" = "MSBTS_AdapterSetting"
  "GroupSetting" = "MSBTS_GroupSetting"
  "Host" = "MSBTS_Host"
  "HostInstance" = "MSBTS_HostInstance"
  "HostInstanceSetting" = "MSBTS_HostInstanceSetting"
  "HostQueue" = "MSBTS_HostQueue"
  "HostSetting" = "MSBTS_HostSetting"
  "MessageInstance" = "MSBTS_MessageInstance"
  "MessageInstanceSuspendedEvent" = "MSBTS_MessageInstanceSuspendedEvent"
  "MsgBoxSetting" = "MSBTS_MsgBoxSetting"
  "Orchestration" = "MSBTS_Orchestration"
  "ReceiveHandler" = "MSBTS_ReceiveHandler"
  "ReceiveLocation" = "MSBTS_ReceiveLocation"
  "ReceiveLocationOrchestration" = "MSBTS_ReceiveLocationOrchestration"
  "ReceivePort" = "MSBTS_ReceivePort"
  "SendHandler" = "MSBTS_SendHandler"
  "SendHandler2" = "MSBTS_SendHandler2"
  "SendPort" = "MSBTS_SendPort"
  "SendPortGroup" = "MSBTS_SendPortGroup"
  "SendPortGroup2SendPort" = "MSBTS_SendPortGroup2SendPort"
  "Server" = "MSBTS_Server"
  "ServerHost" = "MSBTS_ServerHost"
  "ServerSetting" = "MSBTS_ServerSetting"
  "Service" = "MSBTS_Service"
  "ServiceInstance" = "MSBTS_ServiceInstance"
  "ServiceInstanceSuspendedEvent" = "MSBTS_ServiceInstanceSuspendedEvent"
  "Setting" = "MSBTS_Setting"
  "TrackedMessageInstance" = "MSBTS_TrackedMessageInstance"
  "TrackedMessageInstance2" = "MSBTS_TrackedMessageInstance2"
}

$OrchestrationStatus = @{
  "Unbound" = 1
  "Bound" = 2
  "Stopped" = 3
  "Started" = 4
}

$SendPortStatus = @{
  "Bound" = 1
  "Stopped" = 2
  "Started" = 3
}

$ServiceInstanceStatus = @{
  "ReadyToRun" = 1
  "Active" = 2
  "SuspendedResumable" = 4
  "Dehydrated" = 8
  "CompletedWithDiscardedMessages" = 16
  "SuspendedNotResumable" = 32
  "InBreakpoint" = 64
}

Function To-ArrayOfHashtable {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$true,ValueFromPipeline=$true)]
    [Object[]]
    $inputArtifacts )

  Process {
    $inputArtifacts |
      Where-Object { $_ -ne $null } |
      ForEach-Object {
        $obj = $_
        $artifact = @{}

        $obj | Get-Member -MemberType Properties | ForEach-Object {
          $key = $_.Name
          if (-not $key.StartsWith("__")) {
            if ($null -ne $obj."$key") {
              $artifact."$key" = $obj."$key"
            }
          }
        } | Out-Null

        $artifact
      }
  }
}

# BizTalk Generic WMI Call
Function Invoke-BizTalkGenericWmiCall {
  param (
    [string] $Artifact,
    [string] $ArtifactChild,
    [string] $Action )

  process {
    if ( $Artifact -eq "HostInstance" ) {
      $WMIQuery = Get-WmiObject -Class "MSBTS_$Artifact" -Namespace $BizTalkServerWmiNamespace -Filter "HostName='$ArtifactChild'"
    } else {
      $WMIQuery = Get-WmiObject -Class "MSBTS_$Artifact" -Namespace $BizTalkServerWmiNamespace -Filter "Name='$ArtifactChild'"
    }

    if ( $WMIQuery -eq $null ) {
      Write-Output "ERROR : Unknown $Artifact [ $ArtifactChild ] !"
    } else {
      [string] $ObjectState = $WMIQuery.State

      switch ( $Artifact ) {
        'Orchestration' { $StopState = "Stopped|Bound" ; $StartState = "Started" ; }
        'SendPortGroup' { $StopState = "Stopped|Bound" ; $StartState = "Started" ; }
        'SendPort' { $StopState = "Stopped|Bound" ; $StartState = "Started" ; }
        'ReceiveLocation' { $StopState = "Disabled" ; $StartState = "Enabled" ; }
        'HostInstance' { $StopState = "Stopped" ; $StartState = "Running" ; }
      }

      switch ( $Action ) {
        'Start' { $ExpectedState = $StopState ; $WMICommand = '$WMIQuery.Start()' ; }
        'Stop' { $ExpectedState = $StartState ; $WMICommand = '$WMIQuery.Stop()' ; }
        'Enable' { $ExpectedState = $StopState ; $WMICommand = '$WMIQuery.Enable()' ; }
        'Disable' { $ExpectedState = $StartState ; $WMICommand = '$WMIQuery.Disable()' ; }
        'Enlist' { $ExpectedState = "Bound" ; $WMICommand = '$WMIQuery.Enlist()' ; }
        'Unenlist' { $ExpectedState = "$StopState|$StartState" ; $WMICommand = '$WMIQuery.Unenlist()' ; }
      }

      if ( $ObjectState -match $ExpectedState ) {
        $WMICommit = Invoke-Expression -Command $WMICommand

        if ( ( $WMICommit ) -and ( $? -eq $true ) ) {
          Write-Output "SUCCESS : $Action of $Artifact [ $ArtifactChild ]"
        } else {
          Write-Output "ERROR : Unable to $Action $Artifact [ $ArtifactChild ] !"
        }
      } else {
        Write-Output "$Artifact [ $ArtifactChild ] is already at the expected state of ${WMIQuery}.State"
      }
    }
  }
}

# BizTalk Catalog Explorer Assembly
Function Invoke-BizTalkEOM {
  $BizTalkConnectionString = "SERVER=.;DATABASE=BizTalkMgmtDb;Integrated Security=SSPI"
  if ( ( Test-Path "HKLM:SOFTWARE\Microsoft\Biztalk Server\3.0\Administration" ) -eq $true ) {
      $BizTalkMgmtDBServer = ( Get-ItemProperty "HKLM:SOFTWARE\Microsoft\Biztalk Server\3.0\Administration" ).MgmtDBServer
      $BizTalkMgmtDBName = ( Get-ItemProperty "HKLM:SOFTWARE\Microsoft\Biztalk Server\3.0\Administration" ).MgmtDBName
      $BizTalkConnectionString = "SERVER=$BizTalkMgmtDBServer;DATABASE=$BizTalkMgmtDBName;Integrated Security=SSPI"
  }

  $BizTalkCatalogExplorer = New-Object Microsoft.BizTalk.ExplorerOM.BtsCatalogExplorer
  $BizTalkCatalogExplorer.ConnectionString = $BizTalkConnectionString
  $BizTalkCatalogExplorer
}

# Applications
Function Get-Application {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$true,ValueFromPipeline=$true)]
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    $BizTalkCatalogExplorer = Invoke-BizTalkEOM
    $Application = $BizTalkCatalogExplorer.Applications  | Where-Object { $_.Item -match "$Name" }
    $Application
  }
}

Function Start-Application {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    $BizTalkCatalogExplorer = Invoke-BizTalkEOM
    $BizTalkApplication = $BizTalkCatalogExplorer.Applications[$Name]

    if ( $BizTalkApplication.State -ne "Started" ) {
      $BizTalkApplication.Start([Microsoft.BizTalk.ExplorerOM.ApplicationStartOption] "StartAll")
      $BizTalkCatalogExplorer.SaveChanges()

      if ( ( $BizTalkApplication ) -and ( $? -eq $true ) ) {
        Write-Output "SUCCESS : Start of Application [ $Name ]"
      } else {
        Write-Output "ERROR : Unable to Start Application [ $Name ] !"
      }
    } else {
      Write-Output "Application [ $Name ] is already Started"
    }
  }
}

Function Stop-Application {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    $BizTalkCatalogExplorer = Invoke-BizTalkEOM
    $BizTalkApplication = $BizTalkCatalogExplorer.Applications[$Name]

    if ( $BizTalkApplication.State -ne "Stopped" ) {
      $BizTalkApplication.Stop([Microsoft.BizTalk.ExplorerOM.ApplicationStopOption] ("DisableAllReceiveLocations","UnenlistAllOrchestrations","UnenlistAllSendPortGroups","UnenlistAllSendPorts","UndeployAllPolicies"))
      $BizTalkCatalogExplorer.SaveChanges()

      if ( ( $BizTalkApplication ) -and ( $? -eq $true ) ) {
        Write-Output "SUCCESS : Stop of Application [ $Name ]"
      } else {
        Write-Output "ERROR : Unable to Stop Application [ $Name ] !"
      }
    }
    else {
      Write-Output "Application [ $Name ] is already Stopped"
    }
  }
}
#

# Orchestrations
Function Get-Orchestrations-Match-Assembly {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [string] $AssemblyName,
    [ValidateNotNullOrEmpty()]
    [string] $AssemblyVersion)

  process {
    $Orchestrations = Get-WmiObject -Class ${MSBTS}.Orchestration -Namespace $BizTalkServerWmiNamespace -Filter "AssemblyName='${AssemblyName}' and AssemblyVersion='${AssemblyVersion}'" | ForEach-Object { $_.Name }
  }
}

Function Get-Orchestration {
  [CmdletBinding()]
  param (
    [parameter(Mandatory=$true,ValueFromPipeline=$true)]
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.Orchestration -Namespace $BizTalkServerWmiNamespace | Where-Object { $_.Item -match "$Name" }
  }
}

Function Get-Orchestrations {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string] $Status )

  process {
    $allOrchestrations = Get-WmiObject -Class ${MSBTS}.Orchestration -Namespace $BizTalkServerWmiNamespace
    $selectedArtifacts = @()

    if ([string]::IsNullOrEmpty($Status)) {
      $selectedArtifacts = $allOrchestrations

    } elseif ($OrchestrationStatus.Keys -contains $Status) {
      $selectedArtifacts = $allOrchestrations | Where-Object { $_.OrchestrationStatus -eq $OrchestrationStatus[$Status]}

    } else {
      throw "Unknown orchestration status key: $Status"
    }

    return $selectedArtifacts
  }
}

Function Start-Orchestration {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Start' -Artifact 'Orchestration' -ArtifactChild $Name
  }
}

Function Stop-Orchestration {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Stop' -Artifact 'Orchestration' -ArtifactChild $Name
  }
}

Function Enlist-Orchestration {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Enlist' -Artifact 'Orchestration' -ArtifactChild $Name
  }
}

Function Unenlist-Orchestration {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Unenlist' -Artifact 'Orchestration' -ArtifactChild $Name
  }
}

Function Unenlist-Orchestrations-Match-Assembly {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $AssemblyName,
    [ValidateNotNullOrEmpty()]
    [Parameter(Mandatory=$true)]
    [string] $AssemblyVersion)

  process {
    $UnenlistedOrchestrations = @()

    $Orchestrations = Get-WmiObject -Class ${MSBTS}.Orchestration -Namespace $BizTalkServerWmiNamespace

    # No operation is allowed when the BizTalk orchestration is in the unbound state
    # The only allowed operation is to enlist this orchestration when BizTalk orchestration is in the bound state
    # Unbound: 1; Bound: 2; Stopped: 3; Started: 4
    $Orchestrations |
      Where-Object { ($_.AssemblyName -eq ${AssemblyName}) -and ($_.AssemblyVersion -eq $AssemblyVersion) } |
      ForEach-Object {
        $status = [string] $_.OrchestrationStatus
        $actionTaken = $false

        if ( $status -eq $OrchestrationStatus.Started ) {
          try {
            $_.Stop()
            $_.Unenlist()
            $actionTaken = $true
          } catch { }
        } elseif ( $status -eq $OrchestrationStatus.Stopped ) {
          try {
            $_.Unenlist()
            $actionTaken = $true
          } catch { }
        }

        if ($actionTaken) {
          $UnenlistedOrchestrations += New-Object PSCustomObject -Property @{
            Name = $_.Name
            OriginalStatus = $status
          }
        }
      } | Out-Null

    $UnenlistedOrchestrations
  }
}

Function Resume-Orchestrations-Match-Assembly {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(Mandatory=$true)]
    [string] $AssemblyName,
    [ValidateNotNullOrEmpty()]
    [Parameter(Mandatory=$true)]
    [string] $AssemblyVersion,
    [ValidateNotNull()]
    [Parameter(Mandatory=$true)]
    [object[]]$UnenlistedOrchestrations)

  process {
    $Orchestrations = Get-WmiObject -Class ${MSBTS}.Orchestration -Namespace $BizTalkServerWmiNamespace -Filter "AssemblyName='${AssemblyName}' and AssemblyVersion='${AssemblyVersion}'"
    $Orchestrations | ForEach-Object {

      $Orchestration = $_
      $Status = [string] $Orchestration.OrchestrationStatus
      $Name = $Orchestration.Name

      $Unenlisted = $UnenlistedOrchestrations | Where-Object { $_.Name -eq $Name }

      if (($null -ne $Unenlisted) -and ($Status -ne $Unenlisted.OriginalStatus)) {
        Switch ($Unenlisted.OriginalStatus) {
          "3" { if ($Status -eq "2") { $Orchestration.Enlist() } }
          "4" {
              if ($Status -eq "2") {
                $Orchestration.Enlist()
                Start-Sleep -m 500
              }

            $Orchestration.Start()
          }
        }
      }
    } | Out-Null
  }
}

Function Stop-Started-Orchestrations {
  process {
    $startedOrchestrations = Get-Orchestrations "Started"
    $startedOrchestrations | ForEach-Object { $_.Stop() } | Out-Null
    $startedOrchestrations
  }
}

Function Start-Orchestrations {
  [CmdletBinding()]
  param (
    [Parameter(ValueFromPipeline=$true)]
    [string[]]$names)

  process {
    $allOrchestrations = Get-Orchestrations
    if ( ($null -ne $names) -and ($names.Count -gt 0) ) {
      $selectedOrchestrations = $allOrchestrations | Where-Object { $names -Contains $_.Name }
    } else {
      $selectedOrchestrations = $allOrchestrations
    }

    [string[]]$failedOrchestrations = @()
    $selectedOrchestrations | ForEach-Object {

      try{
        # Unbound: 1; Bound: 2; Stopped: 3; Started: 4
        if ($_.OrchestrationStatus -eq $OrchestrationStatus.Stopped) {
          $_.Start()

        } elseif ($_.OrchestrationStatus -eq $OrchestrationStatus.Bound) {
          $_.Enlist()
          $_.Start()
        }
      } catch {
        $failedOrchestrations += $_.Name
      }
    } | Out-Null

    $selectedOrchestrations
  }
}
#

# Send Port Groups
Function Get-SendPortGroup {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.SendPortGroup -Namespace $BizTalkServerWmiNamespace | Where-Object { $_.Item -match "$Name" }
  }
}

Function Start-SendPortGroup {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Start' -Artifact 'SendPortGroup' -ArtifactChild $Name
  }
}

Function Stop-SendPortGroup {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Stop' -Artifact 'SendPortGroup' -ArtifactChild $Name
  }
}

Function Enlist-SendPortGroup {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Enlist' -Artifact 'SendPortGroup' -ArtifactChild $Name
  }
}

Function Unenlist-SendPortGroup {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Unenlist' -Artifact 'SendPortGroup' -ArtifactChild $Name
  }
}
#

# Send Ports
Function Get-SendPort {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.SendPort -Namespace $BizTalkServerWmiNamespace | Where-Object { $_.Item -match "$Name" }
  }
}

Function Start-SendPort {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Start' -Artifact 'SendPort' -ArtifactChild $Name
  }
}

Function Stop-SendPort {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Stop' -Artifact 'SendPort' -ArtifactChild $Name
  }
}

Function Enlist-SendPort {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Enlist' -Artifact 'SendPort' -ArtifactChild $Name
  }
}

Function Unenlist-SendPort {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Unenlist' -Artifact 'SendPort' -ArtifactChild $Name
  }
}

Function Get-SendPorts {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string] $Status )

  process {
    $artifacts = Get-WmiObject -Class ${MSBTS}.SendPort -Namespace $BizTalkServerWmiNamespace

    if ([string]::IsNullOrEmpty($Status)) {
      $artifacts
    } elseif ($SendPortStatus.Keys -contains $Status) {
      $artifacts | Where-Object { $_.Status -eq $SendPortStatus[$Status] }
    } else {
      throw "Unknown send port status key: $Status"
    }
  }
}

Function Stop-Started-SendPorts {
  process {
    $startedSendPorts = Get-SendPorts "Started"
    $startedSendPorts | ForEach-Object { $_.Stop() } | Out-Null
    $startedSendPorts
  }
}

Function Start-SendPorts {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string[]]$names)

  process {
    $allSendPorts = Get-SendPorts
    if ( ($null -ne $names) -and ($names.Count -gt 0) ) {
      $selectedSendPorts = $allSendPorts | Where-Object { $names -Contains $_.Name }
    } else {
      $selectedSendPorts = $allSendPorts
    }

    $selectedSendPorts | ForEach-Object {

      # Bound: 1; Stopped: 2; Started: 3
      if ($_.Status -eq $SendPortStatus.Stopped) {
        $_.Start()

      } elseif ($_.Status -eq $SendPortStatus.Bound) {
        $_.Enlist()
        $_.Start()
      }
    } | Out-Null

    $selectedSendPorts
  }
}
#

# Receive Locations
Function Get-ReceiveLocations {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string[]] $Names )

  process {
    $receiveLocations = Get-WmiObject -Class ${MSBTS}.ReceiveLocation -Namespace $BizTalkServerWmiNamespace
    if ( ($null -ne $Names) -and ($Names.Count -gt 0) ) {
      $receiveLocations | Where-Object { $Names -contains $_.Name }
    } else {
      $receiveLocations
    }
  }
}

Function Get-ReceiveLocation {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.ReceiveLocation -Namespace $BizTalkServerWmiNamespace | Where-Object { $_.Name -match "$Name" }
  }
}

Function Enable-ReceiveLocation {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Enable' -Artifact 'ReceiveLocation' -ArtifactChild $Name
  }
}

Function Disable-ReceiveLocation {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,ValueFromPipeline=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Disable' -Artifact 'ReceiveLocation' -ArtifactChild $Name
  }
}
#

# Hosts
Function Get-Hosts {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string[]] $HostNames )

  process {
    $hosts = Get-WmiObject -Class ${MSBTS}.Host -Namespace $BizTalkServerWmiNamespace

    if ( ($null -ne $HostNames) -and ($HostNames.Count -gt 0) ) {
      $hosts | Where-Object { $HostNames -contains $_.Name }
    } else {
      $hosts
    }
  }
}
#

# Host Instances
Function Get-HostInstances {
  [CmdletBinding()]
  param (
    [parameter(ValueFromPipeline=$true)]
    [string[]] $HostNames )

  process {
    $hostInstances = Get-WmiObject -Class ${MSBTS}.HostInstance -Namespace $BizTalkServerWmiNamespace

    if ( ($null -ne $HostNames) -and ($HostNames.Count -gt 0) ) {
      $hostInstances | Where-Object { $HostNames -contains $_.HostName }
    } else {
      $hostInstances
    }
  }
}

Function Get-HostInstance {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.HostInstance -Namespace $BizTalkServerWmiNamespace -Filter "HostType='1'" | Where-Object { $_.HostName -match "$Name" }
  }
}

Function Start-HostInstance {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Start' -Artifact 'HostInstance' -ArtifactChild $Name
  }
}

Function Stop-HostInstance {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    [string] $Name )

  process {
    Invoke-BizTalkGenericWmiCall -Action 'Stop' -Artifact 'HostInstance' -ArtifactChild $Name
  }
}
#

# Service Instances
Function Get-ServiceInstance {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [string] $Name )

  process {
    Get-WmiObject -Class ${MSBTS}.ServiceInstance -Namespace $BizTalkServerWmiNamespace | Where-Object { $_.Item -match "$Name" -and $_.Item -ne "" }
  }
}

Function Remove-ServiceInstance {
  [CmdletBinding()]
  param (
    [ValidateNotNullOrEmpty()]
    [Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]
    $InstanceID )

  process {
    $WMIQuery = Get-WmiObject -Class ${MSBTS}.ServiceInstance -Namespace $BizTalkServerWmiNamespace -Filter "InstanceID='$InstanceID'"
    $WMICommit = $WMIQuery.Terminate()
    if ( ( $WMICommit ) -and ( $? -eq $true ) )
    {
      Write-Output "SUCCESS : Remove of ServiceInstance [ $InstanceID ]"
    }
    else
    {
      Write-Output "ERROR : Unable to Remove ServiceInstance [ $InstanceID ] !"
    }
  }
}
#