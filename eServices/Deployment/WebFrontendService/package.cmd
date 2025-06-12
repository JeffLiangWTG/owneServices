call copy Bin\Deployment\WebFrontendService\Package.proj dist\package.uiproj

set RootDir=%~dp0\..\..\..\
set ProjectId=%1
call %RootDir%Bin\Deployment\SetMSBuild.cmd
%msbuild% "%RootDir%dist\package.uiproj" /t:bat:package /p:Operator=datbackground;ProjectId=%ProjectId%

cmd /c exit 0