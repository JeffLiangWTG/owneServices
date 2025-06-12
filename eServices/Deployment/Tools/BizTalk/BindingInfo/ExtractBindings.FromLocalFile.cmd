@echo off
REM
REM This cmd will call the Scripts\ExtractBindings.ps1 to extract bindings from the specified bindings xml file
REM and output the results to the "Bin\Bindings" sub-folder

set BINDINGS=%~1
echo "%BINDINGS%"

pushd

REM if no bindings file provided, assume a file from %localappdata% as below
if "%BINDINGS%" == "" (
    set BINDINGS="%localappdata%\BizTalk\Bindings Files\CargoWise.eHub.Products.BindingInfo.xml"
    echo %BINDINGS%
)

if exist "%BINDINGS%" (

    cd /d %~dp0
    call "PaketRestore.cmd"
    del /Q /F "%~dp0..\Bin\Bindings"

    powershell -ExecutionPolicy RemoteSigned -File "%~dp0\Scripts\ExtractBindings.ps1" %BINDINGS% -OutputDir "%~dp0..\Bin\Bindings" -BinDir "%~dp0..\Bin"
    powershell -ExecutionPolicy RemoteSigned -File "%~dp0\Scripts\ResetXpathValues.ps1" -Folder "%~dp0..\Bin\Bindings" -XPathJsonFile "%~dp0Templates\XPath-Bindings.json"
)

popd
pause