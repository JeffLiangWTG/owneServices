@ECHO OFF
SET BTServer=syd-thub-1

REM Receive Host Instances
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:ReceiveHost_FILE
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:ReceiveHost_WCFNetTcp
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:ReceiveHost_WCFCustom
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:ReceiveHost_WCFSQL
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:ReceiveHost_POP3

REM Send Host Instances
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:SendHost_SMTP
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:SendHost_SQL
BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:SendHost_WCFSQL

BizTalk.Utilities.GroupAdmin.exe DeleteHostInstance -Server:%BTServer% -HostName:TrackingHost

pause