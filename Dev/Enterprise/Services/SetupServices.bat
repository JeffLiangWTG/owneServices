@echo off

set ERROR=0
set SOURCE=..\..\bin
set TARGET=..\..\Enterprise\Services\ServiceHost\bin

if NOT EXIST "%TARGET%" md "%TARGET%"

call :copy Common.Logging
call :copy Spring.Core
call :copy antlr.runtime

call :copy CargoWise.ApplicationContext
call :copy CargoWise.BrandManager
call :copy CargoWise.Common
call :copy CargoWise.ComponentModel
call :copy CargoWise.Data
call :copy CargoWise.Data.HttpClient
call :copy CargoWise.Data.Providers.Common
call :copy CargoWise.EntityFramework
call :copy CargoWise.Integration
call :copy CargoWise.Interop
call :copy CargoWise.IO
call :copy CargoWise.ResourceStrings.Cache
call :copy CargoWise.Schema
call :copy CargoWise.Types
call :copy CargoWise.Windows.UI
call :copy Enterprise.DataTransfer.Native.Integration
call :copy Enterprise.DocumentEngine
call :copy Enterprise.DocumentEngineCore
call :copy Enterprise.DocumentEngineIntegration
call :copy Enterprise.Environment
call :copy Enterprise.Initialisation
call :copy Enterprise.Integration
call :copy Enterprise.Licensing.Core
call :copy Enterprise.MasterFiles.Business
call :copy Enterprise.MasterFiles.Integration
call :copy Enterprise.Messaging.Integration
call :copy Enterprise.Registry.Business
call :copy Enterprise.ResourceStrings.Business
call :copy Enterprise.Security
call :copy Enterprise.Security.Core
call :copy Enterprise.Semaphores.Common
call :copy Enterprise.Services.ServiceHost
call :copy Enterprise.UniversalDataBuss.Integration
call :copy Enterprise.ZArchitecture.Business
call :copy Enterprise.ZArchitecture.Core
call :copy Enterprise.ZArchitecture.GUI
call :copy Enterprise.ZArchitecture.Modules
call :copy Enterprise.ZArchitecture.Schema
call :copy Enterprise.ZArchitecture.Web.Utilities
call :copy NUnitCore
call :copy Resources

goto :done

:copy

copy /y "%SOURCE%\%1.dll" "%TARGET%" > NUL
if NOT ERRORLEVEL 1 goto :LBL_5F72FA9C-539D-41CF-A91B-44519CB61CA5

set ERROR=1
echo Failed to copy "%1.dll"

:LBL_5F72FA9C-539D-41CF-A91B-44519CB61CA5

copy /y "%SOURCE%\%1.pdb" "%TARGET%" > NUL

goto :EOF

:done

if "%ERROR%"=="1" pause
