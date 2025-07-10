CREATE TABLE [RefCusConditionValueTypeLanguage]
(
	[ZXX_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_PK] DEFAULT NEWID(),
	[ZXX_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_ZX6_NKLanguage] DEFAULT '',
	[ZXX_ZX4_ValueType]	UNIQUEIDENTIFIER NOT NULL,
	[ZXX_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusConditionValueTypeLanguage_ZXX_Description] DEFAULT '',
	CONSTRAINT [FK_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType] FOREIGN KEY([ZXX_ZX4_ValueType]) REFERENCES [RefCusConditionValueType] ([ZX4_PK]),
	CONSTRAINT [PK_RefCusConditionValueTypeLanguage] PRIMARY KEY CLUSTERED( [ZXX_PK] ASC ),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionValueTypeLanguage_ZXX_ZX4_ValueType] ON [RefCusConditionValueTypeLanguage] ([ZXX_ZX4_ValueType] ASC)
GO
