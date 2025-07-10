@echo off
SETLOCAL

SET "devenvPath=c:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.com"
SET "msbuildPath=c:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"

SET "ProjectName=%~1"
SET "SolutionPath=%~2"

echo.
echo Rebuilding the %ProjectName% solution...
"%msbuildPath%" "%SolutionPath%" /restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform="Any CPU"
IF %ERRORLEVEL% NEQ 0 goto ErrorHandler
echo.
echo Successfully built project: %ProjectName%
goto EndScript

:ErrorHandler
SET "ErrorCode=%ERRORLEVEL%"
echo.
echo Solution %ProjectName% rebuild failed. Please check the errors.
echo Would you like to open the %ProjectName% in Visual Studio? (Y/N)
choice /C YN /N
IF %ERRORLEVEL% EQU 1 (
    "%devenvPath%" "%SolutionPath%"
)
exit /b %ErrorCode%

:EndScript
ENDLOCAL