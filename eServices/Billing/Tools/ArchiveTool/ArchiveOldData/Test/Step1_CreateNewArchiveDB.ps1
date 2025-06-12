. (Join-Path $PSScriptRoot\..\ Publish-Dacpac.ps1)

$serverName = 'au2sp-402l1.sand.wtg.zone'

# Name the database for the oldest year filegroup.
$databaseName = 'CargoWise.eServices.Billing.YearsPre2017'
$userName = 'DBDeploymentLogin'
$password = 'ToBeProvidedAtRunTime'

# Use dacpac built from releases/staging
$dacpacPath = "C:\wtgS\eServices\Billing\bin\Databases\CargoWise.eServices.Billing.Database\CargoWise.eServices.Billing.Database.dacpac"

Publish-Dacpac -serverName $serverName -databaseName $databaseName -userName $userName -password $password -dacpacPath $dacpacPath
