param
(
  [Parameter(Mandatory=$false)][string]$sourcePath = "C:\RefDbRepo",
  [Parameter(Mandatory=$false)][string]$binPath = "C:\RefDbRepo\Bin",
  [Parameter(Mandatory=$false)][string]$publishProfile = "Test",
  [Parameter(Mandatory=$false)][string]$stagingDestination = "StagingDeployTest\",
  [Parameter(Mandatory=$false)][string]$xmlProducersDestination = "XMLProducersDeployTest\",
  [Parameter(Mandatory=$false)][string[]]$stagingServer = @("SYDSP-SWEB-1.sand.wtg.zone", "SYDSP-SWEB-4.sand.wtg.zone"),
  [Parameter(Mandatory=$false)][string]$stagingTaskPath = "RefDbRepoDeployTest\",
  [Parameter(Mandatory=$false)][string]$connectionStringJsonConfigFilePath = "C:\git\wtg\RefDataRepo\RefDataRepo\Common\Infrastructure\Utils\ConnectionStrings.config.json",
  [Parameter(Mandatory=$false)][string[]]$connectionStringJsonConfigDestinationFolders = @(),
  [Parameter(Mandatory=$false)][string]$deliveryServiceDeployPassword = "",
  [Parameter(Mandatory=$false)][string]$updateServiceDeployUsername = "",
  [Parameter(Mandatory=$false)][string]$updateServiceDeployPassword = "",
  [Parameter(Mandatory=$false)][string]$stagingDeployUsername = "",
  [Parameter(Mandatory=$false)][string]$stagingDeployPassword = "",
  [Parameter(Mandatory=$false)][string]$dbPassword = "",
  [Parameter(Mandatory=$false)][string]$dbWriterPassword = "",
  [Parameter(Mandatory=$false)][string]$dbReaderPassword = "",
  [Parameter(Mandatory=$false)][string]$dbWriterPasswordJson = "",
  [Parameter(Mandatory=$false)][string]$dbReaderPasswordJson = "",
  [Parameter(Mandatory=$false)][string]$msbuild = "",
  [Parameter(Mandatory=$false)][string]$msdeploy = "",
  [Parameter(Mandatory=$false)][string]$quartzServiceName = "RefDbRepo Quartz Server",
  [Parameter(Mandatory=$false)][string]$refDbRepoSafeDbName = "RefDbRepoSafe",
  [Parameter(Mandatory=$false)][string]$refDbRepoStagingDbName = "RefDbRepoStaging",
  [Parameter(Mandatory=$false)][string]$refDbRepoDbServer = "RefDbRepoStaging.db.sand.wtg.zone",
  [Parameter(Mandatory=$false)][string]$dbWriterUsername = "",
  [Parameter(Mandatory=$false)][string]$dbReaderUsername = "",
  [Parameter(Mandatory=$false)][string]$deliveryServiceUrl = "",
  [Parameter(Mandatory=$false)][string]$updateServiceUrl = "",
  [Parameter(Mandatory=$false)][boolean]$deployQuartz = $true,
  [Parameter(Mandatory=$false)][boolean]$deployUpdateService = $true,
  [Parameter(Mandatory=$false)][boolean]$deployStagingService = $true,
  [Parameter(Mandatory=$false)][boolean]$deployPortal = $true,
  [Parameter(Mandatory=$false)][boolean]$deployDeliveryService = $true,
  [Parameter(Mandatory=$false)][boolean]$forceSafeDbUprade = $false,
  [Parameter(Mandatory=$false)][boolean]$forceStagingDbUprade = $false,
  [Parameter(Mandatory=$false)][boolean]$upgradeStagingDb = $true,
  [Parameter(Mandatory=$false)][boolean]$upgradeSafeDb = $true,
  [Parameter(Mandatory=$false)][string]$deliveryServiceServerName = "",
  [Parameter(Mandatory=$false)][string]$seleniumServiceUrl = ""
)

Add-Type -AssemblyName "System.IO.Compression.FileSystem"
Add-Type -LiteralPath "$binPath\Tools\net8.0\Microsoft.Web.XmlTransform.dll"

