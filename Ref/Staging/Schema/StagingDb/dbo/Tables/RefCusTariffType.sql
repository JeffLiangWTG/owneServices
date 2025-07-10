CREATE TABLE [dbo].[RefCusTariffType](
	[ZZI_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefCusTariffType_ZZI_PK]  DEFAULT (newid()),
	[ZZI_TariffType] [varchar](5) NOT NULL CONSTRAINT [DF_RefCusTariffType_ZZI_TariffType]  DEFAULT (''),
	[ZZI_Description] NVARCHAR(100) NOT NULL CONSTRAINT [DF_RefCusTariffType_ZZI_Description]  DEFAULT (''),
	[ZZI_ZZZ_NKDataGrouping] [varchar](3) NOT NULL CONSTRAINT [DF_RefCusTariffType_ZZI_ZZZ_NKDataGrouping]  DEFAULT (''),
	[ZZI_ZZR_RateType] [uniqueidentifier],
	[ZZI_ZZR_NKRateType] [varchar](3) CONSTRAINT [DF_RefCusTariffType_ZZI_ZZR_NKRateType] DEFAULT (''),
	[ZZI_HasFormulaSpecificQuestions] [bit] NOT NULL CONSTRAINT [DF_RefCusTariffType_ZZI_HasFormulaSpecificQuestions]  DEFAULT ((0)),
	[ZZI_ZZ9_NKNomenclatureGroupType] VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffType_ZZI_ZZ9_NKNomenclatureGroupType DEFAULT (''),
	CONSTRAINT [PK_RefCusTariffType] PRIMARY KEY CLUSTERED ([ZZI_PK] ASC),
	CONSTRAINT [FK_RefCusTariffType_RefCusRateType] FOREIGN KEY([ZZI_ZZR_RateType]) REFERENCES [dbo].[RefCusRateType] ([ZZR_PK]) ON DELETE CASCADE,
)
