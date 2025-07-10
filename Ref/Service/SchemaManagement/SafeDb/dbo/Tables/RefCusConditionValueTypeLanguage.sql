CREATE TABLE [RefCusConditionValueTypeLanguage]
(
	[ZXX_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_PK] DEFAULT NEWID(),
	[ZXX_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage] DEFAULT '',
	[ZXX_ZX4_ValueType]	UNIQUEIDENTIFIER NOT NULL,
	[ZXX_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_Description] DEFAULT '',
	[ZXX_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZXX_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZXX_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZXX_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZXX_SysStartTime], [ZXX_SysEndTime]),
	CONSTRAINT [CK_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage] CHECK (ZXX_ZX6_NKLanguage <> ''),
	CONSTRAINT [CK_RefCusConditionValueTypeLanguage_ZXX_Description] CHECK (ZXX_Description <> ''),
	CONSTRAINT [FK_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType] FOREIGN KEY([ZXX_ZX4_ValueType]) REFERENCES [RefCusConditionValueType] ([ZX4_PK]),
	CONSTRAINT [FK_RefCusConditionValueTypeLanguage_ZXV_ZX6_NKLanguage] FOREIGN KEY([ZXX_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language]),
	CONSTRAINT [PK_RefCusConditionValueTypeLanguage] PRIMARY KEY CLUSTERED( [ZXX_PK] ASC ),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusConditionValueTypeLanguageHistory))
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType] ON [RefCusConditionValueTypeLanguage] ([ZXX_ZX4_ValueType] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage_ZXX_ZX4_ValueType] ON [RefCusConditionValueTypeLanguage] ([ZXX_ZX6_NKLanguage] ASC,[ZXX_ZX4_ValueType] ASC)
GO
ALTER TABLE RefCusConditionValueTypeLanguage SET (LOCK_ESCALATION = DISABLE);
