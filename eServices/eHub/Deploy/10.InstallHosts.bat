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
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_WCFNetTcp -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_WCFCustom -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_WCFSQL -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:ReceiveHost_POP3 -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:true

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating receive handlers                                       #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:FILE -HostName:ReceiveHost_FILE
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-NetTcp -HostName:ReceiveHost_WCFNetTcp
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-Custom -HostName:ReceiveHost_WCFCustom
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:WCF-SQL -HostName:ReceiveHost_WCFSQL
BizTalk.Utilities.GroupAdmin.exe CreateReceiveHandler -AdapterName:POP3 -HostName:ReceiveHost_POP3

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating send hosts                                             #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:SendHost_SMTP -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:SendHost_SQL -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false
BizTalk.Utilities.GroupAdmin.exe CreateHost -HostName:SendHost_WCFSQL -HostType:InProcess -NTGroup:"%BTInProcNTGroup%" -ForTracking:false -Trusted:false -32Bit:false

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating send handlers                                          #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:SMTP -HostName:SendHost_SMTP
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:SQL -HostName:SendHost_SQL
BizTalk.Utilities.GroupAdmin.exe CreateSendHandler -AdapterName:WCF-SQL -HostName:SendHost_WCFSQL

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