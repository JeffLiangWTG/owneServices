PUSHD Web\ReferenceDataUpdateService.Web

ECHO Installing Modules...
CALL npm ci

IF %ERRORLEVEL% NEQ 0 (
	ECHO Failed to install node modules, try again
	CALL timeout 30
	ECHO Installing Modules...
	CALL npm ci
)

IF %ERRORLEVEL% NEQ 0 (
	ECHO Failed to install node modules, try again
	CALL timeout 30
	ECHO Installing Modules...
	CALL npm ci
)

IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%

POPD
EXIT /B 0
