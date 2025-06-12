. (Join-Path $PSScriptRoot\..\ Publish-Dacpac.ps1)

$serverName = 'sydbilling.db.wtg.zone'

# Name the database for the filegroup 7 years ago.
$databaseName = 'Billing2017'
$userName = 'billing_admin'
$password = 'ToBeProvidedAtRunTime'

# Use dacpac built from releases/production
$dacpacPath = "C:\wtgP\eServices\Billing\bin\Databases\CargoWise.eServices.Billing.Database\CargoWise.eServices.Billing.Database.dacpac"

Publish-Dacpac -serverName $serverName -databaseName $databaseName -userName $userName -password $password -dacpacPath $dacpacPath
