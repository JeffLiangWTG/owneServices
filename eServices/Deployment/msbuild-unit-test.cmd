set BinDir=%cd%\Bin\
set SourcePath=%cd%\

if /I "%QGL_BUILD_CONFIGURATION%" == "RELEASE" (
    set Configuration=%QGL_BUILD_CONFIGURATION%
) ELSE (
    set Configuration=Debug
)

call %~dp0SetMSBuild.cmd
if /I "%Configuration%" == "Debug" (
    %msbuild% %~dp0msbuild-unit-test.proj %* /p:Configuration=%Configuration%
)