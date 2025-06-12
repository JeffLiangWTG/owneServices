@ECHO OFF
ECHO ####################################################################
ECHO #                                                                  #
ECHO # Creates host instance services for a BizTalk server in the Group #
ECHO #                                                                  #
ECHO # Ensure the following variables are configured before continuing: #
ECHO #     BTServer                                                     #
ECHO #     ServiceLogin                                                 #
ECHO #     ServiceLoginPwd                                              #
ECHO #                                                                  #
ECHO ####################################################################
pause

REM *NETBIOS name*
SET BTServer=%COMPUTERNAME%

REM *LOCAL* ServiceLogin=CORPORATE\ehubservice
SET ServiceLogin=CORPORATE\biztalkservicedev
REM *PROD*  SET ServiceLogin=**ChangeMe**CORPORATE\biztalkservice

SET ServiceLoginPwd=Z@hQBn9$

ECHO ******** Are you sure the login and password are correct? *********
pause

ECHO *************************** Last chance ***************************
ECHO ** Are you really sure you wont lock the %ServiceLogin% account? **
pause

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating receive host instances                                 #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:ReceiveHost_FILE -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:ReceiveHost_POP3 -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:ReceiveHost_FTP -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:InboxReceive -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:OutboxReceive -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:AlertSend -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating send host instances                                    #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:SendHost_SMTP -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:SendHost_FTP -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:OutboxSend -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:InboxSend -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:ErrorReceive -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%

ECHO ###################################################################
ECHO #                                                                 #
ECHO # Creating tracking host instance                                 #
ECHO #                                                                 #
ECHO ###################################################################
BizTalk.Utilities.GroupAdmin.exe CreateHostInstance -Server:%BTServer% -HostName:TrackingHost -UserName:%ServiceLogin% -Password:%ServiceLoginPwd%

ECHO ###############################
ECHO #                             #
ECHO # Done                        #
ECHO #                             #
ECHO ###############################
pause