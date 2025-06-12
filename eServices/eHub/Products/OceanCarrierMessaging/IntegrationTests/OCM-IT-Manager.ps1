[CmdletBinding(PositionalBinding=$false)]

Param (
 [Parameter(Mandatory=$false)][string]$server,
 [Parameter(Mandatory=$false)][string]$provider,
 [Parameter(Mandatory=$false)][string]$userName,
 [Parameter(Mandatory=$false)][string]$password,
 [Parameter(Mandatory=$false)][switch]$help,
 [Parameter(Mandatory=$false)][switch]$h,
 [Parameter(Mandatory=$false)][switch]$v,
 [Parameter(Mandatory=$false)][switch]$version
)#end param

function formatXml([xml]$xml, $indent=2) {
	$StringWriter = New-Object System.IO.StringWriter
    $XmlWriter = New-Object System.XMl.XmlTextWriter $StringWriter
    $xmlWriter.Formatting = “indented”
    $xmlWriter.Indentation = $Indent
    $xml.WriteContentTo($XmlWriter)
    $XmlWriter.Flush()
    $StringWriter.Flush()
    return $StringWriter.ToString()
}

function updateTestBase_Infrastructure() {
	$baseXmlPath  = Join-Path -Path  $PSScriptRoot -ChildPath TestDataBase\TestData\eHubTransactions_Infrastructure.xml
	$testBaseScriptsPath= Join-Path -Path  $PSScriptRoot -ChildPath TestBase.sql
	validatePath $baseXmlPath
	validatePath $testBaseScriptsPath

    $scriptFile = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::ReadAllText($testBaseScriptsPath) | Set-Content $scriptFile
	if(($userName -eq "") -or ($password -eq ""))
	{
		$xmlFileContent = Invoke-Sqlcmd -ServerInstance $server -database 'eHubTransactions' -InputFile $scriptFile -MaxCharLength ([int32]::MaxValue) 
	}
	elseif($userName -ieq "ehubreader")
	{
		Install-Module -Name SqlServer -Scope CurrentUser -AllowClobber
		$xmlFileContent = Invoke-Sqlcmd -ConnectionString "Data Source=$server;Initial Catalog=eHubTransactions;App=eHub Gateway;User Id=$userName;Password=$password;ApplicationIntent=ReadOnly" -InputFile $scriptFile -MaxCharLength ([int32]::MaxValue)
	}
	else
	{
		$xmlFileContent = Invoke-Sqlcmd -ServerInstance $server -database 'eHubTransactions' -Username $userName -Password $password -InputFile $scriptFile -MaxCharLength ([int32]::MaxValue)
	}

	Write-Host -ForegroundColor Green "Script Executed Successfully" 

    formatXml($xmlFileContent.Column1) | Set-Content $baseXmlPath
	
	Write-Host -ForegroundColor Green "File At "  $baseXmlPath  " has been modified, Base has been updated successfully" 
}

function updateTestProvider_Infrastructure([string] $provider_name, [string] $provider_name_in_eHubClient = "") {
	$testProviderScriptsPath= Join-Path -Path  $PSScriptRoot -ChildPath 'TestProvider.sql'
	$childXmlPath = $provider_name + '/' + $provider_name + '_RoutingRuleTestData\eHubTransactions_Infrastructure.xml'
	$providerXmlPath  = Join-Path -Path  $PSScriptRoot -ChildPath $childXmlPath
	validatePath $testProviderScriptsPath
	validatePath $providerXmlPath
	$providerName = "ProviderName=$provider_name"
	if ($provider_name_in_eHubClient)
	{
		$providerName = "ProviderName=$provider_name_in_eHubClient";
	}
	$query = [System.IO.File]::ReadAllText($testProviderScriptsPath)

	if(($userName -eq "") -or ($password -eq ""))
	{
		$xmlFileContent = Invoke-Sqlcmd -ServerInstance $server -database 'eHubTransactions' -Query $query -Variable $providerName -MaxCharLength ([int32]::MaxValue)
	}
	elseif($userName -ieq "ehubreader")
	{
		Install-Module -Name SqlServer -Scope CurrentUser -AllowClobber
		$xmlFileContent = Invoke-Sqlcmd -ConnectionString "Data Source=$server;Initial Catalog=eHubTransactions;App=eHub Gateway;User Id=$userName;Password=$password;ApplicationIntent=ReadOnly" -Query $query -Variable $providerName -MaxCharLength ([int32]::MaxValue)
	}
	else
	{
		$xmlFileContent = Invoke-Sqlcmd -ServerInstance $server -database 'eHubTransactions' -Username $userName -Password $password -Query $query -Variable $providerName -MaxCharLength ([int32]::MaxValue)
	}

	Write-Host -ForegroundColor Green "Script Executed Successfully" 
    formatXml($xmlFileContent.Column1) | Set-Content $providerXmlPath

	Write-Host -ForegroundColor Green "File At $providerXmlPath has been modified, $provider_name has been updated successfully" 
}



