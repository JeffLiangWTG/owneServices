CREATE TABLE RefCusTariffAdditionalCodeCategory(
	[ZY3_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffAdditionalCodeCategory_ZY3_PK] DEFAULT NEWID(),
	[ZY3_Category] CHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeCategory_ZY3_Category DEFAULT '',
	[ZY3_Description] NVARCHAR(200) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeCategory_ZY3_Description DEFAULT '',
	[ZY3_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZY3_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZY3_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZY3_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZY3_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZY3_SysStartTime], [ZY3_SysEndTime]),
	CONSTRAINT [PK_RefCusTariffAdditionalCodeCategory] PRIMARY KEY CLUSTERED ([ZY3_PK] ASC),
	CONSTRAINT [FK_RefCusTariffAdditionalCodeCategory_RefDataGrouping] FOREIGN KEY([ZY3_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping]),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusTariffAdditionalCodeCategoryHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusTariffAdditionalCodeCategory_ZY3_ZZZ_NKDataGrouping_ZY3_Category] ON [RefCusTariffAdditionalCodeCategory]([ZY3_ZZZ_NKDataGrouping], [ZY3_Category])
GO
ALTER TABLE RefCusTariffAdditionalCodeCategory SET (LOCK_ESCALATION = DISABLE);
