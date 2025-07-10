CREATE TABLE RefCusVATApplicability
(
ZX5_PK UNIQUEIDENTIFIER NOT NULL,
ZX5_ZZ1_Tariff UNIQUEIDENTIFIER NULL,
ZX5_ZZW_TariffNationalCode UNIQUEIDENTIFIER NULL,
ZX5_ZZF_NKTaxOrFeeCode VARCHAR(4) NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_ZZF_NKTaxOrFeeCode DEFAULT (''),
ZX5_StartDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_StartDate DEFAULT ('1900-01-01'),
ZX5_EndDate SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_EndDate DEFAULT ('2079-06-06 23:59'),
ZX5_AdditionalCode VARCHAR(15) NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_AdditionalCode DEFAULT (''),
ZX5_Description NVARCHAR(500) NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_Description DEFAULT (N''),
ZX5_DataSetId SMALLINT NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_DataSetId DEFAULT (0),
ZX5_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
ZX5_ZZA_TradeGroup UNIQUEIDENTIFIER NULL,
ZX5_VATCategory VARCHAR(4) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_VATCategory] DEFAULT (''),
ZX5_ZZ1_ParentTariffOrNationalCode AS (ISNULL(ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode)),

CONSTRAINT PK_RefCusVATApplicability PRIMARY KEY NONCLUSTERED(ZX5_PK ASC),
CONSTRAINT FK_RefCusVATApplicability_RefCusTariff FOREIGN KEY (ZX5_ZZ1_Tariff) REFERENCES RefCusTariff(ZZ1_PK),
CONSTRAINT FK_RefCusVATApplicability_RefCusTariffNationalCode FOREIGN KEY (ZX5_ZZW_TariffNationalCode) REFERENCES RefCusTariffNationalCode(ZZW_PK),
CONSTRAINT FK_RefCusVATApplicability_RefDataGrouping FOREIGN KEY (ZX5_ZZZ_NKDataGrouping) REFERENCES RefDataGrouping(ZZZ_DataGrouping),
CONSTRAINT FK_RefCusVATApplicability_RefCusTradeGroup FOREIGN KEY (ZX5_ZZA_TradeGroup) REFERENCES RefCusTradeGroup(ZZA_PK),
CONSTRAINT CK_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode CHECK((ZX5_ZZ1_Tariff IS NULL AND ZX5_ZZW_TariffNationalCode IS NOT NULL) OR (ZX5_ZZ1_Tariff IS NOT NULL AND ZX5_ZZW_TariffNationalCode IS NULL)),
CONSTRAINT CK_RefCusVATApplicability_ZX5_StartDate_ZX5_EndDate CHECK(ZX5_StartDate <= ZX5_EndDate),
CONSTRAINT CK_RefCusVATApplicability_ZX5_VATCategory CHECK(ZX5_VATCategory = '' OR ZX5_VATCategory LIKE '[A-Z][0-9][0-9][0-9]'),
CONSTRAINT CK_RefCusVATApplicability_ZX5_VATCategory_ZX5_ZZF_NKTaxOrFeeCode_ZX5_ZZZ_NKDataGrouping CHECK ([ZX5_VATCategory]='' OR ([ZX5_VATCategory] != '' AND [ZX5_ZZZ_NKDataGrouping] = 'FR'))
)
GO
CREATE CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK ON RefCusVATApplicability (ZX5_DataSetId ASC, ZX5_PK ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_StartDateUnique ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate, ZX5_ZZA_TradeGroup)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate ON RefCusVATApplicability(ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_AdditionalCode ON RefCusVATApplicability(ZX5_AdditionalCode) WHERE ZX5_AdditionalCode != ''
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_EndDateUnique ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_EndDate, ZX5_ZZA_TradeGroup)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZA_TradeGroup ON RefCusVATApplicability (ZX5_ZZA_TradeGroup)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode ON RefCusVATApplicability(ZX5_ZZ1_ParentTariffOrNationalCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff ON RefCusVATApplicability(ZX5_ZZ1_Tariff)
GO
ALTER TABLE RefCusVATApplicability SET (LOCK_ESCALATION = DISABLE);
