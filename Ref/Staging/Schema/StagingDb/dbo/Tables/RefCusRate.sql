CREATE TABLE [dbo].[RefCusRate](
	[ZZ2_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefCusRate_ZZ2_PK]  DEFAULT (newid()),
	[ZZ2_ZZ1_Tariff] [uniqueidentifier] NULL,
	[ZZ2_ZZW_TariffNationalCode] [UNIQUEIDENTIFIER] NULL,
	[ZZ2_StartDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusRate_ZZ2_StartDate] DEFAULT GetUtcDate(),
	[ZZ2_EndDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusRate_ZZ2_EndDate] DEFAULT '2079-06-06 23:59',
	[ZZ2_ZY1_NKRateCode] [VARCHAR](5) ,
	[ZZ2_ZY1_ZZR_NKRateType] [VARCHAR](3) ,
	[ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping] [VARCHAR](3) ,
	[ZZ2_RateFormula] [varchar](500) NOT NULL,
	[ZZ2_ZZS_NKPreference] [VARCHAR](10) ,
	[ZZ2_ZZS_ZZZ_NKDataGrouping] [VARCHAR](3) ,
	[ZZ2_SelectorFormula] [varchar](500) NOT NULL CONSTRAINT [DF_RefCusRate_ZZ2_SelectorFormula]  DEFAULT (''),
	[ZZ2_ZZZ_NKDataGrouping] [varchar](3) NOT NULL CONSTRAINT [DF_RefCusRate_ZZ2_ZZZ_NKDataGrouping]  DEFAULT (''),
	[ZZ2_RateFormulaDerivedFrom] NVARCHAR(1000) NULL,
	[ZZ2_RX_NKCurrencyOverride] VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusRate_ZZ2_RX_NKCurrencyOverride DEFAULT (''),
	CONSTRAINT [PK_RefCusRate] PRIMARY KEY CLUSTERED ([ZZ2_PK] ASC),
	CONSTRAINT [FK_RefCusRate_RefCusTariff] FOREIGN KEY([ZZ2_ZZ1_Tariff]) REFERENCES RefCusTariff (ZZ1_PK),
	CONSTRAINT [FK_RefCusRate_RefCusTariffNationalCode] FOREIGN KEY ([ZZ2_ZZW_TariffNationalCode]) REFERENCES RefCusTariffNationalCode (ZZW_PK),
	CONSTRAINT [CK_RefCusRate_ZZ2_StartDate_ZZ2_EndDate] CHECK ([ZZ2_StartDate] <= [ZZ2_EndDate]),
	CONSTRAINT [CK_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZXW_TariffNationalCode] CHECK ((ZZ2_ZZ1_Tariff IS NULL AND ZZ2_ZZW_TariffNationalCode IS NOT NULL) OR (ZZ2_ZZ1_Tariff IS NOT NULL AND ZZ2_ZZW_TariffNationalCode IS NULL)),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusRate_ZZ2_ZZ1_Tariff] ON [RefCusRate] ([ZZ2_ZZ1_Tariff])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusRate_ZZ2_ZZW_TariffNationalCode] ON [RefCusRate] ([ZZ2_ZZW_TariffNationalCode])
