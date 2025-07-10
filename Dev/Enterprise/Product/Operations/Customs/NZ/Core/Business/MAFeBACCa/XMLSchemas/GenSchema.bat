@echo off

rem This Batch File should be run to update the DataFileSchema CS files if the XSD's are ever changed.

xsd XSDs\MAF.EBACCA.Notification_v1.0.xsd /classes /namespace:Enterprise.Customs.NZ.Business.MAFeBACCa.DataFileSchema /language:CS
xsd XSDs\MAF.EBACCA.Request_v1.2.xsd /classes /namespace:Enterprise.Customs.NZ.Business.MAFeBACCa.DataFileSchema /language:CS
xsd XSDs\MAF.EBACCA.Response_v1.0.xsd /classes /namespace:Enterprise.Customs.NZ.Business.MAFeBACCa.DataFileSchema /language:CS
xsd XSDs\MAF.Messaging.Schema.Request_v1.1.xsd /classes /namespace:Enterprise.Customs.NZ.Business.MAFeBACCa.DataFileSchema /language:CS
xsd XSDs\MAF.Messaging.Schema.Response_v1.0.xsd /classes /namespace:Enterprise.Customs.NZ.Business.MAFeBACCa.DataFileSchema /language:CS

echo Done.