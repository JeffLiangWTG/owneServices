CREATE TABLE RefCusTariffBRCharacteristicValue
(
	ZB2_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_PK  DEFAULT (newid()),
	ZB2_ZB1_Characteristic UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic  DEFAULT (''),
	ZB2_Value NVARCHAR(100) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_Value  DEFAULT (''),
	ZB2_Description NVARCHAR(500) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicValue_ZB2_Description  DEFAULT (''),

	CONSTRAINT PK_RefCusTariffBRCharacteristicValue PRIMARY KEY CLUSTERED (ZB2_PK ASC),
	CONSTRAINT FK_RefCusTariffBRCharacteristicValue_RefCusTariffBRCharacteristic FOREIGN KEY(ZB2_ZB1_Characteristic) REFERENCES RefCusTariffBRCharacteristic (ZB1_PK),
	CONSTRAINT CK_RefCusTariffBRCharacteristicValue_ZB2_Value CHECK (ZB2_Value<>''),
	CONSTRAINT CK_RefCusTariffBRCharacteristicValue_ZB2_Description CHECK (ZB2_Description<>'')
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic ON RefCusTariffBRCharacteristicValue(ZB2_ZB1_Characteristic)
GO
ALTER TABLE RefCusTariffBRCharacteristicValue SET (LOCK_ESCALATION = DISABLE);