Write-Output "Source Path: $sourcePath"
Write-Output "Bin Path: $binPath"
Write-Output "Publish Profile: $publishProfile"
Write-Output "Staging Destination: $stagingDestination"
Write-Output "Staging Server: $stagingServer"
Write-Output "MsBuild Path: $msbuild"
Write-Output "MsDeploy Path:" "$msdeploy"

$currentLocation = Get-Location
Set-Location -Path "$sourcePath"
& ".\setup.cmd"
Set-Location -Path $currentLocation

# Clear local publish folder
Write-Output "Clearing build folder.."
If (Test-Path "$binPath\deploy\")
{
  Remove-Item "$binPath\deploy\" -Recurse -Confirm:$false
}

function TransformJson($configFolder, $projectFilePath)
{
	$projectName = [io.path]::GetFileNameWithoutExtension($projectFilePath)
    $projectFolder = Split-Path -Parent $projectFilePath
	$envJsonFile = "$projectFolder\deploy.$publishProfile.config.json"
	$configJsonFiles = @(Get-ChildItem -Path $configFolder -Recurse -Include *$projectName.config.json)

	foreach ($jsonFile in $configJsonFiles)
	{
		$configJsonFile = $jsonFile."FullName"
		$sourceJson = Get-Content $configJsonFile | ConvertFrom-Json
		$envJson = Get-Content $envJsonFile | ConvertFrom-Json

		$envJson.PSObject.Properties | ForEach-Object {
			$envKey = $_.Name
			$envValue = $_.Value

			if ($sourceJson.PSObject.Properties.Name -contains $envKey) {
				$sourceJson.$envKey = $envValue
			} else {
				$sourceJson | Add-Member -NotePropertyName $envKey -NotePropertyValue $envValue
			}
		}

		$sourceJson | ConvertTo-Json -Depth 100 | Out-File $configJsonFile -Encoding UTF8
		ReplaceAndSetConfig $configJsonFile
	}
}

function ReplaceAndSetConfig($config, [Parameter(Mandatory=$false)] $configToSet)
{
	$replacedContent = (Get-Content $config).`
		Replace('***dbPassword***', $dbPassword).`
		Replace('***dbWriterPassword***', $dbWriterPassword).`
		Replace('***dbReaderPassword***', $dbReaderPassword).`
		Replace('***dbWriterPasswordJson***', $dbWriterPasswordJson).`
		Replace('***dbReaderPasswordJson***', $dbReaderPasswordJson).`
		Replace('***refDbRepoSafeDbName***', $refDbRepoSafeDbName).`
		Replace('***refDbRepoStagingDbName***', $refDbRepoStagingDbName).`
		Replace('***refDbRepoDbServer***', $refDbRepoDbServer).`
		Replace('***dbWriterUsername***', $dbWriterUsername).`
		Replace('***dbReaderUsername***', $dbReaderUsername).`
		Replace('***updateService***', $updateServiceUrl).`
		Replace('***deliveryService***', $deliveryServiceUrl).`
		Replace('***deliveryServiceServerName***', $deliveryServiceServerName).`
		Replace('***seleniumServiceUrl***', $seleniumServiceUrl).`
		Replace('***updateServiceServerName***', $stagingServer[0])

	$replacedContent | Set-Content $config
	if ($configToSet) {
		$replacedContent | Set-Content $configToSet
	}
}

function TransformConnectionStringsJson()
{
	#remove .json from file extension
    $configFileName = [io.path]::GetFileNameWithoutExtension($connectionStringJsonConfigFilePath)
	#remove .config from file extension
	$configFileName = [io.path]::GetFileNameWithoutExtension($configFileName)
    $configFolder = Split-Path -Parent $connectionStringJsonConfigFilePath
    $publishProfileConfigFile = "$configFolder\$configFileName.$publishProfile.config.json"
    foreach ($folder in $connectionStringJsonConfigDestinationFolders)
    {
        $destinationConfigFile = "$folder\$configFileName.config.json"
        Write-Output "Copying config file from $publishProfileConfigFile to $destinationConfigFile"
        Copy-Item $publishProfileConfigFile -Destination $destinationConfigFile -Force
		ReplaceAndSetConfig $destinationConfigFile
    }
}

########################### Deploy web projects ###################################################
Write-Output "Transforming connection strings config files"
TransformConnectionStringsJson

# Publish bin folder for Server
Write-Output "Copying Server dlls"
xcopy /s /y "$binPath\Server\net8.0\" "$binPath\deploy\Server\net8.0\"

# Publish transformed configuration for console projects
Write-Output "Building and transforming console projects.."
TransformJson $binPath\deploy\Server\net8.0\ $sourcePath\Service\SchemaManagement\UpgradeManagerRunner\UpgradeManagerRunner.csproj

function CreatePSDrive($driveName , $destination)
{
    if (-Not (Get-PSDrive -Name $driveName -ErrorAction SilentlyContinue) -And $stagingDeployUsername)
    {
        $trimmedDestination = $destination.TrimEnd('\')
        $securedStagingDeployPassword = ConvertTo-SecureString $stagingDeployPassword -AsPlainText -Force
        $stagingDeployCredential = New-Object -TypeName System.Management.Automation.PSCredential `
        -ArgumentList $stagingDeployUsername, $securedStagingDeployPassword
        New-PSDrive -PSProvider FileSystem -Name $driveName -Root $trimmedDestination -Credential $stagingDeployCredential -Scope Script
        Write-Output "PSDrive created: Name = ${driveName} Root = ${destination}"
    }
    else
    {
        if ($stagingDeployUsername)
        {
            Write-Output "PSDrive already exists: Name = ${driveName}"
        }
        else
        {
            Write-Output "PSDrive not created because parameter stagingDeployUsername is blank"
        }
    }
}

