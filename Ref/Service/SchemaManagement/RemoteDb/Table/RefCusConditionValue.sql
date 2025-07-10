CREATE TABLE RefCusConditionValue
(
[ZX3_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionValue_ZX3_PK] DEFAULT NEWID(),
[ZX3_ZX4_ValueType] UNIQUEIDENTIFIER NOT NULL,
[ZX3_ZX1_Condition] UNIQUEIDENTIFIER NOT NULL,
[ZX3_Value] NVARCHAR(500) NOT NULL,
[ZX3_LogicalORWithinGroup] TINYINT NOT NULL CONSTRAINT [DF_RefCusConditionValue_ZX3_LogicalORWithinGroup] DEFAULT 0,
CONSTRAINT [PK_RefCusConditionValue] PRIMARY KEY CLUSTERED( [ZX3_PK] ASC ),
constraint [FK_RefCusConditionValue_RefCusConditionValueType] FOREIGN KEY ([ZX3_ZX4_ValueType]) References [RefCusConditionValueType] ([ZX4_PK]),
constraint [FK_RefCusConditionValue_RefCusCondition] FOREIGN KEY ([ZX3_ZX1_Condition]) References [RefCusCondition] ([ZX1_PK]),
CONSTRAINT [CK_RefCusConditionValue_ZX3_Value] CHECK ([ZX3_Value] <> ''),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusConditionValue_ZX3_ZX1_Condition_ZX3_ZX4_ValueType_ZX3_Value ON RefCusConditionValue(ZX3_ZX1_Condition, ZX3_ZX4_ValueType, ZX3_Value)
GO
CREATE NONCLUSTERED INDEX IX_RefCusConditionValue_ZX3_ZX4_ValueType ON RefCusConditionValue(ZX3_ZX4_ValueType)
GO
ALTER TABLE RefCusConditionValue SET (LOCK_ESCALATION = DISABLE);
