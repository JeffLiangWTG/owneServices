@echo off

call SetMSBuild.cmd

set outputPath=%~dp0bin

%msbuild% src\CargoWise.Billing.API\CargoWise.Billing.API.csproj /t:GeneratePackage /p:Configuration=Release /p:PackageOutputPath=%outputPath%