$gacUtil = "${Env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\gacutil.exe"
$btsTask = "${Env:ProgramFiles(x86)}\Microsoft BizTalk Server 2013 R2\BTSTask.exe"

Add-Type -AssemblyName "Microsoft.Build.Framework"

function Deploy {
  param(
    [string]$ZipFilePath,
    [string]$InstallDir,
    [string]$MachineName,
    [string]$UserId,
    [string]$UserPassword,
    [string]$Profile,
    [string]$PreDeployScript,
    [string]$PostDeployScript
  )

  $ErrorActionPreference = "Stop"
  $result = $false

  if (-not (Test-Path $ZipFilePath)) {
      throw "Package zip file not found: $ZipFilePath"
  }

  try {
      if ($null -ne $error) { $error.Clear() }

      $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "Powershell deploying $ZipFilePath to $MachineName...")
      $result = (DeployRemote -ZipFilePath $ZipFilePath -InstallDir $InstallDir -MachineName $MachineName -UserId $UserId -UserPassword $UserPassword -Profile $Profile -PreDeployScript $PreDeployScript -PostDeployScript $PostDeployScript)

  } catch {
      if (($null -ne $error) -and ($error.Count -gt 0)) {
          throw $error
      }
  }

  return $result
}

function DeployRemote {
  param(
    [string]$ZipFilePath,
    [string]$InstallDir,
    [string]$MachineName,
    [string]$UserId,
    [string]$UserPassword,
    [string]$Profile,
    [string]$PreDeployScript,
    [string]$PostDeployScript
  )

  $result = $false
  $Session = (Connect -MachineName $MachineName -UserId $UserId -UserPassword $UserPassword)

  if ($null -eq $Session) {
      throw "No session established to target machine $MachineName for user $UserId"
  }

  try
  {
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "Established a session to remote machine $MachineName")

    if ($null -ne $error) { $error.Clear() }

    $zipFileName = (Split-Path $ZipFilePath -leaf)
    $zipFileDir = (Split-Path $ZipFilePath -parent)

    [string] $localInstallLogDir = Invoke-Command -ScriptBlock $createTimeStampedTempPath
    [string] $remotePackageDir = Invoke-Command -Session $Session -ScriptBlock $createTimeStampedTempPath
    [string] $destFilePath = Join-Path $remotePackageDir $zipFileName

    Copy-Item -Path $ZipFilePath -Destination $remotePackageDir -ToSession $Session

    $destDir = Invoke-Command -Session $Session -ScriptBlock $extractZipFile -ArgumentList "$destFilePath", "$InstallDir"
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "Extracted $ZipFilePath to $destDir")

    $installLogFile = "install.log"
    $logFilePath = Join-Path $destDir $installLogFile
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "Installing on the target machine $MachineName using credentials from $UserId.")
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "Please wait for the process to finish...")

    # PreDeployScript
    if (($null -ne $PreDeployScript) -and (-not [string]::IsNullOrEmpty($PreDeployScript))) {
      $sb = [Scriptblock]::Create($PreDeployScript)
      $result = Invoke-Command -Session $Session -ScriptBlock $sb -ArgumentList $logFilePath
      $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $result)
    }

    # Deployment main
    $securePw = ConvertTo-SecureString -AsPlainText -Force -String $UserPassword
    $UserCred = New-Object -TypeName System.Management.Automation.PSCredential($UserId, $securePw)

    $result = Invoke-Command -Session $Session -ScriptBlock $localInstallPowershellScripts -ArgumentList $destDir, $logFilePath, $Profile
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $result)

    # PostDeployScript
    if (($null -ne $PostDeployScript) -and (-not [string]::IsNullOrEmpty($PostDeployScript))) {
      $sb = [Scriptblock]::Create($PostDeployScript)
      $result = Invoke-Command -Session $Session -ScriptBlock $sb -ArgumentList $logFilePath
      $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $result)
    }

    Copy-Item -Path $logFilePath -Destination $localInstallLogDir -FromSession $Session

    $err = @()
    $localInstallLogFilePath = Join-Path $localInstallLogDir $installLogFile
    foreach($line in Get-Content "$localInstallLogFilePath") {

      if (($null -ne $line) -and (ContainsError $line)) {
        $err += $line
        $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $line)
      }
      else {
        $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", $line)
      }
    }

    if ($err.Count -gt 0) {
      $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High","Oops! Failed to install $ZipFilePath onto $MachineName...")
      throw $err

    } else {
      $result = $true
      $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High","Hooray! Successfully installed $ZipFilePath onto $MachineName!")
    }

    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "Check out remote install log from local machine at:")
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "$localInstallLogFilePath")

  } catch {
    $result = $false
    if (($null -ne $error) -and ($error.Count -gt 0)) {
      throw "Deploy failed with error(s)."
    }
  }
  finally
  {
    Remove-PsSession -Session $Session
  }

  return $result
}

