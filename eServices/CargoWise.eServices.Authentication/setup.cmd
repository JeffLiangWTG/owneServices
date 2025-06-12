@echo off
pushd %~dp0

echo ==== dotnet tool restore ====
dotnet tool restore
if %errorlevel% neq 0 goto :end

echo ==== dotnet tool run paket restore ====
dotnet tool run paket restore
if %errorlevel% neq 0 goto :end

:end
popd
exit /b %errorlevel%
