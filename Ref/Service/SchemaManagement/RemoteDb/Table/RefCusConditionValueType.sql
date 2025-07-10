CREATE TABLE RefCusConditionValueType
(
[ZX4_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionValueType_ZX4_PK] DEFAULT NEWID(),
[ZX4_ValueType] VARCHAR(5) NOT NULL,
[ZX4_Description] NVARCHAR(500) NOT NULL,
[ZX4_IsFormula] BIT NOT NULL CONSTRAINT [DF_RefCusConditionValueType_ZX4_IsFormula] DEFAULT 0,
[ZX4_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,

CONSTRAINT [PK_RefCusConditionValueType] PRIMARY KEY CLUSTERED( [ZX4_PK] ASC ),
CONSTRAINT [CK_RefCusConditionValueType_ZX4_ValueType] CHECK ([ZX4_ValueType] <> ''),
CONSTRAINT [CK_RefCusConditionValueType_ZX4_Description] CHECK ([ZX4_Description] <> ''),
CONSTRAINT [FK_RefCusConditionValueType_RefDataGrouping] FOREIGN KEY ([ZX4_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionValueType_ZX4_ValueType_ZX4_ZZZ_NKDataGrouping ON RefCusConditionValueType(ZX4_ValueType , ZX4_ZZZ_NKDataGrouping)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionValueType_ZX4_ZZZ_NKDataGrouping ON RefCusConditionValueType(ZX4_ZZZ_NKDataGrouping)
GO
