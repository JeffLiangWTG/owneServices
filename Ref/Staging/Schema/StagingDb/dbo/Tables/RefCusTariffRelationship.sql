CREATE TABLE [dbo].[RefCusTariffRelationship](
	[ZZH_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefCusTariffRelationship_ZZH_PK]  DEFAULT (newid()),
	[ZZH_ZZ1_Tariff] [uniqueidentifier] NOT NULL,
	[ZZH_ZZI_NKTariffType] [varchar](5) CONSTRAINT [DF_RefCusTariffRelationship_ZZH_ZZI_NKTariffType] DEFAULT (''),
	[ZZH_ZZI_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusTariff_ZZH_ZZI_ZZZ_NKDataGrouping] DEFAULT '', 
	[ZZH_TariffCode] [varchar](35) NOT NULL CONSTRAINT [DF_RefCusTariffRelationship_ZZH_TariffCode]  DEFAULT (''),
 CONSTRAINT [PK_RefCusTariffRelationship] PRIMARY KEY CLUSTERED ([ZZH_PK] ASC),
 CONSTRAINT [FK_RefCusTariffRelationship_RefCusTariff] FOREIGN KEY([ZZH_ZZ1_Tariff])REFERENCES [dbo].[RefCusTariff] ([ZZ1_PK]),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffRelationship_ZZH_ZZ1_Tariff] ON [RefCusTariffRelationship] ([ZZH_ZZ1_Tariff])
