@echo off
pushd %~dp0

:: copy files for ResourceStrings
copy .\bin\*.zrs .\bin\net8.0\
copy .\bin\Enterprise.ResourceStrings.InversionMapping .\bin\net8.0\
copy .\bin\Warehouse.RF.ResourceStrings.*.xml .\bin\net8.0\
