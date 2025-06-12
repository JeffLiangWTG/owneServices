param(
    [string]$SourceDir,
    [string]$InstallDir
)

if ([string]::IsNullOrEmpty($SourceDir)) {
    $SourceDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}

if (([string]::IsNullOrEmpty($InstallDir)) -or (-not [System.IO.File]::Exists($InstallDir))) {
    $InstallDir = "${env:ProgramFiles(x86)}\CargoWise eHub\eHub BizTalk Adapters"
}

$biztalkServices = Get-Service | Where-Object {$_.Name.StartsWith("BTSSvc$") -and ($_.Status -eq "Running")}
$biztalkServices | ForEach-Object {
    Write-Host $_.Name
    Stop-Service -Name $_.Name -Force
}

$BinPath = Join-Path $SourceDir "Bin\Adapters"
(Get-ChildItem $BinPath "*.dll") |
ForEach-Object {

    try {

        $srcFilePath = $_.FullName
        if (!(Test-Path $srcFilePath)) {
            Throw "Source file not found: $srcFilePath"
        }

        $destFilePath = Join-Path $InstallDir $_
        If (-not (Test-Path $destFilePath)) {
            New-Item -ItemType File -Path $destFilePath -Force
        }

        Copy-Item -Path $srcFilePath -Destination $InstallDir -Force -PassThru
    } catch {
        Write-Host $Error[0]
        return
    }
}

$biztalkServices |
ForEach-Object {
    Start-Service -Name $_.Name
}