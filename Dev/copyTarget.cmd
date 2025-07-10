pushd "%0..\..\"
cd Bin
set BinDir=%CD%\
popd

xcopy %1 %BinDir% /Y /Q
if exist "%~d1%~p1%~n1.pdb" xcopy "%~d1%~p1%~n1.pdb" %BinDir% /Y /Q
