CREATE TABLE [dbo].[RefCusTariffUOM](
	[ZZ8_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefCusTariffUOM_ZZ8_PK]  DEFAULT (newid()),
	[ZZ8_ZZ1_Tariff] [uniqueidentifier] NULL,
	[ZZ8_Type] [varchar](3) NOT NULL,
	[ZZ8_UOM] [varchar](10) NOT NULL,
	[ZZ8_ZZA_NKTradeGroup] [varchar](35) NULL,
	[ZZ8_ZZA_ZZZ_NKDataGrouping] [varchar](3) null,
	[ZZ8_ZZW_TariffNationalCode] [uniqueidentifier] NULL,
	[ZZ8_ZZZ_NKDataGrouping] [varchar](3) NOT NULL CONSTRAINT [DF_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping] DEFAULT (''),
	[ZZ8_ZZA_NKSecondTradeGroup] [varchar](35) NULL,
	[ZZ8_ZZA_ZZZ_NKSecondDataGrouping] [varchar](3) NULL,
	[ZZ8_StartDate] SMALLDATETIME SPARSE NULL,
	[ZZ8_EndDate] SMALLDATETIME SPARSE NULL,

 CONSTRAINT [PK_RefCusTariffUOM] PRIMARY KEY CLUSTERED ([ZZ8_PK] ASC),
 CONSTRAINT [FK_RefCusTariffUOM_RefCusTariff] FOREIGN KEY([ZZ8_ZZ1_Tariff])REFERENCES [dbo].[RefCusTariff] ([ZZ1_PK]),
 CONSTRAINT [FK_RefCusTariffUOM_RefCusTariffNationalCode] FOREIGN KEY([ZZ8_ZZW_TariffNationalCode]) REFERENCES [RefCusTariffNationalCode] ([ZZW_PK]),
 CONSTRAINT [CK_RefCusTariffUOM_ZZ8_Type] CHECK ([ZZ8_Type]='CU1' OR [ZZ8_Type]='RU1' OR [ZZ8_Type]='AD1' OR [ZZ8_Type]='CU2' OR [ZZ8_Type]='CU3' OR [ZZ8_Type]='CU4' OR [ZZ8_Type]='CU5'),
 CONSTRAINT [CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate] CHECK (([ZZ8_StartDate] IS NULL AND [ZZ8_EndDate] IS NULL) OR ([ZZ8_StartDate] IS NOT NULL AND [ZZ8_EndDate] IS NOT NULL AND [ZZ8_StartDate] <= [ZZ8_EndDate]))
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff] ON [RefCusTariffUOM] ([ZZ8_ZZ1_Tariff])
Go
CREATE NONCLUSTERED INDEX [IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode] ON [RefCusTariffUOM] ([ZZ8_ZZW_TariffNationalCode])
