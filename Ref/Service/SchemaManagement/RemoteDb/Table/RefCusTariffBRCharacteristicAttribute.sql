CREATE TABLE RefCusTariffBRCharacteristicAttribute (
	ZB3_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_PK  DEFAULT (newid()),
	ZB3_ZB1_Characteristic UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic  DEFAULT (''),
	ZB3_Name VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Name  DEFAULT (''),
	ZB3_Code NVARCHAR(10) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Code  DEFAULT (''),
	ZB3_Value NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristicAttribute_ZB3_Value  DEFAULT (''),

	CONSTRAINT PK_RefCusTariffBRCharacteristicAttribute PRIMARY KEY CLUSTERED (ZB3_PK ASC),
	CONSTRAINT FK_RefCusTariffBRCharacteristicAttribute_RefCusTariffBRCharacteristic FOREIGN KEY(ZB3_ZB1_Characteristic) REFERENCES RefCusTariffBRCharacteristic (ZB1_PK)
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic ON RefCusTariffBRCharacteristicAttribute(ZB3_ZB1_Characteristic)
GO
ALTER TABLE RefCusTariffBRCharacteristicAttribute SET (LOCK_ESCALATION = DISABLE);
