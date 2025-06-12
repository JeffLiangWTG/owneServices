set BinDir=%~dp0Bin\
set RelativePackageOutputPath=BAT
set PackageOutputPath=%BinDir%%RelativePackageOutputPath%

if /I "%QGL_BUILD_CONFIGURATION%" == "RELEASE" (
	set Configuration=%QGL_BUILD_CONFIGURATION%
) ELSE (
	set Configuration=Debug
)

call %BinDir%..\SetMSBuild.cmd
%msbuild% "%BinDir%Deployment\BAT\Bat.proj" /t:bat:package:discover /p:Operator=datbackground;Configuration=%Configuration%

cmd /c exit 0