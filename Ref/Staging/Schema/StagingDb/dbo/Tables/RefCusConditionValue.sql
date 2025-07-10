CREATE TABLE RefCusConditionValue
(
 ZX3_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusConditionValue_ZX3_PK default newid(),
 ZX3_ZX4_NKValueType VARCHAR(5),
 ZX3_ZX4_ZZZ_NKDataGrouping VARCHAR(3),
 ZX3_ZX1_Condition UNIQUEIDENTIFIER NOT NULL,
 ZX3_Value NVARCHAR(500) NOT NULL,
 ZX3_LogicalORWithinGroup TINYINT NOT NULL CONSTRAINT DF_RefCusConditionValue_ZX3_LogicalORWithinGroup DEFAULT 0,
 CONSTRAINT PK_RefCusConditionValue PRIMARY KEY CLUSTERED( ZX3_PK ASC ),
 CONSTRAINT FK_RefCusConditionValue_RefCusCondition FOREIGN KEY (ZX3_ZX1_Condition) References RefCusCondition (ZX1_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionValue_ZX3_ZX1_Condition] ON [RefCusConditionValue] ([ZX3_ZX1_Condition])
