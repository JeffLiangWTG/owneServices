param
(
	[Parameter(Mandatory=$true)][string] $ComputerName,
	[Parameter(Mandatory=$true)][string] $UpdateServiceName,
	[Parameter(Mandatory=$true)][string] $DeliveryServiceName,
	[Parameter(Mandatory=$true)][string] $UpdateServicePhysicalFolder,
	[Parameter(Mandatory=$true)][string] $DeliveryServicePhysicalFolder,
	[Parameter(Mandatory=$false)][string] $UserName,
	[Parameter(Mandatory=$false)][string] $Password
)

function CreateWebSite([string] $updateServiceSiteName, [string] $updateServicePhysicalFolder, [string] $deliveryServiceSiteName, [string] $deliveryServicePhysicalFolder) {
	Import-Module WebAdministration
	$updateServiceAppPoolName = "$($updateServiceSiteName)"
	$deliveryServiceAppPoolName = "$($deliveryServiceSiteName)"

	if (Test-Path "IIS:\Sites\$updateServiceSiteName") {
		Write-Output "Removing existing site $updateServiceSiteName"
		Remove-Website -Name $updateServiceSiteName
	}
	if (Test-Path "IIS:\AppPools\$updateServiceAppPoolName") {
		Write-Output "Removing existing AppPool $updateServiceAppPoolName"
		Remove-WebAppPool -Name $updateServiceAppPoolName
	}
	if (Test-Path "IIS:\Sites\$deliveryServiceSiteName") {
		Write-Output "Removing existing site $deliveryServiceSiteName"
		Remove-Website -Name $deliveryServiceSiteName
	}
	if (Test-Path "IIS:\AppPools\$deliveryServiceAppPoolName") {
		Write-Output "Removing existing AppPool $deliveryServiceAppPoolName"
		Remove-WebAppPool -Name $deliveryServiceAppPoolName
	}

	#update service website
	Write-Output "Creating $updateServiceAppPoolName AppPool"
	New-WebAppPool  -Name $updateServiceAppPoolName
	Write-Output "Creating $updateServiceSiteName website"
	New-WebSite -Name $updateServiceSiteName -Port 80 -ApplicationPool $updateServiceAppPoolName -PhysicalPath $updateServicePhysicalFolder -HostHeader $updateServiceSiteName -Force

	#delivery service websites
	Write-Output "Creating $deliveryServiceAppPoolName AppPool"
	New-WebAppPool  -Name $deliveryServiceAppPoolName
	Write-Output "Creating $deliveryServiceSiteName website"
	New-WebSite -Name $deliveryServiceSiteName -Port 80 -ApplicationPool $deliveryServiceAppPoolName -PhysicalPath $deliveryServicePhysicalFolder -HostHeader $deliveryServiceSiteName -Force
	Write-Output "Setting preloadEnabled=true"
	Set-ItemProperty "IIS:\Sites\$deliveryServiceSiteName" -name applicationDefaults.preloadEnabled -value True

	Write-Output "Creating Portal app"
	New-WebApplication -Name Portal -Site $updateServiceSiteName -PhysicalPath "$updateServicePhysicalFolder\Portal" -ApplicationPool $updateServiceAppPoolName -Force

	Write-Output "Creating UpdateService folder to avoid app_offline.htm missing"
	New-Item -Path "$updateServicePhysicalFolder\UpdateService" -ItemType Directory -Force
	Write-Output "Creating UpdateService app"
	New-WebApplication -Name Update -Site $updateServiceSiteName -PhysicalPath "$updateServicePhysicalFolder\UpdateService" -ApplicationPool $updateServiceAppPoolName -Force

	Write-Output "Creating Staging app"
	New-WebApplication -Name Staging -Site $updateServiceSiteName -PhysicalPath "$updateServicePhysicalFolder\Staging" -ApplicationPool $updateServiceAppPoolName -Force
}

try {
	Write-Output "Running CreateServiceWebSites.ps1 on: $ComputerName"
	if ($UserName -and $Password)
	{
		Write-Output "User and pass provided"
		$securePassword = ConvertTo-SecureString -AsPlainText -Force -String $Password
		$credential = New-Object -typename System.Management.Automation.PSCredential -argumentlist $UserName, $securePassword
		Invoke-Command `
			-ComputerName $ComputerName `
			-Credential $credential `
			-ErrorAction Stop `
			-ScriptBlock ${function:CreateWebSite} `
			-ArgumentList $UpdateServiceName, $UpdateServicePhysicalFolder, $DeliveryServiceName, $DeliveryServicePhysicalFolder
	} else {
		Invoke-Command `
			-ComputerName $ComputerName `
			-ErrorAction Stop `
			-ScriptBlock ${function:CreateWebSite} `
			-ArgumentList $UpdateServiceName, $UpdateServicePhysicalFolder, $DeliveryServiceName, $DeliveryServicePhysicalFolder
	}
	
	Write-Output "The script has successfully completed!"
} catch {
	Write-Error "Failed to create sites: $($_.Exception)"
	exit(1)
}
