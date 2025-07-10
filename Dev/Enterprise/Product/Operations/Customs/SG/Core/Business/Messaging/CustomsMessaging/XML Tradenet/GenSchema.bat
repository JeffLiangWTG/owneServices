@echo off

rem This Batch File should be run to update the DataFileSchema CS files if the XSD's are ever changed.
rem Execute this bat file in "Developer Command Prompt for VS".

xsd /P:Schemas.xml

echo Done.
