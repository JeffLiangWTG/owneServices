"%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" remove-view -Name:eHubGatewayLogView
"%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" remove-activity -Name:eHubGatewayLogActivity

"%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" deploy-all -DefinitionFile:.\BAMActivity.xml
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" remove-all -DefinitionFile:.\BAMActivity.xml
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" update-all -DefinitionFile:.\BAMActivity.xml
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" deploy-all -DefinitionFile:"C:\%PROGRAMFILES(X86)%\Microsoft BizTalk ESB Toolkit 2.0\Bam\Microsoft.BizTalk.ESB.BAM.Exceptions.xml"

"%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" add-account -AccountName:"%COMPUTERNAME%\BizTalk Server Administrators" -View:eHubGatewayLogView
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" add-account -AccountName:"%COMPUTERNAME%\BizTalk Server Administrators" -View:ExcAll
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" add-account -AccountName:"%COMPUTERNAME%\BizTalk Server Administrators" -View:ExcByApplication

"%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" set-activitywindow -Activity:eHubGatewayLogActivity -TimeLength:30 -TimeUnit:Day
REM "%PROGRAMFILES(X86)%\Microsoft BizTalk Server 2013 R2\Tracking\bm.exe" set-activitywindow -Activity:EsbExceptions -TimeLength:6 -TimeUnit:Month

pause