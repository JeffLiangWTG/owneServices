CREATE TABLE RefCusTariffNationalCode(
	[ZZW_PK]	UNIQUEIDENTIFIER	NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_PK] DEFAULT NEWID(),
	[ZZW_ZZ1_Tariff]	UNIQUEIDENTIFIER	NOT NULL,
	[ZZW_NationalCode]	VARCHAR(10)	NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_NationalCode] DEFAULT '',
	[ZZW_Description]	NVARCHAR(MAX)	NOT NULL,
	[ZZW_ZZF_NKTaxOrFeeCode]	VARCHAR(4)	NOT NULL,
	[ZZW_StartDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_StartDate] DEFAULT GetUtcDate(),
	[ZZW_EndDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_EndDate] DEFAULT '2079-06-06 23:59',
	[ZZW_PublishedDate] DATE NULL,
	[ZZW_ZZZ_NKDataGrouping]	VARCHAR(3)	NOT NULL,
	[ZZW_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZZW_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZZW_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZZW_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZZW_SysStartTime], [ZZW_SysEndTime]),
	CONSTRAINT [PK_RefCusTariffNationalCode] PRIMARY KEY CLUSTERED( [ZZW_PK] ASC ),
	CONSTRAINT [FK_RefCusTariffNationalCode_RefCusTariff] FOREIGN KEY([ZZW_ZZ1_Tariff]) REFERENCES  RefCusTariff ([ZZ1_PK]),
	CONSTRAINT [FK_RefCusTariffNationalCode_RefDataGrouping] FOREIGN KEY([ZZW_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping ([ZZZ_DataGrouping]),
	CONSTRAINT [CK_RefCusTariffNationalCode_ZZW_StartDate_ZZW_EndDate] CHECK ([ZZW_StartDate]<=[ZZW_EndDate]),
	CONSTRAINT [CK_RefCusTariffNationalCode_ZZW_NationalCode] CHECK ([ZZW_NationalCode] <> '')
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusTariffNationalCodeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode] ON [dbo].[RefCusTariffNationalCode]
(
	[ZZW_ZZZ_NKDataGrouping] ASC,
	[ZZW_ZZ1_Tariff] ASC,
	[ZZW_StartDate] ASC,
	[ZZW_ZZF_NKTaxOrFeeCode] ASC,
	[ZZW_NationalCode] ASC
)
GO
ALTER TABLE RefCusTariffNationalCode SET (LOCK_ESCALATION = DISABLE);
