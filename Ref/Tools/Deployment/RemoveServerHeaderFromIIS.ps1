# please run it as Administrator
$sites = Get-WebSite

foreach ($site in $sites) {
	Write-Output "set site: $($site.Name)"
	Set-WebConfigurationProperty -pspath "MACHINE/WEBROOT/APPHOST/$($site.Name)"  -filter "system.webServer/security/requestFiltering" -name "removeServerHeader" -value "True"
}
