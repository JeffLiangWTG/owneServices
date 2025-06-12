ROBOCOPY Bin\BizTalkAdapters\Build Bin\BizTalkAdapters\Publish\bin /NP /NJS
IF %ERRORLEVEL% GTR 7 EXIT /B %ERRORLEVEL%
ROBOCOPY BizTalkAdapters\Setup Bin\BizTalkAdapters\Publish\setup /NP /NJS
IF %ERRORLEVEL% GTR 7 EXIT /B %ERRORLEVEL%
EXIT /B 0
