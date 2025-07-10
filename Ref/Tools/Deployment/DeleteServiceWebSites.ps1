param
(
	[Parameter(Mandatory=$true)][string] $ComputerName,
	[Parameter(Mandatory=$true)][string] $UpdateServiceName,
	[Parameter(Mandatory=$true)][string] $DeliveryServiceName,
	[Parameter(Mandatory=$false)][string] $UserName,
	[Parameter(Mandatory=$false)][string] $Password
)

function DeleteWebSite([string] $updateServiceSiteName, [string] $deliveryServiceSiteName) {
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
}

try {
	if ($UserName -and $Password)
	{
		Write-Output "User and pass provided"
		$securePassword = ConvertTo-SecureString -AsPlainText -Force -String $Password
		$credential = New-Object -typename System.Management.Automation.PSCredential -argumentlist $UserName, $securePassword
		Invoke-Command `
			-ComputerName $ComputerName `
			-Credential $credential `
			-ErrorAction Stop `
			-ScriptBlock ${function:DeleteWebSite} `
			-ArgumentList $UpdateServiceName, $DeliveryServiceName
	} else {
		Invoke-Command `
			-ComputerName $ComputerName `
			-ErrorAction Stop `
			-ScriptBlock ${function:DeleteWebSite} `
			-ArgumentList $UpdateServiceName, $DeliveryServiceName
	}

	Write-Output "The script has successfully completed!"
} catch {
	Write-Error "Failed to delete site: $($_.Exception)"
	exit(1)
}
