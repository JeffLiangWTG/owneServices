CREATE TABLE [dbo].[RefCusTariffAttribute](
	[ZZ3_PK] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RefCusTariffAttribute_ZZ3_PK]  DEFAULT (newid()),
	[ZZ3_ZZ1_Tariff] [uniqueidentifier] NULL,
	[ZZ3_ZZW_TariffNationalCode] [uniqueidentifier] NULL,
	[ZZ3_Name] [varchar](50) NOT NULL CONSTRAINT [DF_RefCusTariffAttribute_ZZ3_Name]  DEFAULT (''),
	[ZZ3_Value] [nvarchar](MAX) NOT NULL CONSTRAINT [DF_RefCusTariffAttribute_ZZ3_Value]  DEFAULT (''),
 CONSTRAINT [PK_RefCusTariffAttribute] PRIMARY KEY CLUSTERED ([ZZ3_PK] ASC),
 CONSTRAINT [FK_RefCusTariffAttribute_RefCusTariff] FOREIGN KEY([ZZ3_ZZ1_Tariff])REFERENCES [dbo].[RefCusTariff] ([ZZ1_PK]),
 CONSTRAINT FK_RefCusTariffAttribute_RefCusTariffNationalCode FOREIGN KEY (ZZ3_ZZW_TariffNationalCode) REFERENCES RefCusTariffNationalCode (ZZW_PK),
 CONSTRAINT CK_RefCusTariffAttribute_ZZ3_ZZ1_Tariff_ZZ3_ZZW_TariffNationalCode CHECK ((ZZ3_ZZ1_Tariff IS NOT NULL AND ZZ3_ZZW_TariffNationalCode IS NULL) OR (ZZ3_ZZ1_Tariff IS NULL AND ZZ3_ZZW_TariffNationalCode IS NOT NULL)),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffAttribute_ZZ3_ZZ1_Tariff] ON [RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffAttribute_ZZ3_ZZW_TariffNationalCode] ON [RefCusTariffAttribute] ([ZZ3_ZZW_TariffNationalCode])
