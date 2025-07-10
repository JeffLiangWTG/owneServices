$basePath = Join-Path $PSScriptRoot "..\Bin\"
pushd $basePath
$appServer = $null
try {
    write-host "starting appserver"
    cd "AppServer"
    $appServer = start-process -FilePath "CargoWise.Blazor.AppServer.exe" -ArgumentList "--environment Development" -PassThru
    Start-Sleep -Seconds 3
    if ($appServer.HasExited) {
        throw "appserver exited $($appServer.ExitCode)"
    }
    write-host "appserver started (hopefully), launching playwright"
    cd "..\"
    $env:PWDEBUG="console"
    playwright "open" "https://localhost:5001"
} finally {
    popd
    if (-not $appServer.HasExited) {
        write-host "killing appserver instance"
        $appServer.Kill($true)
    }
}