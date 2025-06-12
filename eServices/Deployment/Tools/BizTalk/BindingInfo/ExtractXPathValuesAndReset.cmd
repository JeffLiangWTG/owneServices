@echo off
REM
REM This cmd will call the Scripts\ResetXpathValues.ps1 to extract XPath values as defined by the Templates\XPath-Bindings.json file
REM and reset the values to empty string. The extracted values are saved to a json file named after the xml file, so that it 
REM can be used afterwards by the buildBindings to set them up to a given profile
REM

cd /d %~dp0

call "PaketRestore.cmd"
del /Q /F "%~dp0..\Bin\Bindings"

powershell -ExecutionPolicy RemoteSigned -File "%~dp0\Scripts\ResetXpathValues.ps1" -Folder "%~1" -BinDir "%~dp0\Templates\XPath-Bindings.json"

pause