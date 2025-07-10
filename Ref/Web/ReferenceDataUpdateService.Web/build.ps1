param 
(
 [Parameter(Mandatory=$false)][string]$env
)

Write-Output "Env: $env"

$currentLocation = Get-Location

Write-Output "OutDir listage"
& ls -dir
if ($lastExitCode -ne 0) { exit 1 }

Write-Output "Machine name: $env:computername"
Write-Output "Current location: $currentLocation"

& npm --version
if ($lastExitCode -ne 0) { exit 1 }

& node --version
if ($lastExitCode -ne 0) { exit 1 }

& npm ci
if ($lastExitCode -ne 0) { exit 1 }

& npm run-script check-types
if ($lastExitCode -ne 0) { exit 1 }

& npm run-script build:$env  -- --output-path="wwwroot"
if ($lastExitCode -ne 0) { exit 1 }
