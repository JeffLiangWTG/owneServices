for /d %%i in (*) do ( if not '%%i'=='Bin' xcopy /EXCLUDE:ExceptFiles.txt %%i Bin\Deployment\%%i /c/r/i/k/e/y )
xcopy /EXCLUDE:ExceptFiles.txt *.* Bin\Deployment\ /c/r/k/y