@echo off

call SetMSBuild.cmd
%msbuild% "src\CargoWise.Billing.API\CargoWise.Billing.API.csproj" /t:GeneratePackage /p:Configuration=Release