function updateProviders([string] $provider_name, [string] $provider_code_if_different = "") {
	switch($provider_name)
	{
		"Base" {
			Write-Host 'updating TestDataBase xml files'
			updateTestBase_Infrastructure 
		}
		"All" {
			Write-Host 'Updating All Test Data xml files'
			updateProviders "Base"
			updateProviders "INTTRA"
			updateProviders "Easipass"
			updateProviders "Ningbo" "NGBEDI"
			updateProviders "SouthEast"
		}
		"" {
			updateProviders "All"
		}
		default 
		{
			Write-Host 'updating TestProvider' $provider_name
			updateTestProvider_Infrastructure  $provider_name $provider_code_if_different
		}
	}
}

function validatePath([string] $path) {
	try
	{
		$test_content = [System.IO.File]::ReadAllText($path)
		Write-Host  "Path valide success"  $path -foregroundcolor "Yellow"
	}
	catch 
	{
		Write-Host  "Cann't read file in the path, you might have entered an invalid provider name or you don't have access to the file" -foregroundcolor "yellow"
		Write-Host  "Invalid Path is: " $path  -foregroundcolor "Magenta"
		throw "An error occurred during updating file: $($_.Exception)"
	}
}

Try {
	if($h -or $help)
	{
		Write-Host -ForegroundColor Green 'GET-HELP'
		Write-Host 'OCM-IT-Manager.ps1 [-server <string>] [-provider <string>] [-help] [-h] [-v] [-version] [<CommonParameters>]' 
		write-host "`n"
		Write-Host -ForegroundColor Green '-Server'
		Write-Host 'Format: SYDCO-XXXX-X by using Data from selected eHubTrasaction database. Default value is Localhost'
		write-host "`n"
		Write-Host -ForegroundColor Green '-Provider'
		Write-Host 'Indicates the Area you want to test,Contains the Easipass; INTTRA; NINGBO; SouthEast and Base. default Value: All'
		write-host "`n"
		Write-Host -ForegroundColor Green '-UserName and -Password'
		Write-Host 'Default username and password would be windows authentication, specify the username and password if you need to connect to other databases'
		write-host "`n"
		Write-Host -ForegroundColor Green '-v/-version; -h/-help '
		Write-Host 'Get versions and help information'
		write-host "`n"
		Write-Host -ForegroundColor Green 'EXAMPLES'
		Write-Host '.\OCM-IT-Manager.ps1 -help'
		Write-Host '.\OCM-IT-Manager.ps1 -Provider All'
		Write-Host '.\OCM-IT-Manager.ps1 -Provider Base'
		Write-Host '.\OCM-IT-Manager.ps1 -Server SYDCO-WJFL-2 -Provider Base'
		Write-Host '.\OCM-IT-Manager.ps1 -server wg1-vsql-1.wg.cargowise.com -userName ehubreader -password ehubrocks'
		Write-Host '.\OCM-IT-Manager.ps1 -Server SYDCO-WJFL-2'
		
		return
	}
	if($v -or $version)
	{
		Write-Host '1.0.0.0' 
		Write-Host 'Use Get-Help to obtain information on usage.' 
		return
	}

	if($server -eq "")
	{
		$server = "localhost"
	}
	Write-Host 'Will update from:' $server

	updateProviders $provider
}
Finally {
	# powershell doesnt reset current directory on exit, so we handle that manually
	Pop-Location
}
