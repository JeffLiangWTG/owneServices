@ECHO.
@ECHO   %~n0
@ECHO.

@OPENFILES > NUL 2>&1
@IF %ERRORLEVEL% NEQ 0 (
	ECHO ERROR: Must run as administrator. 1>&2
	GOTO :END
)
@IF '%1' EQU '' (
	ECHO ERROR: Can only deploy directories.
	GOTO :END
)
@IF NOT EXIST %~s1\NUL (
	ECHO ERROR: Can only deploy directories.
	GOTO :END
)
@IF '%1' NEQ '' (
	ECHO Deploying from: %1
	ECHO.
)

@IF /I '%2' EQU '-Y' GOTO :DEPLOY

@SET /P "Y=Enter 'Y' to continue: "
@IF /I '%Y%' NEQ 'Y' (
	ECHO.
	ECHO   Cancelled
	GOTO :END
)

:DEPLOY
@FOR /F "tokens=1,3" %%a in ('REG QUERY "HKLM\Software\Microsoft\Biztalk Server\3.0\Administration"') DO @(
 SET "%%~a=%%~b"
)
@SET YYYYMMDD=%DATE:~10,4%%DATE:~7,2%%DATE:~4,2%
@SET HHMMSS=%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%%TIME~9,2%
@SET MSBuild=c:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0CargoWise.eHub.BizTalk.DeployRelease.buildproj"
@SET Properties=/p:BuildDir=%1;ServerName="%MgmtDBServer%"
@SET LogFile=%~n0.%YYYYMMDD%-%HHMMSS%.%USERNAME%.log
@IF '%1' NEQ '' (
	SET Logger=/l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;append;logfile="%~1\%LogFile%"
) ELSE (
	IF NOT EXIST C:\Deploy\%YYYYMMDD% MD C:\Deploy\%YYYYMMDD%
	SET Logger=/l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;append;logfile="C:\Deploy\%YYYYMMDD%\%LogFile%"
)

%MSBuild% %Properties% %Logger% /t:AddAssemblies;ImportBindings

:END
@ECHO.
@IF /I '%2' NEQ '-Y' @PAUSE
