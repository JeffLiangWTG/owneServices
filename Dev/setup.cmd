@echo off
pushd %~dp0

IF [%1]==[] ( SET "Configuration=Debug" ) ELSE ( SET "Configuration=%1" )
echo Configuration: %Configuration%

echo ==== dotnet tool restore ====
dotnet tool restore
if %errorlevel% neq 0 goto :end

echo ==== dotnet tool run paket restore ====
dotnet tool run paket restore
if %errorlevel% neq 0 goto :end

echo ==== building NetCore\NetCore.PackageCopier ====
powershell -ExecutionPolicy Bypass -Command "if ((.\BuildFlags.ps1 -Name 'CW_BUILD_FLAG_DISABLE_NET_CORE') -ne 'true') { dotnet build NetCore\NetCore.PackageCopier\NetCore.PackageCopier.csproj -c %Configuration% }"
if %errorlevel% neq 0 goto :end

echo ==== building NetCore\NetCore.PackageCopier.Winzor ====
powershell -ExecutionPolicy Bypass -Command "if ((.\BuildFlags.ps1 -Name 'CW_BUILD_FLAG_DISABLE_WINZOR') -ne 'true') { dotnet build NetCore\NetCore.PackageCopier\NetCore.PackageCopier.Winzor.csproj -c %Configuration% }"
if %errorlevel% neq 0 goto :end

echo ==== execute signDeps.cmd ====
call signDeps.cmd
if %errorlevel% neq 0 goto :end

:end
popd
echo ==== setup.cmd end. errorlevel = %errorlevel% ====
exit /b %errorlevel%
