CREATE TABLE RefCusTariffAdditionalCodeLanguage (
	[ZY4_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffAdditionalCodeLanguage_ZY4_PK] DEFAULT NEWID(),
	[ZY4_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZY4_ZY2_TariffAdditionalCode] UNIQUEIDENTIFIER NOT NULL,
	[ZY4_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeLanguage_ZY4_Description DEFAULT '',
	[ZY4_DataSetPK] UNIQUEIDENTIFIER,
	[ZY4_DataSetCode] VARCHAR(3),
	[ZY4_SysStartTime]  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZY4_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZY4_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZY4_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZY4_SysStartTime], [ZY4_SysEndTime]),
	CONSTRAINT [PK_RefCusTariffAdditionalCodeLanguage] PRIMARY KEY CLUSTERED ([ZY4_PK] ASC),
	CONSTRAINT [FK_RefCusTariffAdditionalCodeLanguage_RefLanguageType] FOREIGN KEY([ZY4_ZX6_NKLanguage]) REFERENCES RefLanguageType ([ZX6_Language]),
	CONSTRAINT [FK_RefCusTariffAdditionalCodeLanguage_RefCusTariffAdditionalCode] FOREIGN KEY([ZY4_ZY2_TariffAdditionalCode]) REFERENCES RefCusTariffAdditionalCode ([ZY2_PK]),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusTariffAdditionalCodeLanguageHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCodeLanguage_ZY4_ZX6_NKLanguage_ZY4_ZY2_TariffAdditionalCode ON RefCusTariffAdditionalCodeLanguage(ZY4_ZX6_NKLanguage, ZY4_ZY2_TariffAdditionalCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCodeLanguage_ZY4_DataSetPK_ZY4_DataSetCode ON RefCusTariffAdditionalCodeLanguage(ZY4_DataSetPK ASC, ZY4_DataSetCode ASC)
GO
ALTER TABLE RefCusTariffAdditionalCodeLanguage SET (LOCK_ESCALATION = DISABLE);
