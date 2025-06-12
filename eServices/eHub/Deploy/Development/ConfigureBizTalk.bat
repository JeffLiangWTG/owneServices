@net session 1>nul 2>&1
@if errorLevel 1 goto :NotAdmin

cd /d %~dp0
c:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ConfigureBizTalk.buildproj /l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;append;logfile=ConfigureBizTalk.log
goto :End

:NotAdmin
@echo.
@echo Please run as Administrator.
@echo.

:End
@pause


