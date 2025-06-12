$ErrorActionPreference = 'Stop'
Set-Location "$env:DAT_BIN_PATH\DAT"
Add-Type -Path Dat.Integration.dll, eHub.DatImplementation.Deployment.dll
$dep = New-Object eHub.DatImplementation.Deployment.BuildDeployer (New-Object Dat.Integration.TaskLogger)
$dep.DeployOnDemand("Release", $args[0], $env:DAT_SOURCE_PATH, $env:DAT_BIN_PATH)