function RemovePSDrive($driveName)
{
    if (Get-PSDrive -Name $driveName -ErrorAction SilentlyContinue)
    {
        Remove-PSDrive -Name $driveName
        Write-Output "PSDrive removed: Name = ${driveName}"
    }
    else
    {
        Write-Output "PSDrive not found: Name = ${driveName}"
    }
}

function RecycleAppPool($serviceName, $projectPath, $publishProfile, $userName, $password, $command)
{
    Write-Output "$command on $serviceName application pool"
    $authType = if ($password) { "Basic" } else  { "NTLM" }
    & $msbuild $projectPath /p:PublishProfile=$publishProfile /t:RecycleAppPool `
    /p:username=$userName /p:password=`"$password`" /p:AuthType="$authType" /p:MsDeployPath="$msdeploy" /p:AppPoolCommand="$command"

    if ($lastExitCode -ne 0) { Write-Error "$command on $serviceName application pool failed" }
}

function GetUserName($publishProfile)
{
    [xml]$xmlContent = Get-Content $publishProfile
    $userName = ($xmlContent).Project.PropertyGroup.UserName
    if ($userName -is [array])
    {
        return $userName[0]
    }
    return $userName
}

if ($deployQuartz)
{
	# Stop Scheduler Service
	foreach ($server in $stagingServer)
	{
		CreatePSDrive "RefDataRepoService" "\\${server}\${stagingDestination}"
		Write-Output "Stop Scheduler Service on ${server}"
		Get-Service -ComputerName $server -Name $quartzServiceName | Stop-Service
		RemovePSDrive "RefDataRepoService"
	}
}

if ($deployDeliveryService)
{
	#new delivery service
	$deliveryServicePublishProfiles = Get-ChildItem $sourcePath\Service\NewService\Properties\PublishProfiles\$publishProfile*.pubxml -Name
	foreach ($deliveryServicePublishProfile in $deliveryServicePublishProfiles)
	{
		# replace service publication files (in theory, nothing for now)
		Write-Output "Replacing new delivery service Pubxml"
		New-Item $binPath\deploy\NewService\$deliveryServicePublishProfile -Type File -Force
		ReplaceAndSetConfig $sourcePath\Service\NewService\Properties\PublishProfiles\$deliveryServicePublishProfile $binPath\deploy\NewService\$deliveryServicePublishProfile

		#transform json config in sourcePath to be published later
		TransformJson $sourcePath\Service\NewService $sourcePath\Service\NewService\NewService.csproj

		# Turn applications offline
		$deliveryServiceDeployUsername = GetUserName "$sourcePath\Service\NewService\Properties\PublishProfiles\$deliveryServicePublishProfile"
		RecycleAppPool "Delivery Service" "$sourcePath\Service\NewService\NewService.csproj" $deliveryServicePublishProfile $deliveryServiceDeployUsername $deliveryServiceDeployPassword "StopAppPool"
	}
}

