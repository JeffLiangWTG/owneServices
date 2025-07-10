CREATE TABLE RefCusTariffBRCharacteristic
(
	ZB1_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_PK  DEFAULT (newid()),
	ZB1_CharacteristicType VARCHAR(5) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_CharacteristicType  DEFAULT (''),
	ZB1_ZZ1_Tariff UNIQUEIDENTIFIER NULL,
	ZB1_ZZ5_Nomenclature UNIQUEIDENTIFIER NULL,
	ZB1_Style VARCHAR(10) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_Style  DEFAULT (''),
	ZB1_MaxLength SMALLINT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_MaxLength  DEFAULT (0),
	ZB1_DecimalPlaces SMALLINT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_DecimalPlaces  DEFAULT (0),
	ZB1_Code NVARCHAR(35) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_Code  DEFAULT (''),
	ZB1_Text NVARCHAR(1000) NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_Text  DEFAULT (''),
	ZB1_StartDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_StartDate  DEFAULT ('1900-01-01'),
	ZB1_EndDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_EndDate  DEFAULT ('2079-06-06 23:59'),
	ZB1_IsImport BIT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_IsImport  DEFAULT (0),
	ZB1_IsExport BIT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_IsExport  DEFAULT (0),
	ZB1_IsMandatory BIT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_IsMandatory  DEFAULT (0),
	ZB1_IsConditioningAttribute BIT NOT NULL CONSTRAINT DF_RefCusTariffBRCharacteristic_ZB1_IsConditioningAttribute  DEFAULT (0),

	CONSTRAINT PK_RefCusTariffBRCharacteristic PRIMARY KEY CLUSTERED (ZB1_PK ASC),
	CONSTRAINT FK_RefCusTariffBRCharacteristic_RefCusTariff FOREIGN KEY(ZB1_ZZ1_Tariff) REFERENCES RefCusTariff (ZZ1_PK),
	CONSTRAINT FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup FOREIGN KEY(ZB1_ZZ5_Nomenclature) REFERENCES RefCusNomenclatureGroup (ZZ5_PK),
	CONSTRAINT CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType CHECK  (ZB1_CharacteristicType='NVE' OR ZB1_CharacteristicType='NCM' OR ZB1_CharacteristicType='NCMTE' OR ZB1_CharacteristicType='LPC' OR ZB1_CharacteristicType='LPCT'),
	CONSTRAINT CK_RefCusTariffBRCharacteristic_ZB1_Style CHECK  (ZB1_Style='BOOLEAN' OR ZB1_Style='STRING' OR ZB1_Style='NUMBER' OR ZB1_Style='LIST' OR ZB1_Style='COMPOSED' OR ZB1_Style='DATE'),
	CONSTRAINT CK_RefCusTariffBRCharacteristic_ZB1_StartDate_ZB1_EndDate CHECK  (ZB1_StartDate<=ZB1_EndDate),
	CONSTRAINT CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff CHECK  ((ZB1_ZZ1_Tariff IS NULL AND ZB1_ZZ5_Nomenclature IS NOT NULL) OR (ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_ZZ5_Nomenclature IS NULL)),
)
GO
CREATE NONCLUSTERED INDEX IX_ZB1_ZZ1_Tariff ON RefCusTariffBRCharacteristic (ZB1_ZZ1_Tariff ASC) where ZB1_ZZ1_Tariff IS NOT NULL
GO
CREATE NONCLUSTERED INDEX IX_ZB1_ZZ5_Nomenclature ON RefCusTariffBRCharacteristic (ZB1_ZZ5_Nomenclature ASC) where ZB1_ZZ5_Nomenclature IS NOT NULL 
GO
ALTER TABLE RefCusTariffBRCharacteristic SET (LOCK_ESCALATION = DISABLE);
