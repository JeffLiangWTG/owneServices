REM Create a scheduled task "CleanScopedDirTempFolders" to run the C:\Selenium\Clean.ps1 script daily at 1:00 AM
schtasks /create /tn "CleanScopedDirTempFolders" /tr "powershell.exe -File C:\Selenium\Clean.ps1" /sc daily /st 01:00 /f

REM Query and list the details of the scheduled task "CleanScopedDirTempFolders"
schtasks /query /tn "CleanScopedDirTempFolders" /fo LIST /v
