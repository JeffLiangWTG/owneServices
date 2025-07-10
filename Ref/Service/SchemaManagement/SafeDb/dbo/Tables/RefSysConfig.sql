CREATE TABLE RefSysConfig(
	[ZRC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefSysConfig_ZRC_PK]  DEFAULT (NEWID()),
	[ZRC_ZRT_NKConfigCode] VARCHAR(10) NOT NULL,
	[ZRC_DecimalValue] DECIMAL(19, 8) NOT NULL CONSTRAINT [DF_RefSysConfig_ZRC_DecimalValue] DEFAULT (0),
	[ZRC_StringValue] NVARCHAR(4000) NOT NULL CONSTRAINT [DF_RefSysConfig_ZRC_StringValue]  DEFAULT (''),
	[ZRC_BitValue] BIT NOT NULL CONSTRAINT [DF_RefSysConfig_ZRC_BitValue] DEFAULT (0),
	[ZRC_BinaryValue] VARBINARY(MAX) NULL,
	[ZRC_StartDate] DATETIME NOT NULL,
	[ZRC_EndDate] DATETIME NULL,
	[ZRC_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZRC_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZRC_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZRC_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZRC_SysStartTime], [ZRC_SysEndTime]),
	CONSTRAINT [PK_RefSysConfig] PRIMARY KEY CLUSTERED ([ZRC_PK] ASC),
	CONSTRAINT [CK_RefSysConfig_ZRC_SingleFieldValue] CHECK (([ZRC_StringValue] <> '' AND [ZRC_BitValue] = (0) AND [ZRC_DecimalValue] = (0) AND [ZRC_BinaryValue] IS NULL) OR ([ZRC_BitValue] <> (0) AND [ZRC_StringValue] = '' AND [ZRC_DecimalValue] = (0) AND [ZRC_BinaryValue] IS NULL) OR ([ZRC_DecimalValue] <> (0) AND [ZRC_StringValue] = '' AND [ZRC_BitValue] = (0) AND [ZRC_BinaryValue] IS NULL) OR ([ZRC_BinaryValue] IS NOT NULL AND [ZRC_StringValue] = '' AND [ZRC_BitValue] = (0) AND [ZRC_DecimalValue] = (0))),
	CONSTRAINT [CR_RefSysConfig_ZRC_EndDate] CHECK (([ZRC_EndDate] > [ZRC_StartDate] OR [ZRC_EndDate] IS NULL)),
	CONSTRAINT [CK_RefSysConfig_ZRC_StringValue] CHECK ((right([ZRC_StringValue],(1)) <> '>' AND left([ZRC_StringValue],(1)) <> '<')),
	CONSTRAINT [FK_RefSysConfig_RefSysConfigType] FOREIGN KEY ([ZRC_ZRT_NKConfigCode]) REFERENCES RefSysConfigType ([ZRT_ConfigCode])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefSysConfigHistory))
GO
CREATE NONCLUSTERED INDEX IX_RefSysConfig_ZRC_ZRT_NKConfigCode ON RefSysConfig (ZRC_ZRT_NKConfigCode)
GO
ALTER TABLE RefSysConfig SET (LOCK_ESCALATION = DISABLE);
