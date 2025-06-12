pushd %~dp0
"..\paket.exe" restore

xcopy /icry "..\packages\visualstudio2017\Microsoft.TeamFoundationServer.ExtendedClient\lib\net45\*.dll" "%~dp0..\Bin\"
xcopy /icry "..\packages\visualstudio2017\Microsoft.VisualStudio.Services.Client\lib\net45" "%~dp0..\Bin\"
xcopy /icry "..\packages\visualstudio2017\Microsoft.VisualStudio.Services.InteractiveClient\lib\net45" "%~dp0..\Bin\"
xcopy /icry "..\packages\visualstudio2017\Microsoft.TeamFoundationServer.Client\lib\net45" "%~dp0..\Bin\"

popd