param(
    [Parameter(Mandatory=$true)][string]$Path, 
    [Parameter(Mandatory=$false)][hashtable]$Settings
)

$ErrorActionPreference = 'Stop'

$webConfig = [xml](Get-Content $Path)

$aspnetcore = $webConfig["configuration"]["location"]["system.webServer"]["aspNetCore"]
if ($aspnetcore -eq $null) { throw "Not a valid ASP.Net Core web.config file." }

$envVars = $aspnetcore["environmentVariables"]
if ($null -eq $envVars) {
	$envVars = $webConfig.CreateElement("environmentVariables")
	$aspnetcore.AppendChild($envVars) | Out-Null
}

foreach ($Key in $Settings.Keys) {
    $setting = ($envVars | Select-Xml -XPath "*[@name='$Key'][1]").Node
    if ($null -eq $setting) {
	    $setting = $webConfig.CreateElement("environmentVariable")
	    $setting.SetAttribute("name", $Key)
	    $envVars.AppendChild($setting) | Out-Null
    }
    $setting.SetAttribute("value", $Settings[$Key]) | Out-Null
}
$webConfig.Save($Path)