function LocalInstall {
  param([string]$destDir, [string]$logFilePath, [string]$profile)

  $LogAppend = {
    param ([string] $message, [string] $logFilePath)

    (Get-Date -format '[yyyy-MM-dd HH:mm:ss]: ') + $message | Add-Content -Path $logFilePath
  }

  [string] $profileArgPs1 = [string]::Empty
  [string] $profileArgMsbuild = [string]::Empty
  if (![string]::IsNullOrEmpty($profile)) {
    $profileArgPs1 = "-Profile $profile"
    $profileArgMsbuild = "/p:Profile=$profile"
  }

  Set-Location $destDir
  "Set-Location: $destDir" | Set-Content -Path $logFilePath
  & $LogAppend -message "remote install started" -logFilePath $logFilePath

  try {
    # Execute every powershell script *.ps1 file on the target machine
    Get-ChildItem -Path *.ps1 -Exclude "Install.ps1" |  Sort-Object -Property Name |
      ForEach-Object {
        try {
          $psCmd = 'powershell.exe -ExecutionPolicy RemoteSigned -File "{0}" {2} >> "{1}"' -f $_, $logFilePath, $profileArgPs1
          & $LogAppend -message "Executing $psCmd" -logFilePath $logFilePath

          Start-Process cmd.exe -ArgumentList('/c "' + $psCmd + '"') -Wait -PassThru -Verb RunAs

        } catch {
          if ($null -ne $error[0]) {
              $error | % { & $LogAppend -message $_ -logFilePath $logFilePath }
              throw $errormsbuild
          }
        }
      }

    # Execute every msbuild project *.proj file on the target machine
    $msbuild = "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
    if (-not (Test-Path $msbuild)) {
      $msbuild = "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\msbuild.exe"
    }

    Get-ChildItem -Path *.proj |  Sort-Object -Property Name |
      ForEach-Object {
        try {
          $projectFile = $_

          $arguments = '"$projectFile" /fileLoggerParameters:LogFile="$logFilePath";Append'
          $msbuildCmd = '"{0}" "{1}" /fileLoggerParameters:LogFile="{2}";Append "{3}" /v:d' -f $msbuild, $projectFile, $logFilePath, $profileArgMsbuild
          & $LogAppend -message "Executing $msbuildCmd" -logFilePath $logFilePath

          Start-Process cmd.exe -ArgumentList('/c "' + $msbuildCmd + '"') -Wait -PassThru -Verb RunAs

        } catch {
          if ($null -ne $error[0]) {
            $error | % { & $LogAppend -message $_ -logFilePath $logFilePath }
            throw $error
          }
        }
      }

    & $LogAppend -message "remote install completed" -logFilePath $logFilePath
  } catch {
    & $LogAppend -message "Error occured: $_" -logFilePath $logFilePath
    return $_
  }
}
$localInstallPowershellScripts = ${function:LocalInstall}

function ContainsError {
  param ([string] $line)

  @(
    "^Build FAILED\.$",
    "^.*: error :.*$",
    "^.*: error MSB\d+:.*$",
    "^\s+[1-9]\d*\s+Error\(s\)$"
  ) | % {
    if ($line -match $_) { return $true }
  }

  return $false
}

function DeployLocalhost {
  param(
    [string]$ZipFilePath,
    [string]$InstallDir
  )

  $result = $false

  if (-not (Test-Path $ZipFilePath)) {
      throw "Package zip file not found: $ZipFilePath"
  }

  try
  {
    if ($null -ne $error) { $error.Clear() }
    $zipFileName = (Split-Path $ZipFilePath -leaf)

    [string] $PsRemotePackageDir = (& $createTimeStampedTempPath)
    [string] $destFilePath = Join-Path $PsRemotePackageDir $zipFileName

    Copy-Item -Path $ZipFilePath -Destination $PsRemotePackageDir
    & $extractZipFile -zipFilePath $destFilePath -installDir $InstallDir

    & $executePowershellScripts -packageFilesDir $PsRemotePackageDir
    $result = $true

  } catch {
    $result = $false
    if ($null -ne $error[0]) { throw $error[0] }
  }

  return $result
}

$createTimeStampedTempPath = {

  $tempDir = [System.IO.Path]::GetTempPath()
  [string]$timeStamp = [System.DateTime]::Now.ToString("yyyyMMdd_HHmmss")
  $tempPath = Join-Path $tempDir $timeStamp
  New-Item -ItemType Directory -Path $tempPath
}

$extractZipFile = {
  param($zipFilePath, $installDir)

  [string] $destDir = $installDir
  if (([string]::IsNullOrEmpty($destDir)) -or (-not [System.IO.File]::Exists($destDir))) {
    $destDir = (Get-Item $zipFilePath).Directory.FullName
  }

  Add-Type -AssemblyName System.IO.Compression.FileSystem
  [System.IO.Compression.ZipFile]::ExtractToDirectory($zipFilePath, $destDir)
  Remove-Item $zipFilePath

  return $destDir
}

