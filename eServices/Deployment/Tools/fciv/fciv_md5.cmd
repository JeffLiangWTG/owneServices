set OutputDir=%~1
set fciv_file="%OutputDir%\fciv.txt"

pushd %~dp0

if exist %fciv_file% del %fciv_file%
echo %date%-%time% > %fciv_file%
fciv.exe "%OutputDir%" -add -r -type *.dll >> %fciv_file%

popd