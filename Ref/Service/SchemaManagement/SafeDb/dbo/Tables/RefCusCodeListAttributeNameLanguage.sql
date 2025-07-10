CREATE TABLE RefCusCodeListAttributeNameLanguage(
	[ZXH_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusCodeListAttributeNameLanguage_ZXH_PK] DEFAULT (NEWID()),
	[ZXH_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZXH_ZXE_CodeListAttributeName] UNIQUEIDENTIFIER NOT NULL,
	[ZXH_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusCodeListAttributeNameLanguage_ZXH_Description] DEFAULT (''),
	[ZXH_Name] NVARCHAR(MAX) NULL,
	[ZXH_ColumnCaption] NVARCHAR(35) NOT NULL CONSTRAINT [DF_RefCusCodeListAttributeNameLanguage_ZXH_ColumnCaption] DEFAULT (''),
	[ZXH_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZXH_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZXH_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZXH_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZXH_SysStartTime], [ZXH_SysEndTime]),
	CONSTRAINT [PK_RefCusCodeListAttributeNameLanguage] PRIMARY KEY CLUSTERED ([ZXH_PK] ASC),
	CONSTRAINT [FK_RefCusCodeListAttributeNameLanguage_ZXH_ZX6_NKLanguage] FOREIGN KEY([ZXH_ZX6_NKLanguage]) REFERENCES RefLanguageType ([ZX6_Language]),
	CONSTRAINT [FK_RefCusCodeListAttributeNameLanguage_ZXH_ZXE_CodeListAttributeName] FOREIGN KEY([ZXH_ZXE_CodeListAttributeName]) REFERENCES RefCusCodeListAttributeName ([ZXE_PK]),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusCodeListAttributeNameLanguageHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttributeNameLanguage_ZXH_ZX6_NKLanguage_ZXH_ZXE_CodeListAttributeName ON RefCusCodeListAttributeNameLanguage (ZXH_ZX6_NKLanguage, ZXH_ZXE_CodeListAttributeName)
GO
ALTER TABLE RefCusCodeListAttributeNameLanguage SET (LOCK_ESCALATION = DISABLE);
