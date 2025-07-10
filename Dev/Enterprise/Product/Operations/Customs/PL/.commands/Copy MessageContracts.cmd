@echo off
cd /d "%~dp0"

for /f "tokens=1,* delims==" %%A in (Paths.cfg) do (
    set "%%APath=%%B"
)
SETLOCAL
SET "ErrorCode=0"
SET "FilesCopied=0"

pushd "%CustomsPath%" >nul
set "CustomsAbsolutePath=%CD%"
popd >nul

pushd "%DevPath%" >nul
set "DevAbsolutePath=%CD%"
popd >nul

SET "CustomsPathToBin=%CustomsAbsolutePath%\PL\Bin\net48\"
SET "DevPathToBin=%DevAbsolutePath%\Bin"
echo.
echo Copying MessageContracts.PL binaries files from "%CustomsPathToBin%" to "%DevPathToBin%"
echo.

for %%f in ("%CustomsPathToBin%CargoWise.Customs.PL.MessageContracts*.dll" "%CustomsPathToBin%CargoWise.Customs.PL.MessageContracts*.pdb" "%CustomsPathToBin%CargoWise.Customs.PL.MessageDefinition*.dll" "%CustomsPathToBin%CargoWise.Customs.PL.MessageDefinition*.pdb") do (
    echo %%f | findstr /I /V ".Test" > nul
    if not errorlevel 1 (
        echo Copying "%%~nxf"
        copy /Y "%%~ff" "%DevPathToBin%"
        if errorlevel 1 (
            echo Error copying "%%~nxf".
            SET "ErrorCode=1"
        ) else (
            SET /A "FilesCopied+=1"
        )
    )
)

if %FilesCopied% EQU 0 (
    echo.
    echo ERROR: No files were found for copying!
    SET "ErrorCode=1"
    goto ErrorHandler
)

echo.
echo MessageContracts.PL copied successfully
echo.
goto EndScript

:ErrorHandler
SET "ErrorCode=%ERRORLEVEL%"
echo.
echo !!! Copying failed !!!
echo.
goto EndScript

:EndScript
ENDLOCAL
exit /b %ErrorCode%