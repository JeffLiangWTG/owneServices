@ECHO OFF
ECHO ####################################################################
ECHO #                                                                  #
ECHO # Creates logical hosts within a BizTalk Group                     #
ECHO #                                                                  #
ECHO # Ensure the following variables are configured before continuing: #
ECHO #     BTInProcNTGroup                                              #
ECHO #                                                                  #
ECHO ####################################################################
pause

REM *LOCAL* SET BTInProcNTGroup=BizTalk Application Users
SET BTInProcNTGroup=CORPORATE\BizTalkHostUsersDev
REM *PROD* SET BTInProcNTGroup=CORPORATE\BizTalkHostUsers

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating receive hosts                                          #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_FILE -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_POP3 -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:true
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_FTP -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:true
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:InboxReceive -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:true
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:OutboxReceive -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:true
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:AlertSend -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating receive handlers                                       #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:FILE -HostName:ReceiveHost_FILE
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:POP3 -HostName:ReceiveHost_POP3
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:FTP -HostName:ReceiveHost_FTP
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-SQL -HostName:InboxReceive
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-SQL -HostName:OutboxReceive
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-SQL -HostName:AlertSend

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating send hosts                                             #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:SendHost_SMTP -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:SendHost_FTP -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:OutboxSend -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:InboxSend -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ErrorReceive -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false


ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating send handlers                                          #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:SMTP -HostName:SendHost_SMTP
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:FTP -HostName:SendHost_FTP
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:WCF-SQL -HostName:ErrorReceive
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:WCF-SQL -HostName:InboxSend
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:WCF-SQL -HostName:OutboxSend

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating tracking host                                          #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:TrackingHost -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:true -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe SetHostProperty -Property:HostTracking -Value:False -HostName:^^BizTalkServerApplication$

ECHO ###############################
ECHO #                             #
ECHO # Done                        #
ECHO #                             #
ECHO ###############################
pause