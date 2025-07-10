@echo off
cd /d "%~dp0"

for /f "tokens=1,* delims==" %%A in (Paths.cfg) do (
    set "%%APath=%%B"
)
SETLOCAL
call :setESC
SET "ErrorCode=0"
SET "FilesCopied=0"

pushd "%CustomsPath%" >nul
set "CustomsAbsolutePath=%CD%"
popd >nul

pushd "%DevFolder%" >nul
set "DevAbsolutePath=%CD%"
popd >nul

call "%~dp0build.cmd" MessageContracts.PL %CustomsAbsolutePath%\PL\src\PL.sln
IF %ERRORLEVEL% NEQ 0 goto ErrorHandler
echo.
echo MessageContracts.PL built successfully
echo.

call "Copy MessageContracts.cmd" 
goto EndScript

:ErrorHandler
SET "ErrorCode=%ERRORLEVEL%"
echo.
echo !!! built failed !!!
echo.
goto EndScript

:EndScript
ENDLOCAL
exit /b %ErrorCode%