if (-not (Test-Path env:DAT_IS_BUILDING)) { 
    Write-Output "Skipped. This script is intended to be run on DAT only."
    return 
}

Write-Output "Cleaning up refs directories in bin..."
Get-ChildItem -Path "bin" -Filter "refs" -Recurse -Directory | ForEach-Object {
    Write-Output "Deleting $($_.FullName)"
    Remove-Item -Path $_.FullName -Force -Recurse
}