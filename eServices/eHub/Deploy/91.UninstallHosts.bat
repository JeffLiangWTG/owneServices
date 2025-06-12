@ECHO OFF

REM Receive Handlers
BizTalk.Utilities.GroupAdmin.exe DeleteReceiveHandler -AdapterName:FILE -HostName:ReceiveHost_FILE
BizTalk.Utilities.GroupAdmin.exe DeleteReceiveHandler -AdapterName:WCF-NetTcp -HostName:ReceiveHost_WCFNetTcp
BizTalk.Utilities.GroupAdmin.exe DeleteReceiveHandler -AdapterName:WCF-Custom -HostName:ReceiveHost_WCFCustom
BizTalk.Utilities.GroupAdmin.exe DeleteReceiveHandler -AdapterName:WCF-SQL -HostName:ReceiveHost_WCFSQL
BizTalk.Utilities.GroupAdmin.exe DeleteReceiveHandler -AdapterName:POP3 -HostName:ReceiveHost_POP3

REM Receive Hosts
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:ReceiveHost_FILE
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:ReceiveHost_WCFNetTcp
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:ReceiveHost_WCFCustom
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:ReceiveHost_WCFSQL
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:ReceiveHost_POP3

REM Send Handlers
BizTalk.Utilities.GroupAdmin.exe DeleteSendHandler -AdapterName:SMTP -HostName:SendHost_SMTP
BizTalk.Utilities.GroupAdmin.exe DeleteSendHandler -AdapterName:SQL -HostName:SendHost_SQL
BizTalk.Utilities.GroupAdmin.exe DeleteSendHandler -AdapterName:WCF-SQL -HostName:SendHost_WCFSQL

REM Send Hosts
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:SendHost_SMTP
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:SendHost_SQL
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:SendHost_WCFSQL

REM Tracking
BizTalk.Utilities.GroupAdmin.exe SetHostProperty -Property:HostTracking -Value:True -HostName:^^BizTalkServerApplication$
BizTalk.Utilities.GroupAdmin.exe DeleteHost -HostName:TrackingHost

pause