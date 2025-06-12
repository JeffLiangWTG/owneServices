pushd %~dp0

set BinDir=%~dp0Bin

call %BinDir%\Deployment\SetMSBuild.cmd
%msbuild% "%BinDir%\Deployment\BAT\Bat.proj"

popd