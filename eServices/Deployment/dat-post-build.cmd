pushd %~dp0

call Copy2Bin.cmd
call msbuild-unit-test.cmd

popd