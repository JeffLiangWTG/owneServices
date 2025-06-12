param(
	[string]$destServer,
	[string]$deployUser,
	[string]$deployPwd,
	[string]$binPath,
	[string]$deployBat,
	[string[]]$hosts
)

$ErrorActionPreference = 'Stop'

Set-PSDebug -Trace 2

$msdeploy = Join-Path $env:ProgramFiles "IIS\Microsoft Web Deploy V3\msdeploy.exe"

$getDeployFiles = (
	'-verb:sync',
	'-source:runCommand="DIR C:\eServices.Deploy\CargoWise.eHub.Assemblies\WI*.txt /B",waitAttempts=1,waitInterval=120000',
	-join ('-dest:auto,wmsvc=', $destServer, ',username="', $deployUser, '",password="', $deployPwd, '"'),
	'-allowUntrusted',
	'-verbose'
)
& $msdeploy $getDeployFiles | Tee-Object -Variable deployFiles
if (-not $?) { exit $LastExitCode }
$matches = $deployFiles | Select-String '^(?:Info\: )?((WI\d{8}).txt)$'

if ($matches)
{
	foreach($match in $matches)
	{
		$deployFile = $match.Matches.Groups[1].Value
		$workItem = $match.Matches.Groups[2].Value
		$deployDir = $(if ($null -ne $env:TIMESTAMP) { $env:TIMESTAMP.Replace(".","_") + '_' } else { [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmssZ_') }) + $workItem
		$commandFile = $workItem + '.deploy.bat'
		$localDeployDir = (Join-Path (Resolve-Path $binPath) $deployDir)
		$localDeployFile = (Join-Path $localDeployDir $deployFile)
		$localCommandFile = (Join-Path $localDeployDir $commandFile)
		if (Test-Path $localDeployDir) { Remove-Item -Path $localDeployDir -Recurse -Force }
		New-Item -Path $localDeployDir -ItemType Directory

		$getAsmList = (
			'-verb:sync',
			-join ('-source:filePath="C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployFile, '",wmsvc=', $destServer, ',username="', $deployUser, '",password="', $deployPwd, '"'),
			-join ('-dest:filePath="', $localDeployFile, '"'),
			'-allowUntrusted',
			'-verbose'
		)
		& $msdeploy $getAsmList
		if (-not $?) { exit $LastExitCode }

		foreach ($asm in Get-Content $localDeployFile) {
			Copy-Item -Path $(Join-Path $binPath $asm) -Destination $localDeployDir
		}

		Add-Content $localCommandFile (-join ('MD C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, '\Backup'))
		Add-Content $localCommandFile 'IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%'

		Add-Content $localCommandFile (-join ('powershell foreach ($dll in Get-ChildItem C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, ' *.dll) { $name = [System.Reflection.AssemblyName]::GetAssemblyName($dll.FullName).FullName; $asm = [System.Reflection.Assembly]::Load($name); if ($asm) { Copy-Item $asm.Location C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, '\Backup -Verbose } }'))
		Add-Content $localCommandFile 'IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%'

		Add-Content $localCommandFile (-join ('CALL C:\Deploy\Scripts\', $deployBat, ' C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, ' -Y'))
		Add-Content $localCommandFile 'IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%'

		foreach ($btssvc in $hosts) {
			Add-Content $localCommandFile (-join ('powershell Stop-Service ''', $btssvc, ''' -Force -Verbose; Start-Service ''', $btssvc, ''' -Verbose;'))
			Add-Content $localCommandFile 'IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%'
		}

		Add-Content $localCommandFile (-join ('DEL C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployFile))
		Add-Content $localCommandFile 'IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%'

		$copyDeployDir = (
			'-verb:sync',
			-join ('-source:dirPath="', $localDeployDir, '"'),
			-join ('-dest:dirPath="C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, '",wmsvc=', $destServer, ',username="', $deployUser, '",password="', $deployPwd, '"'),
			'-allowUntrusted',
			'-verbose'
			)
		& $msdeploy $copyDeployDir
		if (-not $?) { exit $LastExitCode }

		$runCommandFile = (
			'-verb:sync',
			-join ('-source:runCommand="C:\eServices.Deploy\CargoWise.eHub.Assemblies\', $deployDir, '\', $commandFile, '",waitAttempts=1,waitInterval=900000'),
			-join ('-dest:auto,wmsvc=', $destServer, ',username="', $deployUser, '",password="', $deployPwd, '"'),
			'-allowUntrusted',
			'-verbose'
		)
		& $msdeploy $runCommandFile
		if (-not $?) { exit $LastExitCode }
	}
}