function ExecuteScripts {
  param($packageFilesDir)

  Get-ChildItem -Path $packageFilesDir\* -Include *.ps1 -Exclude "Install.ps1" |  Sort-Object -Property Name |
    ForEach-Object {
      try {
        & $_.FullName $packageFilesDir
      } catch {
        if ($null -ne $error[0]) { throw $error[0] }
      }
    }
}
$executePowershellScripts = ${function:ExecuteScripts}

function DeployExtractedPackages {
  param(
    [string]$PackageFilesDir,
    [string]$MachineName,
    [string]$UserId,
    [string]$UserPassword
  )

  if (($MachineName -ne '') -and ($MachineName -notmatch "\.|localhost|$env:COMPUTERNAME")) {
    $result = (DeployExtractedPackagesRemote -ZipFilePath $ZipFilePath -MachineName $MachineName -UserId $UserId -UserPassword $UserPassword)
  } else {
    & $executePowershellScripts -packageFilesDir $PackageFilesDir
    $result = $true
  }

  return $result
}

function DeployExtractedPackagesRemote {
  param(
    [string]$PackageFilesDir,
    [string]$MachineName,
    [string]$UserId,
    [string]$UserPassword
  )

  $result = $false

  $Session = (Connect -MachineName $MachineName -UserId $UserId -UserPassword $UserPassword)

  if ($null -eq $Session) {
    throw "No session established to target machine"
  }

  try
  {
    if ($null -ne $error) { $error.Clear() }
    Invoke-Command -Session $Session -ScriptBlock $executePowershellScripts -ArgumentList $PackageFilesDir
    $result = $true

  } finally {
    Remove-PsSession -Session $Session
  }

  return $result
}

function AddMachineToTrustedHosts {
  param(
      [ValidateNotNullOrEmpty()]
      [Parameter(ValueFromPipeline=$true,Mandatory=$true)]
      [string]$Server)

  process{
    $current = (Get-Item WSMan:\localhost\Client\TrustedHosts).Value | Out-String

    if ( $current -eq $null ) {
      winrm set winrm/config/client ('@{{ TrustedHosts="{0}" }}' -f $Server ) | Out-Null
    } else {
      $hosts = $current.Trim() -split ","

      if ($hosts -notcontains $Server) {
        $hosts += $Server
        winrm set winrm/config/client ('@{{ TrustedHosts="{0}" }}' -f ($hosts -join ",") ) | Out-Null
      }
    }

    (Get-Item WSMan:\localhost\Client\TrustedHosts).Value | Out-String
  }
}

function Connect
{
  param([string]$MachineName,[string]$UserId,[string]$UserPassword)

  if ($null -ne $error) { $error.Clear() }

  if ($MachineName -eq '')
  {
    try
    {
      $Session = New-PSSession
    } finally {
      if ($null -ne $error[0]) { throw "Could not create a session on local machine: $error[0]" }
    }
  }
  else
  {
    try
    {
      $Session = New-PSSession -Computername $MachineName -ErrorAction Stop
    } catch {
	     $error.Clear()
       if ( ($null -eq $Session) -and ($UserId -ne '') ) {
        $securePw = ConvertTo-SecureString -AsPlainText -Force -String $UserPassword
        $UserCred = New-Object -TypeName System.Management.Automation.PSCredential($UserId, $securePw)

        try
        {
          $Session = New-PSSession -Computername $MachineName -Credential $UserCred -ErrorAction Stop
        } catch {
          $error.Clear()
          $sessopts = New-PSSessionOption -SkipRevocationCheck
          $Session = New-PSSession -Computername $MachineName -Credential $UserCred -UseSSL -SessionOption $sessopts
        }
      }

    } finally {
      if ($null -ne $error[0]) { throw "Could not create a session on [$MachineName]: $error[0]" }
    }
  }

  return $Session
}

function DeployAssemblyToGac {
  param([string]$assemblyFilePath)

  try {
    if ($null -ne $error) { $error.Clear() }
    $result = & $gacUtil "/nologo" "/i" $assemblyFilePath | Out-String

    if ($result.Contains('Failure adding assembly to the cache:')) { throw $result  }

  } catch {
    if ($null -ne $error[0]) { throw $error[0] }
  }
}

function DeployAssemblyToBizTalk {
  param(
    [string]$assemblyFilePath,
    [string]$applicationName,
    [string]$type,
    [string]$options)

  try {
    if ($null -ne $error) { $error.Clear() }
    $result = & $btsTask AddResource -ApplicationName:$ApplicationName -Type:$Type -Overwrite -source:$assemblyFilePath -Options:$Options | Out-String

    if ($result.Contains('Error: ')) { throw $result }

  } catch {
    if ($null -ne $error[0]) { throw $error[0] }
  }
}