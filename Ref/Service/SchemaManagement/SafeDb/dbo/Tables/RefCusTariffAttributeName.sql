CREATE TABLE [dbo].[RefCusTariffAttributeName](
	[ZY6_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffAttributeName_ZY6_PK] DEFAULT (NEWID()),
	[ZY6_Name] VARCHAR(32) NOT NULL,
	[ZY6_Description] VARCHAR(500) NOT NULL,
	[ZY6_ZZI_NKTariffType] VARCHAR(5) NOT NULL,
	[ZY6_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZY6_ColumnCaption] VARCHAR(35) NOT NULL CONSTRAINT [DF_RefCusTariffAttributeName_ZY6_ColumnCaption] DEFAULT (''),
	[ZY6_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZY6_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZY6_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZY6_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZY6_SysStartTime], [ZY6_SysEndTime]),
	CONSTRAINT [PK_RefCusTariffAttributeName] PRIMARY KEY NONCLUSTERED ([ZY6_PK] ASC),
	CONSTRAINT [FK_RefCusTariffAttributeName_RefDataGrouping] FOREIGN KEY([ZY6_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping ([ZZZ_DataGrouping]),
	CONSTRAINT [FK_RefCusTariffAttributeName_RefCusTariffType] FOREIGN KEY([ZY6_ZZZ_NKDataGrouping], [ZY6_ZZI_NKTariffType]) REFERENCES RefCusTariffType ([ZZI_ZZZ_NKDataGrouping], [ZZI_TariffType]),
	CONSTRAINT [CK_RefCusTariffAttributeName_ZY6_Description] CHECK ([ZY6_Description] <>''),
	CONSTRAINT [CK_RefCusTariffAttributeName_ZY6_Name] CHECK ([ZY6_Name] <>''),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[RefCusTariffAttributeNameHistory]))
GO 
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefCusTariffAttributeName_ZY6_ZZZ_NKDataGrouping_ZY6_ZZI_NKTariffType_ZY6_Name] ON RefCusTariffAttributeName([ZY6_ZZZ_NKDataGrouping], [ZY6_ZZI_NKTariffType], [ZY6_Name])
GO
ALTER TABLE RefCusTariffAttributeName SET (LOCK_ESCALATION = DISABLE);
