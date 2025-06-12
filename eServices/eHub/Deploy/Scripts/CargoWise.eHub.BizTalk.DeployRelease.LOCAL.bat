@IF '%1' NEQ '' IF NOT EXIST %~s1\NUL (
	@ECHO Can only deploy directories.
	GOTO :END
)

@SET MSBuild=c:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0CargoWise.eHub.Biztalk.DeployRelease.buildproj"
@SET ServerName=localhost
@SET ExcludedHosts=ReceiveHost_FILE;ReceiveHost_FTP;ReceiveHost_FTPEx;ReceiveHost_FTPEx2;ReceiveHost_POP3;ReceiveHost_SFTPEx;ReceiveHost_SFTPEx2;ReceiveHost_WCFSQL;SendHost_FTP;SendHost_FTPEx;SendHost_FTPEx2;SendHost_SFTPEx;SendHost_SFTPEx2;SendHost_SMTP;SendHost_SQL;SendHost_WCF;SendHost_WCFSQL;DISABLED
@SET Properties=/p:BuildDir=%1;ServerName="%ServerName%";ExcludeHosts="%ExcludedHosts%";ScriptDir=%~dp0
@IF '%1' NEQ '' (SET Logger=/l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;append;logfile="%~1\CargoWise.eHub.BizTalk.DeployRelease.log") ELSE (SET Logger=/v:diag)

%MSBuild% %Properties% %Logger% /t:AddAssemblies;ImportBindings

:END
@PAUSE