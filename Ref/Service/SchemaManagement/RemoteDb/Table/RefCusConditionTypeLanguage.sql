CREATE TABLE RefCusConditionTypeLanguage
(
[ZXW_PK] UNIQUEIDENTIFIER NOT NULL,
[ZXW_ZX2_ConditionType] UNIQUEIDENTIFIER NOT NULL,
[ZXW_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage] DEFAULT '',
[ZXW_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_Description] DEFAULT '',

CONSTRAINT [PK_RefCusConditionTypeLanguage] PRIMARY KEY ([ZXW_PK]),
CONSTRAINT [FK_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType] FOREIGN KEY ([ZXW_ZX2_ConditionType]) REFERENCES [RefCusConditionType] ([ZX2_PK]),
CONSTRAINT [FK_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage] FOREIGN KEY([ZXW_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage_ZXW_ZX2_ConditionType ON RefCusConditionTypeLanguage(ZXW_ZX6_NKLanguage, ZXW_ZX2_ConditionType)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType ON RefCusConditionTypeLanguage(ZXW_ZX2_ConditionType)
GO
