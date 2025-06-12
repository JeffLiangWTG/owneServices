@ECHO OFF
ECHO ###################################################################
ECHO #                                                                 #
ECHO # Stops and then starts Receive*, Send* host instances and the    #
ECHO #     default BizTalkServerApplication host instance              #
ECHO #                                                                 #
ECHO ###################################################################
pause

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Stopping hosts instances                                        #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Stopped -HostInstanceName:^^Receive -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Stopped -HostInstanceName:^^Send -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Stopped -HostInstanceName:^^BizTalkServerApplication$ -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Stopped -HostInstanceName:^^TrackingHost$ -Server:[A-Za-z0-9]

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Starting hosts instances                                        #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Started -HostInstanceName:^^Receive -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Started -HostInstanceName:^^Send -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Started -HostInstanceName:^^BizTalkServerApplication$ -Server:[A-Za-z0-9]
BizTalk.Utilities.GroupAdmin.exe SetHostInstanceState -State:Started -HostInstanceName:^^TrackingHost$ -Server:[A-Za-z0-9]

ECHO ###############################
ECHO #                             #
ECHO # Done                        #
ECHO #                             #
ECHO ###############################
pause