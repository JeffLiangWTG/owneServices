CREATE TABLE RefCusVATApplicability (
	[ZX5_PK] UNIQUEIDENTIFIER NOT NULL,
	[ZX5_ZZ1_Tariff] UNIQUEIDENTIFIER NULL,
	[ZX5_ZZW_TariffNationalCode] UNIQUEIDENTIFIER NULL,
	[ZX5_ZZF_NKTaxOrFeeCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_ZZF_NKTaxOrFeeCode] DEFAULT (''),
	[ZX5_StartDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_StartDate] DEFAULT ('1900-01-01'),
	[ZX5_EndDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_EndDate] DEFAULT ('2079-06-06 23:59'),
	[ZX5_AdditionalCode] VARCHAR(15) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_AdditionalCode] DEFAULT (''),
	[ZX5_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_Description] DEFAULT (N''),
	[ZX5_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZX5_DataSetPK] UNIQUEIDENTIFIER,
	[ZX5_DataSetCode] VARCHAR(3),
	[ZX5_ZZA_TradeGroup] UNIQUEIDENTIFIER NULL,
	[ZX5_VATCategory] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_VATCategory] DEFAULT (''),
	[ZX5_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZX5_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZX5_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZX5_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZX5_SysStartTime], [ZX5_SysEndTime]),
	CONSTRAINT [PK_RefCusVATApplicability] PRIMARY KEY CLUSTERED([ZX5_PK] ASC),
	CONSTRAINT [FK_RefCusVATApplicability_RefCusTariff] FOREIGN KEY ([ZX5_ZZ1_Tariff]) REFERENCES RefCusTariff([ZZ1_PK]),
	CONSTRAINT [FK_RefCusVATApplicability_RefCusTariffNationalCode] FOREIGN KEY ([ZX5_ZZW_TariffNationalCode]) REFERENCES RefCusTariffNationalCode([ZZW_PK]),
	CONSTRAINT [FK_RefCusVATApplicability_RefDataGrouping] FOREIGN KEY ([ZX5_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping([ZZZ_DataGrouping]),
	CONSTRAINT [FK_RefCusVATApplicability_RefCusTradeGroup] FOREIGN KEY ([ZX5_ZZA_TradeGroup]) REFERENCES RefCusTradeGroup([ZZA_PK]),
	CONSTRAINT [CK_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode] CHECK(([ZX5_ZZ1_Tariff] IS NULL AND [ZX5_ZZW_TariffNationalCode] IS NOT NULL) OR ([ZX5_ZZ1_Tariff] IS NOT NULL AND [ZX5_ZZW_TariffNationalCode] IS NULL)),
	CONSTRAINT [CK_RefCusVATApplicability_ZX5_StartDate_ZX5_EndDate] CHECK([ZX5_StartDate] <= [ZX5_EndDate]),
	CONSTRAINT [CK_RefCusVATApplicability_ZX5_VATCategory] CHECK(ZX5_VATCategory = '' OR ZX5_VATCategory LIKE '[A-Z][0-9][0-9][0-9]'),
	CONSTRAINT [CK_RefCusVATApplicability_ZX5_VATCategory_ZX5_ZZF_NKTaxOrFeeCode_ZX5_ZZZ_NKDataGrouping] CHECK ([ZX5_VATCategory]='' OR ([ZX5_VATCategory] != '' AND [ZX5_ZZZ_NKDataGrouping] = 'FR'))
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusVATApplicabilityHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_StartDateUnique
	ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate, ZX5_ZZA_TradeGroup)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_EndDateUnique
	ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_EndDate, ZX5_ZZA_TradeGroup)
GO
CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_DataSetPK_ZX5_DataSetCode ON RefCusVATApplicability(ZX5_DataSetPK ASC, ZX5_DataSetCode ASC)
GO
ALTER TABLE RefCusVATApplicability SET (LOCK_ESCALATION = DISABLE);
