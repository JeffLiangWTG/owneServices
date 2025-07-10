CREATE TABLE RefCusConditionCode
(
	[ZY7_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionCode_ZY7_PK] DEFAULT NEWID(),
	[ZY7_ConditionCode] VARCHAR(3) NOT NULL,
	[ZY7_Description] VARCHAR(4000) NOT NULL,
	[ZY7_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	CONSTRAINT [PK_RefCusConditionCode] PRIMARY KEY CLUSTERED( [ZY7_PK] ASC )
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionCode_ZY7_ConditionCode_ZY7_ZZZ_NKDataGrouping] ON RefCusConditionCode ([ZY7_ConditionCode] ASC, [ZY7_ZZZ_NKDataGrouping] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusConditionCode_ZY7_ConditionCode] ON RefCusConditionCode ([ZY7_ConditionCode] ASC)
GO
