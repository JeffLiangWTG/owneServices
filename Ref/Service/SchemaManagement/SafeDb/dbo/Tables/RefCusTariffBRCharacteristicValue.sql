CREATE TABLE [dbo].[RefCusTariffBRCharacteristicValue]
(
	ZB2_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_PK  DEFAULT (newid()),
	ZB2_ZB1_Characteristic UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic  DEFAULT (''),
	ZB2_Value NVARCHAR(100) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_Value  DEFAULT (''),
	ZB2_Description NVARCHAR(500) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_Description  DEFAULT (''),
	ZB2_DataSetPK UNIQUEIDENTIFIER,
	ZB2_DataSetCode VARCHAR(3),
	ZB2_SysStartTime  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT DF_ZB2_SysStartTime DEFAULT CONVERT(DATETIME2, SYSUTCDATETIME()),
	ZB2_SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT DF_ZB2_SysEndTime DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME (ZB2_SysStartTime, ZB2_SysEndTime),

	CONSTRAINT PK_RefCusTariffBRCharacteristicValue PRIMARY KEY CLUSTERED (ZB2_PK ASC),
	CONSTRAINT FK_RefCusTariffBRCharacteristicValue_RefCusTariffBRCharacteristic FOREIGN KEY(ZB2_ZB1_Characteristic) REFERENCES RefCusTariffBRCharacteristic (ZB1_PK),
	CONSTRAINT CK_RefCusTariffBRCharacteristicValue_ZB2_Value CHECK (ZB2_Value<>''),
	CONSTRAINT CK_RefCusTariffBRCharacteristicValue_ZB2_Description CHECK (ZB2_Description<>'')
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[RefCusTariffBRCharacteristicValueHistory]))
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic_ZB2_DataSetPK ON RefCusTariffBRCharacteristicValue (ZB2_ZB1_Characteristic,ZB2_DataSetPK)
GO
ALTER TABLE RefCusTariffBRCharacteristicValue SET (LOCK_ESCALATION = DISABLE)
