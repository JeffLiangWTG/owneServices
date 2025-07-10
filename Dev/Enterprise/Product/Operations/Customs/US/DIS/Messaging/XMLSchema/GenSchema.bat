@echo off

rem This Batch File needs to run using C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Microsoft Visual Studio 2012\Visual Studio Tools\VS2012 x86 Native Tools Command Prompt or similar VS Command Prompt
rem This Batch File should be run to update the DataFileSchema CS files if the XSD's are ever changed.
rem BEFORE running this script make sure that
rem 1) [schemaLocation] attributes in [xs:import] elements are changed in accordance with files names and locations in the project
rem http://www.cbp.gov/xp/cgov/trade/automated/modernization/ace_edi_messages/catair_main/abi_catair/catair_chapters/document_imaging_igs/

xsd MessageEnvelope.xsd /c /namespace:Enterprise.Customs.US.DIS.Messaging.DataFileSchema /language:CS

echo Done.