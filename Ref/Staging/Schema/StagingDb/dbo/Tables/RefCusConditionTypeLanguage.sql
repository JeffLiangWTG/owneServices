CREATE TABLE [RefCusConditionTypeLanguage]
(
	[ZXW_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_PK] DEFAULT NEWID(),
	[ZXW_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_ZX6_NKLanguage] DEFAULT '',
	[ZXW_ZX2_ConditionType] UNIQUEIDENTIFIER NOT NULL,
	[ZXW_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionTypeLanguage_ZXW_Description] DEFAULT '',
	CONSTRAINT [FK_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType] FOREIGN KEY([ZXW_ZX2_ConditionType]) REFERENCES [RefCusConditionType] ([ZX2_PK]),
	CONSTRAINT [PK_RefCusConditionTypeLanguage] PRIMARY KEY CLUSTERED( [ZXW_PK] ASC ),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionTypeLanguage_ZXW_ZX2_ConditionType] ON [RefCusConditionTypeLanguage] ([ZXW_ZX2_ConditionType])
GO
