@ECHO OFF
SET jmeterPath=%1
SET testName=%2
SET reportPath=%testName%\%testName%-Report
ECHO. 2>%reportPath%\%testName%.log
CALL %jmeterPath%\jmeter -n -t %testName%\TestPlan.jmx -JReportPath=%reportPath% -j %reportPath%\%testName%.log