if ($deployUpdateService)
{
	#replace service publication files
	#new update service
	$updateServicePublishProfiles = Get-ChildItem $sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($updateServicePublishProfile in $updateServicePublishProfiles)
	{
		Write-Output "Replacing new update service Pubxml"
		New-Item $binPath\deploy\NewUpdateService\$updateServicePublishProfile -Type File -Force
		ReplaceAndSetConfig $sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\Properties\PublishProfiles\$updateServicePublishProfile $binPath\deploy\NewUpdateService\$updateServicePublishProfile

		#transform json config in sourcePath to be published later
		TransformJson $sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService $sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\NewSafeDataUpdateService.csproj

		RecycleAppPool "Update Service" "$sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\NewSafeDataUpdateService.csproj" $updateServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StopAppPool"
	}
}

if ($deployPortal)
{
	#Portal
	$updateServicePublishProfiles = Get-ChildItem $sourcePath\Web\ReferenceDataUpdateService.Web\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($updateServicePublishProfile in $updateServicePublishProfiles)
	{
		Write-Output "Replacing Portal Pubxml and webpack"
		New-Item $binPath\deploy\Portal\$updateServicePublishProfile -Type File -Force
		ReplaceAndSetConfig $sourcePath\Web\ReferenceDataUpdateService.Web\Properties\PublishProfiles\$updateServicePublishProfile $binPath\deploy\Portal\$updateServicePublishProfile
		New-Item "$binPath\deploy\Portal\webpack.$publishProfile.js" -Type File -Force
		ReplaceAndSetConfig "$sourcePath\Web\ReferenceDataUpdateService.Web\webpack.$publishProfile.js" "$binPath\deploy\Portal\webpack.$publishProfile.js"

		RecycleAppPool "Portal" "$sourcePath\Web\ReferenceDataUpdateService.Web\ReferenceDataUpdateService.Web.csproj" $updateServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StopAppPool"
	}
}

if ($deployStagingService)
{
	#New Staging
	$stagingServicePublishProfiles = Get-ChildItem $sourcePath\Staging\Service\NewService\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($stagingServicePublishProfile in $stagingServicePublishProfiles)
	{
		Write-Output "Replacing new staging service Pubxml"
		New-Item $binPath\deploy\NewStagingService\$stagingServicePublishProfile -Type File -Force
		ReplaceAndSetConfig $sourcePath\Staging\Service\NewService\Properties\PublishProfiles\$stagingServicePublishProfile $binPath\deploy\NewStagingService\$stagingServicePublishProfile

		#transform json config in sourcePath to be published later
		TransformJson $sourcePath\Staging\Service\NewService $sourcePath\Staging\Service\NewService\NewService.csproj

		# Turn applications offline
		RecycleAppPool "New Staging Service" "$sourcePath\Staging\Service\NewService\NewService.csproj" $stagingServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StopAppPool"
	}
}

if ($upgradeSafeDb)
{
	# Upgrade database
	if ($forceSafeDbUprade)
	{
		& "$binPath\deploy\Server\net8.0\CargoWise.RefDbRepo.Service.UpgradeManagerRunner.exe" -force
	}
	else
	{
		& "$binPath\deploy\Server\net8.0\CargoWise.RefDbRepo.Service.UpgradeManagerRunner.exe"
	}

	if ($lastExitCode -ne 0) { Write-Error "Upgrade Safe database failed" }
}

