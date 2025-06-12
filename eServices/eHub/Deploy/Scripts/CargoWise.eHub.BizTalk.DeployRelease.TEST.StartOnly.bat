@IF '%1' NEQ '' IF NOT EXIST %~s1\NUL (
	@ECHO Can only deploy directories.
	GOTO :END
)

@SET MSBuild=c:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0CargoWise.eHub.Biztalk.DeployRelease.buildproj"
@SET Properties=/p:BuildDir=%1;ServerName="%ServerName%";ExcludeHosts="%ExcludedHosts%"
@IF '%1' NEQ '' (SET Logger=/l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;append;logfile="%~1\CargoWise.eHub.BizTalk.DeployRelease.log") ELSE (SET Logger=/v:diag)

%MSBuild% %Properties% %Logger% /t:StartHostInstances

:END
@PAUSE
