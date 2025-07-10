@echo off

rem This Batch File should be run to update the DataFileSchema CS files if the XSD's are ever changed.
rem Execute this bat file in "Developer Command Prompt for VS".

del CustomsExchangeRate.cs

xsd /P:Schemas.xml

ren CommonBasicComponents-2_CommonAggregateComponents-2_CustomsExchangeRate_TradenetResponse.cs CustomsExchangeRate.cs

echo Done.
