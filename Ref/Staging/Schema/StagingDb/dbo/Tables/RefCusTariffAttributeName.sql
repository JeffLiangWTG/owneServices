CREATE TABLE [dbo].[RefCusTariffAttributeName](
	[ZY6_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffAttributeName_ZY6_PK] DEFAULT (NEWID()),
	[ZY6_Name] VARCHAR(32) NOT NULL,
	[ZY6_Description] VARCHAR(500) NOT NULL,
	[ZY6_ZZI_NKTariffType] VARCHAR(5) NOT NULL,
	[ZY6_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZY6_ColumnCaption] VARCHAR(35) NOT NULL CONSTRAINT [DF_RefCusTariffAttributeName_ZY6_ColumnCaption] DEFAULT (''),
	CONSTRAINT [PK_RefCusTariffAttributeName] PRIMARY KEY NONCLUSTERED ([ZY6_PK] ASC),
)
GO
CREATE CLUSTERED INDEX [IX_RefCusTariffAttributeName_ZY6_ZZZ_NKDataGrouping_ZY6_ZZI_NKTariffType_ZY6_Name] ON RefCusTariffAttributeName([ZY6_ZZZ_NKDataGrouping], [ZY6_ZZI_NKTariffType], [ZY6_Name])
GO
