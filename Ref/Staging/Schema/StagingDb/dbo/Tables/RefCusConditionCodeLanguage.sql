CREATE TABLE RefCusConditionCodeLanguage
(
	[ZY8_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionCodeLanguage_ZY8_PK] DEFAULT NEWID(),
	[ZY8_ZY7_ConditionCode] UNIQUEIDENTIFIER NOT NULL,
	[ZY8_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZY8_Description] NVARCHAR(MAX) NOT NULL,
	CONSTRAINT [PK_RefCusConditionCodeLanguage] PRIMARY KEY CLUSTERED ([ZY8_PK]),
	CONSTRAINT [FK_RefCusConditionLanguageCode_RefCusConditionCode] FOREIGN KEY ([ZY8_ZY7_ConditionCode]) REFERENCES RefCusConditionCode ([ZY7_PK])
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionCodeLanguage_ZY8_ZX7_ConditionCode_ZY8_ZX6_NKLanguage] ON RefCusConditionCodeLanguage ([ZY8_ZY7_ConditionCode] ASC, [ZY8_ZX6_NKLanguage] ASC)
GO
