param (
    [ValidateSet("Up", "Down", "List", "Kill", "BlazorUp", "BlazorDown")]
    [parameter(Mandatory=$false,Position=0)][string]$Command = $null,
    [parameter(Mandatory=$false)][string]$AppServerRepository = $null, # falls back to $PSScriptRoot\..\ 
    [parameter(Mandatory=$false)][string]$ClientAppRepository = $null, # defaults to $Repository\..\Blazor.Client\. Used to identify ClientApp instances.
    [parameter(Mandatory=$false)][string]$CargoWiseRepository = $null # defaults to $Repository (to use 'local' cargowise restored into Blazor bin folder). Set to dev repo path to use cargowise exe direct from Dev repo.
)

Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"
$ProgressPreference="SilentlyContinue"

. $PSScriptRoot\..\include.ps1

$scriptName = $MyInvocation.ScriptName
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Start-Command {
    param([parameter()][string]$Value = $null)
    switch ($Value.ToLower()) {
        "up" {
            $convertedFormsRelativePath = "bin\AppServer\convertedForms.csv"
            $expectedConvertedFormsPath = Join-Path $cwDevRepo $convertedFormsRelativePath
            if (-not (test-path $expectedConvertedFormsPath -PathType Leaf)) {
                Write-Log "convertedForms.csv missing from $cwDevRepo, copying from $repoPath"
                mkdir $(split-path $expectedConvertedFormsPath -Parent) > $null
                cp -Path $(Join-Path $repoPath $convertedFormsRelativePath) -Destination $expectedConvertedFormsPath -Verbose
            }
            Start-App -Path $(Join-Path -Path $cwDevRepo -ChildPath "bin\CargoWiseOneAnyCpu.exe") -WaitForInputIdle
            Start-App -Path $(Join-Path -Path $repoPath -ChildPath "bin\SessionBroker\CargoWise.Blazor.SessionBroker.exe") -ArgumentList "--environment=Development"
        }
        "down" { 
            Stop-App -Path $(Join-Path -Path $repoPath -ChildPath "bin\SessionBroker\CargoWise.Blazor.SessionBroker.exe")
        }
        "kill" {
            Get-BlazorProcesses -RootPath $repoPath | Stop-Process -Verbose
            Get-BlazorProcesses -RootPath $cwDevRepo | Stop-Process -Verbose
            Get-BlazorProcesses -RootPath $blazorClientRepo | Stop-Process -Verbose
        }
        "list" {
            Write-Log "Launched procs:"
            $startedProcs | select Id,Name,Path,StartTime,ExitTime,ExitCode | ft | Out-Host
            Write-Log "Blazor procs (repo: $($repoPath)):"
            Get-BlazorProcesses -RootPath $repoPath  | select Id,Name,Path,StartTime,MainWindowTitle | ft | Out-Host
            Write-Log "Dev procs (repo: $($cwDevRepo)):"
            Get-BlazorProcesses -RootPath $cwDevRepo  | select Id,Name,Path,StartTime,MainWindowTitle | ft | Out-Host
            Write-Log "Blazor.Client procs (repo: $blazorClientRepo):"
            Get-BlazorProcesses -RootPath  $blazorClientRepo | select Id,Name,Path,StartTime,MainWindowTitle | ft | Out-Host
        }
        default { 
            if (-not [string]::IsNullOrEmpty($Value)) {
                Write-Log "Unexpected input: $Value (expected: up, down, list, kill, Ctrl+C (exit))"
            }
        }
    }
}

function Start-App {
    param (
        [parameter(Mandatory=$true)][string]$Path,
        [parameter(Mandatory=$false)][string]$WorkingDirectory,
        [parameter(Mandatory=$false)][string[]]$ArgumentList = @(),
        [parameter(Mandatory=$false)][switch]$WaitForInputIdle
    )
    
    $exeName = Split-Path -Path $Path -Leaf
    if ([string]::IsNullOrEmpty($WorkingDirectory)) {
        $WorkingDirectory = Split-Path -Path $Path -Parent
    }

    Write-Log "[$exeName] Starting";
    #Write-Log "[$exeName] Starting (path: $Path, workingDirectory: $WorkingDirectory, waitForInputIdle: $WaitForInputIdle, args: $ArgumentList)";

    $runningProc = Get-BlazorProcesses -RootPath $Path
    if ($null -ne $runningProc) {
        Write-Log "[$exeName] Already running, skipping startup"
        return;
    }

    $proc = Start-Process -PassThru $Path -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
    $startedProcs.Add($proc)
    
    if ($null -eq $proc) {
        throw "Start-Process returned null for $Path"
    }

    Write-Log "[$exeName] Started (processId: $($proc.Id))"

    if ($WaitForInputIdle -and -not $proc.HasExited) {
        Write-Log "[$exeName] Waiting for InputIdle"
        $proc.WaitForInputIdle() > $null
        Write-Log "[$exeName] InputIdle received"
    }

    Write-Log "[$exeName] Post-start wait"
    Start-Sleep -Seconds 2
    if ($proc.HasExited) {
        throw "$exeName exited with $($proc.ExitCode) after $($proc.ExitTime - $proc.StartTime)"
    }
    Write-Log "[$exeName] Startup complete"
}

