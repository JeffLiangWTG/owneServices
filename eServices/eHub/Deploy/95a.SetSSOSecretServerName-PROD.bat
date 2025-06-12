@ECHO OFF
ECHO THIS SCRIPT IS NO LONGER REQUIRED FOR DEPLOYMENT TO A SINGLE SQL SERVER
ECHO ####################################################################
ECHO #                                                                  #
ECHO # Sets the Master Secret Server name to the required DNS alias     #
ECHO #   instead of using the NETBIOS name                              #
ECHO #                                                                  #
ECHO # This script should be run on the Master Secret Server (probably  #
ECHO #   the SQL Server)                                                #
ECHO #                                                                  #
ECHO ####################################################################
pause

REM "C:\Program Files\Common Files\Enterprise Single Sign-On\ssomanage.exe" -server %COMPUTERNAME%
REM "C:\Program Files\Common Files\Enterprise Single Sign-On\ssomanage.exe" -updatedb .\95a.SSOSecretServerUpdate-PROD.xml
REM net stop entsso
REM net start entsso
pause