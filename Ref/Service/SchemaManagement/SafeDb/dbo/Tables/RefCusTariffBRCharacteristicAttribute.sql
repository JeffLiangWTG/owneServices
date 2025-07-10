CREATE TABLE [dbo].[RefCusTariffBRCharacteristicAttribute]
(
	ZB3_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_PK  DEFAULT (newid()),
	ZB3_ZB1_Characteristic UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic  DEFAULT (''),
	ZB3_Name VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Name  DEFAULT (''),
	ZB3_Code NVARCHAR(10) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Code  DEFAULT (''),
	ZB3_Value NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Value  DEFAULT (''),
	ZB3_DataSetPK UNIQUEIDENTIFIER,
	ZB3_DataSetCode VARCHAR(3),
	ZB3_SysStartTime  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT DF_ZB3_SysStartTime DEFAULT CONVERT(DATETIME2, SYSUTCDATETIME()),
	ZB3_SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT DF_ZB3_SysEndTime DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME (ZB3_SysStartTime, ZB3_SysEndTime),

	CONSTRAINT PK_RefCusTariffBRCharacteristicAttribute PRIMARY KEY CLUSTERED (ZB3_PK ASC),
	CONSTRAINT FK_RefCusTariffBRCharacteristicAttribute_RefCusTariffBRCharacteristic FOREIGN KEY(ZB3_ZB1_Characteristic) REFERENCES RefCusTariffBRCharacteristic (ZB1_PK)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[RefCusTariffBRCharacteristicAttributeHistory]))
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic_ZB3_DataSetPK ON RefCusTariffBRCharacteristicAttribute (ZB3_ZB1_Characteristic,ZB3_DataSetPK)
GO
ALTER TABLE RefCusTariffBRCharacteristicAttribute SET (LOCK_ESCALATION = DISABLE)
