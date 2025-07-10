CREATE TABLE RefCusConditionLanguage (
	[ZXJ_PK] UNIQUEIDENTIFIER NOT NULL,
	[ZXJ_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage] DEFAULT '',
	[ZXJ_Comment] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_Comment] DEFAULT '',
	[ZXJ_Source] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_Source] DEFAULT '',
	[ZXJ_ZX1_Condition] UNIQUEIDENTIFIER NOT NULL,
	[ZXJ_AdditionalComment] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_AdditionalComment] DEFAULT '',
	CONSTRAINT [PK_RefCusConditionLanguage] PRIMARY KEY NONCLUSTERED ([ZXJ_PK]),
	CONSTRAINT [CK_ZXJ_ZX6_NKLanguageNotEmpty] CHECK ([ZXJ_ZX6_NKLanguage] <> ''),
	CONSTRAINT [CK_ZXJ_Comment_OR_ZXJ_SourceNotEmpty] CHECK ([ZXJ_Comment] <> '' OR [ZXJ_Source] <> ''),
	CONSTRAINT [FK_RefCusConditionLanguage_RefCusCondition] FOREIGN KEY ([ZXJ_ZX1_Condition]) REFERENCES RefCusCondition ([ZX1_PK]),
	CONSTRAINT [FK_RefCusConditionLanguage_RefLanguageType] FOREIGN KEY ([ZXJ_ZX6_NKLanguage]) REFERENCES RefLanguageType([ZX6_Language])
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage ON RefCusConditionLanguage(ZXJ_ZX6_NKLanguage ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionLanguage_ZXJ_ZX1_Condition_ZXJ_ZX6_NKLanguage ON RefCusConditionLanguage(ZXJ_ZX1_Condition ASC, ZXJ_ZX6_NKLanguage ASC)
GO
ALTER TABLE RefCusConditionLanguage SET (LOCK_ESCALATION = DISABLE);
