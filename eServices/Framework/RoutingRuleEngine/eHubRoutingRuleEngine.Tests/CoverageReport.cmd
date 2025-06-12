RMDIR ..\Bin\TestResults /S /Q
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
dotnet test --collect "XPlat Code Coverage" --results-directory "..\Bin\TestResults"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
reportgenerator -reports:"..\Bin\TestResults\*\coverage.cobertura.xml" -targetdir:"..\Bin\TestResults\CoverageReport" -reporttypes:Html
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
START ..\Bin\TestResults\CoverageReport\index.html
