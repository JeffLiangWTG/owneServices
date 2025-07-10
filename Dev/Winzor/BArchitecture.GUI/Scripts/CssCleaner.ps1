if (!$args[0]) {
	Write-Host "Please provide a path to the project directory"
	exit 1
}

$npmInstalled = Get-Command "npm" -erroraction 'silentlycontinue'

if (!$npmInstalled) {
	Write-Host "npm not installed - exiting."
	exit 0
}

$nodeVersion = node -v
if (!($nodeVersion.Contains("v18.") -or $nodeVersion.Contains("v20."))) {
	Write-Host "/**"
	Write-Host " *"
	Write-Host " * Node.js $nodeVersion is not supported - exiting. "
	Write-Host " * Please upgrade to the latest LTS version of Node.js. "
	Write-Host " * https://nodejs.org/en/download/package-manager"
	Write-Host " *"
	Write-Host " */"
	exit 0
}

$themes = $args[0]

$npmFolderResults = New-Item -ItemType Directory -Force -Path $env:APPDATA\npm
if (Test-Path env:DAT_IS_BUILDING) {
	npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- prettier@3.5 --write $themes/**/*.scss

	$gitResults = & { git status } 2>&1 3>&1

	if ($gitResults -like "*modified:   themes/*.scss") {
		Write-Host "/**"
		Write-Host " *"
		Write-Host " * Changes in WinzorFramework's SCSS files have been detected from Prettier."
		Write-Host " *"
		Write-Host " * Please build WinzorFramework locally with Node.js installed,"
		Write-Host " * then commit the changes."
		Write-Host " *"
		Write-Host " */"
		exit 1
	}
}
else {
	if (-not (Test-Path env:QGL_IS_BUILDING)) {
		npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- prettier@3.5 --write $themes/**/*.scss
	}
}
