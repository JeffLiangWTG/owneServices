#Requires -RunAsAdministrator
param (
   [Parameter(Mandatory=$false)][string]$StagingPath,
   [Parameter(Mandatory=$false)][string]$BackupPath,
   [Parameter(Mandatory=$false)][string]$InstallPath = "C:\eServices\BizTalkAdapters"
)
$ErrorActionPreference = 'Stop'

Set-PSDebug -Trace 2

if (-not $StagingPath) {
    $StagingPath = Resolve-Path "$PSScriptRoot\..\.."
}

if (-not $BackupPath) {
    $BackupPath = Join-Path $StagingPath 'Backup'
}

Start-Transcript -OutputDirectory $StagingPath

if (Get-WmiObject Win32_Process -Filter "Name = 'mmc.exe' and CommandLine like '%BTSmmc.msc%'") {
    throw 'Aborting deployment. All instances of ''BizTalk Server Administration Console'' for all users on the server must be stopped before continuing.'
}

[xml]$settings = Get-Content (Join-Path $PSScriptRoot "Deploy-BizTalkAdapters-Settings.xml")

if ($BackupPath) {
    ROBOCOPY $InstallPath $BackupPath /S /E /NP /V /TS /W:2 /R:60

    $settings.BizTalkAdapters.Adapter | ForEach-Object {
        $regKey = "HKCR\Wow6432Node\CLSID\{$($_.RegKey)}"
        $regBackupFile = Join-Path $BackupPath ($_.Name + ".Backup.reg")
        Write-Host "REG EXPORT $regKey $regBackupFile"
        REG EXPORT $regKey $regBackupFile
    }
}

try
{
	(Get-Service BTSSvc* | % { Start-Job { Stop-Service $input -Force -Verbose -ErrorAction Ignore } -InputObject $_.Name }) | Wait-Job | Receive-Job

    $binPath = Join-Path $InstallPath "bin"
    $setupPath = Join-Path $InstallPath "setup"

    ROBOCOPY $StagingPath $binPath /MIR /NP /V /TS /W:2 /R:60
    ROBOCOPY $PSScriptRoot $setupPath /MIR /NP /V /TS /W:2 /R:60

    $settings.BizTalkAdapters.Adapter | ForEach-Object {
        $regDeployFile = Join-Path $PSScriptRoot ($_.Name + ".reg")
        ($process = Start-Process REG -ArgumentList "IMPORT $regDeployFile" -PassThru).WaitForExit()
        if ($process.ExitCode -ne 0) { throw "REG exited with code {0}." -f $proc.ExitCode }
    }
}
finally
{
    Start-Service BTSSvc* -Verbose
}
