CREATE TABLE RefAccTaxRate
(
	[ZAT_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefAccTaxRate_ZAT_PK] DEFAULT NEWID(),
	[ZAT_RN_NKCountry] CHAR(2) NOT NULL,
	[ZAT_ReferenceRateType] VARCHAR(10) NOT NULL,
	[ZAT_StartDate] DATE NOT NULL CONSTRAINT [DF_RefAccTaxRate_ZAT_StartDate] DEFAULT ('1 January 1'),
	[ZAT_EndDate] DATE NOT NULL CONSTRAINT [DF_RefAccTaxRate_ZAT_EndDate] DEFAULT ('31 December 9999'),
	[ZAT_RateNumerator] INTEGER NOT NULL CONSTRAINT [DF_RefAccTaxRate_ZAT_RateNumerator] DEFAULT(0),
	[ZAT_RateDenominator] INTEGER NOT NULL CONSTRAINT [DF_RefAccTaxRate_ZAT_RateDenominator] DEFAULT(1),
	CONSTRAINT [CK_RefAccTaxRate_ZAT_RN_NKCountry] CHECK ([ZAT_RN_NKCountry] <> ''),
	CONSTRAINT [CK_RefAccTaxRate_ZAT_ReferenceRateType] CHECK ([ZAT_ReferenceRateType] <> ''),
	CONSTRAINT [CK_RefAccTaxRate_ZAT_StartDate_ZAT_EndDate] CHECK ([ZAT_StartDate]<=[ZAT_EndDate]),
	CONSTRAINT [CK_RefAccTaxRate_ZAT_RateNumerator] CHECK ([ZAT_RateNumerator] >= 0),
	CONSTRAINT [CK_RefAccTaxRate_ZAT_RateDenominator] CHECK ([ZAT_RateDenominator] >= 1)
)
GO
CREATE UNIQUE CLUSTERED INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate ON RefAccTaxRate(ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate ON RefAccTaxRate(ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate)
GO
