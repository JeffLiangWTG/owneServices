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

$wwwRoot = $args[0]
$eslintConfig = $args[1]

$npmFolderResults = New-Item -ItemType Directory -Force -Path $env:APPDATA\npm
if (Test-Path env:DAT_IS_BUILDING) {
	npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- prettier@3.5 --write --config $wwwRoot/js/.prettierrc.json $wwwRoot/js/module/*.js $wwwRoot/*.js | Tee-Object -Variable prettierResults
	npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- eslint@9.20 --max-warnings=0 -c $eslintConfig $wwwRoot/js/module/*.js $wwwRoot/*.js | Tee-Object -Variable eslintResults
	
	$gitResults = & { git status } 2>&1 3>&1

	if ($gitResults -like "*modified:   wwwroot/js/*.js") {
		Write-Host "/**"
		Write-Host " *"
		Write-Host " * Changes in WinzorFramework have been detected from Prettier and/or ESLint."
		Write-Host " *"
		Write-Host " * Please build WinzorFramework locally with Node.js installed,"
		Write-Host " * then commit the changes."
		Write-Host " *"
		Write-Host " */"
		Write $gitResults
		exit 1
	}
}
else {
	if (-not (Test-Path env:QGL_IS_BUILDING)) {
		npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- prettier@3.5 --write --config $wwwRoot/js/.prettierrc.json $wwwRoot/js/module/*.js $wwwRoot/*.js | Tee-Object -Variable prettierResults
		npm exec --yes --registry http://proget.wtg.zone/npm/Registry/ -- eslint@9.20 --fix --max-warnings=0 -c $eslintConfig $wwwRoot/js/module/*.js $wwwRoot/*.js | Tee-Object -Variable eslintResults
	}
}

if (!$eslintResults) {
	Write-Host "No errors found in the JavaScript files found by eslint."
	exit 0
}

if (!$eslintResults.Contains("(0 errors, 0 warnings)")) {
	Write-Host "/**"
	Write-Host " *"
	Write-Host " * Errors found in the JavaScript files in WinzorFramework"
	Write-Host " * Please fix them before committing"
	Write-Host " *"
	Write-Host " */"
	exit 1
}
