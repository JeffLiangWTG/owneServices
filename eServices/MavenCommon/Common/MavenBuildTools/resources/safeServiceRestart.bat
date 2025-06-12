@echo off

if [%1]==[] GOTO usage
if [%2]==[] GOTO usage

HOSTNAME | FIND "%1" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO RestartServiceFromLocalhost
echo %1 | FIND "%COMPUTERNAME%" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO RestartServiceFromLocalhost
echo %1 | FIND "localhost" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO RestartServiceFromLocalhost
echo %1 | FIND "127.0.0.1" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO RestartServiceFromLocalhost
for /f "tokens=1-2 delims=:" %%a in ('ipconfig^|find "IPv4"') do set ip=%%b
set ip=%ip:~1%
echo %1 | FIND "%ip%" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO RestartServiceFromLocalhost
ping -n 1 %1 | FIND "Received = 1" >NUL
IF errorlevel 1 GOTO SystemOffline
SC \\%1 query %2 | FIND "STATE" >NUL
IF errorlevel 1 GOTO SystemOffline

:ResolveInitialState
SC \\%1 query %2 | FIND "STATE" | FIND "RUNNING" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO StopService
SC \\%1 query %2 | FIND "STATE" | FIND "STOPPED" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO StartService
SC \\%1 query %2 | FIND "STATE" | FIND "PAUSED" >NUL
IF errorlevel 0 IF NOT errorlevel 1 GOTO SystemOffline
echo Service State is changing, waiting for service to resolve its state before making changes
sc \\%1 query %2 | Find "STATE"
:: timeout does not work so use ping instead
ping 127.0.0.1 -n 2 > nul
GOTO ResolveInitialState

:StopService
echo Stopping %2 on \\%1
sc \\%1 stop %2 >NUL
if %errorlevel% neq 0 GOTO SystemOffline
GOTO StopingService

:StartService
echo Starting %2 on \\%1
sc \\%1 start %2 >NUL
if %errorlevel% neq 0 GOTO SystemOffline
GOTO StartingService

:StopingServiceDelay
echo Waiting for %2 to stop
:: timeout does not work so use ping instead
ping 127.0.0.1 -n 3 > nul

:StopingService
SC \\%1 query %2 | FIND "STATE" | FIND "STOPPED" >NUL
IF errorlevel 1 GOTO StopingServiceDelay

:StopedService
echo %2 on \\%1 is stopped
GOTO StartService


:StartingServiceDelay
echo Waiting for %2 to start
:: timeout does not work so use ping instead
ping 127.0.0.1 -n 3 > nul
:StartingService
SC \\%1 query %2 | FIND "STATE" | FIND "RUNNING" >NUL
IF errorlevel 1 GOTO StartingServiceDelay

:StartedService
echo %2 on \\%1 is started
GOTO:eof

:SystemOffline
echo [ERROR] Server \\%1 or service %2 is not accessible or is offline
exit /b 1
GOTO:eof

:RestartServiceFromLocalhost
echo Found %1 is localhost. Execute NET.exe
echo NET stop %2
NET stop %2 
if %errorlevel% neq 0 GOTO SystemOffline
echo NET start %2
NET start %2
if %errorlevel% neq 0 GOTO SystemOffline
:: timeout does not work so use ping instead, need to wait the server to warm up
ping 127.0.0.1 -n 10 > nul
GOTO:eof

:usage
echo Will restart a service, waiting for the service to stop/start (if necessary)
echo.
echo %0 [system name] [service name]
echo Example: %0 localhost Tomcat8
echo.
echo Run: safeServiceRestart.bat "%1" "%2"
echo [ERROR] Invalid arguments: safeServiceRestart.bat "%1" "%2"
exit /b 1
GOTO:eof