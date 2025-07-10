CREATE TABLE RefCusConditionValueTypeLanguage
(
[ZXX_PK] UNIQUEIDENTIFIER NOT NULL,
[ZXX_ZX4_ValueType] UNIQUEIDENTIFIER NOT NULL,
[ZXX_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage] DEFAULT '',
[ZXX_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_Description] DEFAULT '',

CONSTRAINT [PK_RefCusConditionValueTypeLanguage] PRIMARY KEY ([ZXX_PK]),
CONSTRAINT [FK_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType] FOREIGN KEY ([ZXX_ZX4_ValueType]) REFERENCES [RefCusConditionValueType] ([ZX4_PK]),
CONSTRAINT [FK_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage] FOREIGN KEY([ZXX_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage_ZXX_ZX4_ValueType ON RefCusConditionValueTypeLanguage(ZXX_ZX6_NKLanguage, ZXX_ZX4_ValueType)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType ON RefCusConditionValueTypeLanguage(ZXX_ZX4_ValueType)
GO
