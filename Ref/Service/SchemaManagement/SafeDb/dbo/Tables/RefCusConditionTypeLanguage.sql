CREATE TABLE [RefCusConditionTypeLanguage]
(
	[ZXW_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_PK] DEFAULT NEWID(),
	[ZXW_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage] DEFAULT '',
	[ZXW_ZX2_ConditionType] UNIQUEIDENTIFIER NOT NULL,
	[ZXW_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_Description] DEFAULT '',
	[ZXW_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZXW_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZXW_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZXW_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZXW_SysStartTime], [ZXW_SysEndTime]),
	CONSTRAINT [CK_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage] CHECK (ZXW_ZX6_NKLanguage <> ''),
	CONSTRAINT [CK_RefCusConditionTypeLanguage_ZXW_Description] CHECK (ZXW_Description <> ''),
	CONSTRAINT [FK_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType] FOREIGN KEY([ZXW_ZX2_ConditionType]) REFERENCES [RefCusConditionType] ([ZX2_PK]),
	CONSTRAINT [FK_RefCusConditionTypeLanguage_ZXV_ZX6_NKLanguage] FOREIGN KEY([ZXW_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language]),
	CONSTRAINT [PK_RefCusConditionTypeLanguage] PRIMARY KEY CLUSTERED( [ZXW_PK] ASC ),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusConditionTypeLanguageHistory))
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType] ON [RefCusConditionTypeLanguage] ([ZXW_ZX2_ConditionType] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage_ZXW_ZX2_ConditionType] ON [RefCusConditionTypeLanguage] ([ZXW_ZX6_NKLanguage] ASC,[ZXW_ZX2_ConditionType] ASC)
GO
ALTER TABLE RefCusConditionTypeLanguage SET (LOCK_ESCALATION = DISABLE);
