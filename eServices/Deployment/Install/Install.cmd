pushd %~dp0

set localPath=%cd%
powershell -command "& %localPath%\Install.ps1"
popd