"%MSDEPLOY%" -verb:sync -source:runCommand="md %DEPLOY_STAGING_PATH%& compact /c /s %DEPLOY_STAGING_PATH%\",waitAttempts=1,waitInterval=30000 -dest:auto,%DEST_CONNECTION% -allowUntrusted -verbose
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
"%MSDEPLOY%" -verb:sync -source:dirPath="%DAT_BIN_PATH%\BizTalkAdapters\Publish" -dest:dirPath="%DEPLOY_STAGING_PATH%\Deploy",%DEST_CONNECTION% -allowUntrusted -verbose
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
"%MSDEPLOY%" -verb:sync -source:runCommand="powershell -NonInteractive -File %DEPLOY_STAGING_PATH%\Deploy\setup\Deploy-BizTalkAdapters-Install.ps1 %DEPLOY_STAGING_PATH%\Deploy\bin %DEPLOY_STAGING_PATH%\Backup",waitAttempts=1,waitInterval=600000 -dest:auto,%DEST_CONNECTION% -allowUntrusted -verbose
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