if ($deployDeliveryService)
{
	# Deploy New Delivery Service
	Write-Output "Deploying New Service.."
	$authType = if ($deliveryServiceDeployPassword) { "Basic" } else  { "NTLM" }
	$deliveryServicePublishProfiles = Get-ChildItem $sourcePath\Service\NewService\Properties\PublishProfiles\$publishProfile*.pubxml -Name
	foreach ($deliveryServicePublishProfile in $deliveryServicePublishProfiles)
	{
		$deliveryServiceDeployUsername = GetUserName "$sourcePath\Service\NewService\Properties\PublishProfiles\$deliveryServicePublishProfile";

		& $msbuild "$sourcePath\Service\NewService\NewService.csproj" /t:"Restore;Build" /p:DeployOnBuild=True /p:UserName= `
		/p:AuthType=$authType /p:PublishProfile=$deliveryServicePublishProfile /p:AllowUntrustedCertificate=True /p:Configuration=Release `
		/p:IntermediateOutputPath=$binPath\deploy\NewService\ /p:OutDir="$binPath\Server\net8.0\" `
		/p:username=$deliveryServiceDeployUsername /p:password=$deliveryServiceDeployPassword `
		/p:PublishProfileRootFolder=$binPath\deploy\NewService /p:ReferencePath="$binPath\Server\net8.0\"

		if ($lastExitCode -ne 0) { Write-Error "New Delivery Service Deployment failed" }

		RecycleAppPool "Delivery Service" "$sourcePath\Service\NewService\NewService.csproj" $deliveryServicePublishProfile $deliveryServiceDeployUsername $deliveryServiceDeployPassword "StartAppPool"
	}
}

if ($deployUpdateService)
{
	# Deploy New Update Service
	Write-Output "Deploying New Update Service.."
	$authType = if ($updateServiceDeployUsername) { "Basic" } else  { "NTLM" }
	$updateServicePublishProfiles = Get-ChildItem $sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($updateServicePublishProfile in $updateServicePublishProfiles)
	{
	  & $msbuild "$sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\NewSafeDataUpdateService.csproj" /t:"Restore;Build" /p:DeployOnBuild=True /p:UserName= `
	  /p:AuthType=$authType /p:PublishProfile=$updateServicePublishProfile /p:AllowUntrustedCertificate=True /p:Configuration=Release `
	  /p:IntermediateOutputPath=$binPath\deploy\NewUpdateService\ /p:OutDir="$binPath\Server\net8.0\" `
	  /p:username=$updateServiceDeployUsername /p:password=`"$updateServiceDeployPassword`" `
	  /p:PublishProfileRootFolder=$binPath\deploy\NewUpdateService /p:ReferencePath="$binPath\Server\net8.0\"

	  if ($lastExitCode -ne 0)
	  {
		Write-Error "New Update Service Deployment failed"
	  }

	  RecycleAppPool "Update Service" "$sourcePath\Service\SafeDataUpdateService\NewSafeDataUpdateService\NewSafeDataUpdateService.csproj" $updateServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StartAppPool"
	}
}

