@echo off
for /D %%G in (Staging\UniversalXMLProducers\*) do (
echo %%G|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%G))
for /D %%G in (Staging\PushNotification\*) do (
echo %%G|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%G))
for /D %%G in (Staging\MessageDownloader\*) do (
echo %%G|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%G))
for /D %%G in (Staging\MessageProcessor\*) do (
echo %%G|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%G))
for /D %%G in (Staging\UniversalXmlProcessor\*) do (
echo %%G|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%G))
for /D %%G in (UniversalXMLProducers\*) do (
for /D %%H in (%%G\*) do (
echo %%H|find "Test" >nul
if errorlevel 1 (COPY .\FodyWeavers.* %%H))
)
)
EXIT /B 0
