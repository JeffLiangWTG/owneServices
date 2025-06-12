@ECHO OFF
ECHO ###################################################################
ECHO #                                                                 #
ECHO # Disables artefact tracking on all:                              #
ECHO #     Orchestrations                                              #
ECHO #     Pipelines                                                   #
ECHO #     Receive Ports                                               #
ECHO #     Send Ports                                                  #
ECHO #                                                                 #
ECHO ###################################################################
pause

BizTalk.Utilities.GroupAdmin.exe SetOrchTracking -Options:None -TypeName:[a-zA-Z]
BizTalk.Utilities.GroupAdmin.exe SetPipelineTracking -Options:None -TypeName:[a-zA-Z]
BizTalk.Utilities.GroupAdmin.exe SetReceivePortTracking -Options:None -PortName:[a-zA-Z]
BizTalk.Utilities.GroupAdmin.exe SetSendPortTracking -Options:None -PortName:[a-zA-Z]

ECHO ###############################
ECHO #                             #
ECHO # Done                        #
ECHO #                             #
ECHO ###############################
pause