CREATE TABLE RefCusConditionType(
	[ZX2_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConditionType_ZX2_PK] DEFAULT NEWID(),
	[ZX2_ConditionClass] VARCHAR(5) NOT NULL,
	[ZX2_ConditionType] VARCHAR(6) NOT NULL,
	[ZX2_Description] NVARCHAR(500) NOT NULL,
	[ZX2_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZX2_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZX2_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZX2_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZX2_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZX2_SysStartTime], [ZX2_SysEndTime]),
	CONSTRAINT [PK_RefCusConditionType] PRIMARY KEY CLUSTERED( [ZX2_PK] ASC ),
	CONSTRAINT [CK_RefCusConditionType_ZX2_ConditionClass] CHECK ([ZX2_ConditionClass]='CLASS' OR [ZX2_ConditionClass]='RATE' OR [ZX2_ConditionClass]='CTRL' OR [ZX2_ConditionClass]='VAT' OR [ZX2_ConditionClass]='RISK'),
	CONSTRAINT [CK_RefCusConditionType_ZX2_ConditionType] CHECK ([ZX2_ConditionType] <>''),
	CONSTRAINT [CK_RefCusConditionType_ZX2_Description] CHECK ([ZX2_Description] <>''),
	CONSTRAINT [FK_RefCusConditionType_RefDataGrouping] FOREIGN KEY([ZX2_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusConditionTypeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusConditionType_ZX2_ConditionType_ZX2_ZZZ_NKDataGrouping] ON [RefCusConditionType]([ZX2_ConditionType] , [ZX2_ZZZ_NKDataGrouping])
GO
ALTER TABLE RefCusConditionType SET (LOCK_ESCALATION = DISABLE);