function Stop-App {
    param ([parameter(Mandatory=$true)]$Path)
    $items = Get-BlazorProcesses -RootPath $Path
    if ($null -ne $items) {
        Write-Log "Shutting down procs:"
        $items | select Id,Name,Path,CommandLine,MainWindowTitle,StartTime,ExitTime,ExitCode | ft | Out-Host
        $items | ForEach-Object {
            $item = $_;
            try {
                if ($null -ne $item) {
                    Write-Log "Shutting down $($item.Id)"
                    $item.Kill();
                }
            } catch {
                Write-Error "Error killing process: $_"
            }
        }
    } else {
        Write-Log "No processes to stop at $Path"
    }
}

function Get-BlazorProcesses {
    param ([parameter(Mandatory=$true)][string]$RootPath)
    $procNames = @("CargoWise.Blazor.SessionBroker", "CargoWise.Blazor.AppServer", "CargoWise", "CargoWiseOneAnyCpu")
    return Get-Process | Where-Object { $_.Name -iin($procNames) -and $_.Path -ilike "$RootPath*" }
}

$startedProcs = [System.Collections.Generic.List[System.Object]]::new()

try {
    Write-Log "Begin $scriptName"

    $repoPath = if (-not [string]::IsNullOrEmpty($AppServerRepository)) {
        $(Resolve-Path -Path $AppServerRepository).Path
    } else {
        #todo: drop in Search-Up -RelativePathSpec ".git\" from profile to get git repo by default
        $gitPath = $(Resolve-Path "$PSScriptRoot\..\.git").Path
        $(Resolve-Path "$gitPath\..").Path
    };
    
    if (-not $(test-path $repoPath -PathType Container)) {
        Write-Log "Error: repository path not found. Run from a subfolder of the repo, or specify -Repository <repository root path>"
        throw "Repository path not found: $repoPath";
    } else {
        Write-Log "Using 'Blazor SessionBroker' from repo: $repoPath"
    };

    $blazorClientRepo = if ([string]::IsNullOrEmpty($ClientAppRepository)) {
        $(Join-Path $repoPath "..\Blazor.Client\")
    } else {
        $(Resolve-Path $ClientAppRepository).Path
    };
    
    if (-not $(test-path $blazorClientRepo -PathType Container)) {
        Write-Log "Error: repo path for blazor client exe not found (expected Blazor.Client repository path to identify running ClientApp procs)"
        throw "Repository path not found: $blazorClientRepo";
    } else {
        Write-Log "Using 'Blazor ClientApp' from repo: $blazorClientRepo"
    };

    $cwDevRepo = if ([string]::IsNullOrEmpty($CargoWiseRepository)) {
        $repoPath
    } else {
        $(Resolve-Path $CargoWiseRepository).Path
    };
    
    if (-not $(test-path $cwDevRepo -PathType Container)) {
        Write-Log "Error: repo path for cw dev exe not found (expected Blazor or Dev repository path)"
        throw "Repository path not found: $cwDevRepo";
    } else {
        Write-Log "Using 'CargoWiseOneAnyCpu' from repo: $cwDevRepo"
    }
    
    if ([string]::IsNullOrEmpty($Command)) {
        while ($true) {
            try {
                Start-Command $(Read-Host -Prompt "Command")
            } catch {
                Write-Log "Error running $($Command): $_" -ForegroundColor Red @args
            }
        }
    } else {
        Start-Command $Command > $null
    }
} finally {
    Write-Log "End $scriptName (elapsed: $($sw.Elapsed))"
}
