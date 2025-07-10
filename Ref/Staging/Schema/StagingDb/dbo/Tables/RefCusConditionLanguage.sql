CREATE TABLE RefCusConditionLanguage (
	[ZXJ_PK] UNIQUEIDENTIFIER NOT NULL,
	[ZXJ_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage] DEFAULT '',
	[ZXJ_Comment] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_Comment] DEFAULT '',
	[ZXJ_Source] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_Source] DEFAULT '',
	[ZXJ_ZX1_Condition] UNIQUEIDENTIFIER NOT NULL,
	[ZXJ_AdditionalComment] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_AdditionalComment] DEFAULT '',
	CONSTRAINT [PK_RefCusConditionLanguage] PRIMARY KEY NONCLUSTERED ([ZXJ_PK]),
	CONSTRAINT [FK_RefCusConditionLanguage_RefCusCondition] FOREIGN KEY ([ZXJ_ZX1_Condition]) REFERENCES RefCusCondition ([ZX1_PK])
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage ON RefCusConditionLanguage(ZXJ_ZX6_NKLanguage ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionLanguage_ZXJ_ZX1_Condition ON RefCusConditionLanguage(ZXJ_ZX1_Condition ASC)
GO
