@echo off

if (%1) == ()  (%2) == ()  (%3) == () goto noparam

rem ================================================================
rem
rem Reading variables
rem
rem ================================================================
set SourceDir=%1
if (%SourceDir:~-1%) == (\) set SourceDir=%SourceDir:~0,-1%

set DestDir=%2
if (%DestDir:~-1%) == (\) set DestDir=%DestDir:~0,-1%

set NewDrive=%3
set User=%4
set Password=%5

if (%DestDir:~0,2%) == (\\) (set IsUNC=1) else (set IsUNC=0)

rem ================================================================
rem
rem Checking the parameters
rem
rem ================================================================
echo.
echo PARAMETERS:
echo ================================================================
echo Source path                      : %SourceDir%
echo Destination path                 : %DestDir%
echo Is Destination path in UNC format: %IsUNC%
echo User                             : %User%
echo Password                         : %password%
echo ================================================================
echo.

rem ================================================================
rem
rem Mapping a network drive if required
rem
rem ================================================================
if %IsUNC% == 1 (
	echo Mapping network drive...Conneting to %DestDir%...
	if NOT %User% == "" (
		net use %NewDrive%: %DestDir% %Password% /user:%User%
	) else (
		net use %NewDrive%: %DestDir%
	)	
) else (
	echo Destination folder is on the same computer. No mapping required
)

rem ================================================================
rem
rem Removing all files (first step) and folders (second step) at the destination folder
rem
rem ================================================================
del /f /q /s %DestDir%\*.*

for /d %%i in (%DestDir%) do rmdir /s /q %%i

rem ================================================================
rem
rem Unpacking files from EnterpriseWeb ZIP package 
rem
rem ================================================================
ediunzip %SourceDir%\EnterpriseWebDeploy.zip %DestDir%

rem ================================================================
rem
rem Copying files from Enterprise application folder
rem
rem ================================================================
copy %SourceDir%\*.dll %DestDir%\Bin\


rem ================================================================
rem
rem Un-Mapping a network drive if required
rem
rem ================================================================
if %IsUNC% == 1 (
	echo Un-Mapping network drive
	echo.
	net use /delete %newDrive%:
)

goto exit

:noparam
echo Please specify correct parameters
echo USAGE: EnterpriseWebDeploy.bat [SourceDir] [DestinationDir] [NewDriveMapping] [UserName] [Password]
echo   SourceDir       - source folder to copy files from (usually, Enterprise\Distribution\Application folder)
echo   DestinationDir  - destination folder to copy the web application to
echo   NewDriveMapping - if Desitnation folder is UNC path, you need to specify a letter to map the drive to it
echo   UserName        - if remote computer requires user name to be specified
echo   Password        - if remote computer requires password for the user to be specified
echo.
goto exit

:exit 
echo.

