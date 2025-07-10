CREATE TABLE RefCusConditionCodeLanguage
(
	[ZY8_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionCodeLanguage_ZY8_PK] DEFAULT NEWID(),
	[ZY8_ZY7_ConditionCode] UNIQUEIDENTIFIER NOT NULL,
	[ZY8_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZY8_Description] NVARCHAR(MAX) NOT NULL,

	CONSTRAINT [PK_RefCusConditionCodeLanguage] PRIMARY KEY CLUSTERED ([ZY8_PK]),
	CONSTRAINT [CK_ZY8_ZX6_NKLanguageNotEmpty] CHECK ([ZY8_ZX6_NKLanguage] <> ''),
	CONSTRAINT [CK_ZY8_DescriptionNotEmpty] CHECK ([ZY8_Description] <> ''),
	CONSTRAINT [FK_RefCusConditionLanguageCode_RefCusConditionCode] FOREIGN KEY ([ZY8_ZY7_ConditionCode]) REFERENCES RefCusConditionCode ([ZY7_PK]),
	CONSTRAINT [FK_RefCusConditionLanguageCode_RefLanguageType] FOREIGN KEY ([ZY8_ZX6_NKLanguage]) REFERENCES RefLanguageType([ZX6_Language])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionCodeLanguage_ZY8_ZX1_ConditionCode_ZY8_ZX6_NKLanguage ON RefCusConditionCodeLanguage(ZY8_ZY7_ConditionCode ASC, ZY8_ZX6_NKLanguage ASC)
GO