########################## Deploy Staging projects ################################################
function ZipFolder($folder)
{
    $folderName = Split-Path $folder -Leaf
    $zipFile = $folder.trimend("\") + "\..\$folderName.zip"
    [io.compression.zipfile]::CreateFromDirectory($folder, $zipFile)
    Remove-Item $folder -Recurse -Confirm:$false
}

function ExtractFolder($destination, $zipfileName)
{
    $guid = [System.Guid]::NewGuid()
    $tmpFolder = $destination.trimend("\") + "\$guid"
    $zipFile = $destination.trimend("\") + "\$zipfileName"
    [io.compression.zipfile]::ExtractToDirectory($zipfile, $tmpFolder)
    xcopy /s /y $tmpFolder "$tmpFolder\..\"
    # Remove-Item $zipFile -Confirm:$false
    Remove-Item $tmpFolder -Recurse -Confirm:$false
}

if ($deployPortal)
{
	# Deploy Portal
	Write-Output "Deploying Portal.."
	$authType = if ($updateServiceDeployUsername) { "Basic" } else  { "NTLM" }
	$updateServicePublishProfiles = Get-ChildItem $sourcePath\Web\ReferenceDataUpdateService.Web\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($updateServicePublishProfile in $updateServicePublishProfiles)
	{
	  # execute build.ps1 before deployment
	  #set location for execution of npm commands
	  Set-Location "$sourcePath\Web\ReferenceDataUpdateService.Web\"
	  #execute build.ps1 considering current location
	  & ".\build.ps1" $publishProfile
	  #set location back to original location
	  Set-Location $currentLocation

	  if ($lastExitCode -ne 0) { Write-Error "Portal Deployment failed while executing build.ps1" }

	  & $msbuild "$sourcePath\Web\ReferenceDataUpdateService.Web\ReferenceDataUpdateService.Web.csproj" /t:"Restore;Build" /p:DeployOnBuild=True /p:UserName= `
	  /p:AuthType=$authType /p:PublishProfile=$updateServicePublishProfile /p:AllowUntrustedCertificate=True /p:Configuration=Release `
	  /p:IntermediateOutputPath=$binPath\deploy\Portal\ /p:OutDir="$binPath\Portal\net8.0\" `
	  /p:username=$updateServiceDeployUsername /p:password=`"$updateServiceDeployPassword`" `

	  if ($lastExitCode -ne 0) { Write-Error "Portal Deployment failed" }

	  RecycleAppPool "Portal" "$sourcePath\Web\ReferenceDataUpdateService.Web\ReferenceDataUpdateService.Web.csproj" $updateServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StartAppPool"
	}
}

function DeployBinary($psDriveName, $destination, $zipfile)
{
    CreatePSDrive $psDriveName $destination
    xcopy /s /y $zipfile $destination
}

# Publish bin folder for Staging
Write-Output "Copying Staging dlls.."
xcopy /s /y "$binPath\Staging" "$binPath\deploy\Staging\"
Write-Output "Copying XMLProducers dlls.."
# Publish bin folder for XMLProducers
xcopy /s /y "$binPath\UniversalXMLProducers" "$binPath\deploy\UniversalXMLProducers\"

# Publish transformed configuration for console projects
Write-Output "Building and transforming console projects.."
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\MessageDownloader\eHubMessageDownloader\eHubMessageDownloader.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\MessageProcessor\RuleRunner\RuleRunner.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\PushNotification\DataChangeCaptureNotification\DataChangeCaptureNotification.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\Schedulers\NewSchedulers\NewSchedulers.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\Schema\ApplicationConfig\ApplicationConfig.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\Schema\DbUpgrader\DbUpgrader.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\UniversalXmlProcessor\DataPublishingProcessor\DataPublishingProcessor.csproj
TransformJson $binPath\deploy\Staging\ $sourcePath\Staging\UniversalXmlProcessor\UniversalXMLStagingDataProcessor\UniversalXMLStagingDataProcessor.csproj

TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\Staging\Schema\ApplicationConfig\ApplicationConfig.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\Staging\UniversalXMLProducers\Common\Common.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\Staging\UniversalXMLProducers\DummyNet6Proj\DummyNet6Proj.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\Staging\UniversalXMLProducers\RefUNLOCOUpdater\RefUNLOCOUpdater.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\Staging\UniversalXMLProducers\RefUNLOCOUtcOffsetUpdater\RefUNLOCOUtcOffsetUpdater.csproj

TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\BE\BEReferenceData.CmdLine\BEReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\BR\BRReferenceData.CmdLine\BRReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\CA\CAReferenceData.CmdLine\CAReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\ES\ESReferenceData.CmdLine\ESReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\EU\EUNTariffDataProducer\EUNTariffDataProducer.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\EU\EUNTariffPopulator\EUNTariffPopulator.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\EU\EUReferenceData.CmdLine\EUReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\GB\GBReferenceData.CmdLine\GBReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\IHS\IHSReferenceData.CmdLine\IHSReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\IT\ITCustomsTariffRateProducer\ITCustomsTariffRateProducer.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\KR\KRReferenceData.CmdLine\KRReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\MX\MXReferenceData.CmdLine\MXReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\NL\NLReferenceData.CmdLine\NLReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\PL\PLReferenceData.CmdLine\PLReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\TR\TRReferenceData.CmdLine\TRReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\US\USReferenceData.CmdLine\USReferenceData.CmdLine.csproj
TransformJson $binPath\deploy\UniversalXMLProducers\ $sourcePath\UniversalXmlProducers\ZA\ZAReferenceData.CmdLine\ZAReferenceData.CmdLine.csproj

#Here we use the transformed files to create the default application configs.
& "$binPath\Tools\net8.0\CargoWise.RefDbRepo.ApplicationConfigurationGenerator.exe" "$binPath\deploy" "$binPath\StagingService\net8.0\"

if ($deployStagingService)
{
	# Deploy New Staging Service
	Write-Output "Deploying New Staging Service.."
	$authType = if ($updateServiceDeployUsername) { "Basic" } else  { "NTLM" }
	$updateServicePublishProfiles = Get-ChildItem $sourcePath\Staging\Service\NewService\Properties\PublishProfiles\$publishProfile*.pubxml -Name;
	foreach ($updateServicePublishProfile in $updateServicePublishProfiles)
	{
		& $msbuild "$sourcePath\Staging\Service\NewService\NewService.csproj" /t:"Restore;Build" /p:DeployOnBuild=True /p:UserName= `
		/p:AuthType=$authType /p:PublishProfile=$updateServicePublishProfile /p:AllowUntrustedCertificate=True /p:Configuration=Release `
		/p:IntermediateOutputPath=$binPath\deploy\NewStagingService\ /p:OutDir="$binPath\StagingService\net8.0" `
		/p:username=$updateServiceDeployUsername /p:password=`"$updateServiceDeployPassword`" `
		/p:PublishProfileRootFolder=$binPath\deploy\NewStagingService /p:ReferencePath="$binPath\StagingService\net8.0"

		if ($lastExitCode -ne 0) { Write-Error "New Staging Service Deployment failed" }

		RecycleAppPool "New Staging Service" "$sourcePath\Staging\Service\NewService\NewService.csproj" $updateServicePublishProfile $updateServiceDeployUsername $updateServiceDeployPassword "StartAppPool"
	}
}

if ($upgradeStagingDb)
{
	# Upgrade database
	if ($forceStagingDbUprade)
	{
		& "$binPath\deploy\Staging\net8.0\CargoWise.RefDbRepo.Staging.DbUpgrader.exe" -force
	}
	else
	{
		& "$binPath\deploy\Staging\net8.0\CargoWise.RefDbRepo.Staging.DbUpgrader.exe"
	}
}

if ($lastExitCode -ne 0) { Write-Error "Upgrade Staging database failed" }

Write-Output "Zipping Staging dlls.."
ZipFolder $binPath\deploy\Staging\
Write-Output "Zipping UniversalXMLProducers dlls.."
ZipFolder $binPath\deploy\UniversalXMLProducers\

function DeployStaging($server, $stagingDes, $xmlProducersDes)
{
	# Move to the server
	Write-Output "Moving Staging dlls to server.."
	DeployBinary "StagingDeploy" $stagingDes "$binPath\deploy\Staging.zip"

	Write-Output "Moving XMLProducers dlls to server.."
	DeployBinary "XMLProducersDeploy" $xmlProducersDes "$binPath\deploy\UniversalXMLProducers.zip"

	# Deploy scheduled task binary
	Write-Output "Extracting Staging dlls.."
	ExtractFolder $stagingDes "Staging.zip"

	Write-Output "Extracting XMLProducers dlls.."
	ExtractFolder $xmlProducersDes "UniversalXMLProducers.zip"

	if ($deployQuartz)
	{
		# Start Scheduler Service
		Write-Output "Start Scheduler Service on ${server}"
		Get-Service -ComputerName $server -Name $quartzServiceName | Start-Service
	}
	else
	{
		Write-Output "Quartz deployment is skipped"
	}

	RemovePSDrive "StagingDeploy"
	RemovePSDrive "XMLProducersDeploy"
}

foreach ($server in $stagingServer)
{
	$stagingDes = "\\${server}\${stagingDestination}"
	$xmlProducersDes = "\\${server}\${xmlProducersDestination}"
	DeployStaging $server $stagingDes $xmlProducersDes
}

Remove-Item "$binPath\deploy\Staging.zip" -Confirm:$false
Remove-Item "$binPath\deploy\UniversalXMLProducers.zip" -Confirm:$false

# Clear local publish folder
Write-Output "Clearing build folder.."
If (Test-Path "$binPath\deploy\")
{
  Remove-Item "$binPath\deploy\" -Recurse -Confirm:$false
}

Write-Output "End"
