$ProgressPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'

# prerequisite variable conditions:
# 1. set $var.biztalk_config_xml_filepath
# 2. set ${var.DEVADMIN_USERNAME}
# 3. set ${var.DEVADMIN_PASSWORD}}
# 4. $env:BTSINSTALLPATH is set if BizTalk Server 2013 R2 is already installed

$PsThisScriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$var = @{}
$var | Add-Member biztalk_config_xml_filepath (Join-Path $PsThisScriptPath "biztalk-server-2013r2-server-config.xml")
$var | Add-Member DEVADMIN_USERNAME DevAdmin
$var | Add-Member DEVADMIN_PASSWORD 3hubRock$

Set-Location -Path $env:TEMP

(Get-Date -format '[yyyy-MM-dd HH:mm:ss]: ') + 'Config BizTalk Server 2013 R2 server environment' | Write-Output

Copy-Item -Path $var.biztalk_config_xml_filepath -Destination biztalk-server-2013r2-server-config.xml

$ssoBackupFileName = [guid]::NewGuid()
$ssoSecretsBackUpFilePath = "${env:PROGRAMFILES}\Common Files\Enterprise Single Sign-On\${ssoBackupFileName}.bak"

(Get-Content biztalk-server-2013r2-server-config.xml) |
Foreach-Object {$_ -replace '{env:SSO_SECRETS_BACKUP_FILEPATH}', ${ssoSecretsBackUpFilePath}} |
Foreach-Object {$_ -replace '{env:DEVADMIN_USERNAME}', $var.DEVADMIN_USERNAME} |
Foreach-Object {$_ -replace '{env:DEVADMIN_PASSWORD}', $var.DEVADMIN_PASSWORD} |
Foreach-Object {$_ -replace '{env:COMPUTERNAME}', ${env:COMPUTERNAME}} |
Out-File biztalk-server-config.xml | Out-Null

$logFilePath = 'biztalk-server-config_' + (Get-Date -format 'yyyyMMdd_HHmmss') + '.log'
& "$env:BTSINSTALLPATH\Configuration.exe" /s biztalk-server-config.xml /noprogressbar /l $logFilePath | Out-Null

Get-content $logFilePath | Out-String | Write-Output
