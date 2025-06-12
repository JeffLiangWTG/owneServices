pushd %~dp0

set BinDir=%~dp0Bin
set RelativePackageOutputPath=BAT
set PackageOutputPath=%BinDir%\%RelativePackageOutputPath%

call %BinDir%\Deployment\SetMSBuild.cmd
%msbuild% "%BinDir%\Deployment\BAT\Bat.proj" /t:bat:package:discover /p:Operator=datbackground

popd