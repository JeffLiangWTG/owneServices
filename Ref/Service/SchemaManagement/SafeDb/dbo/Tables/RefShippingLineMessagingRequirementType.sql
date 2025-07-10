CREATE TABLE RefShippingLineMessagingRequirementType(
	[RST_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_PK] DEFAULT (NEWID()),
	[RST_Code] CHAR(3) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_Code] DEFAULT '',
	[RST_Description] VARCHAR(256) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_Description] DEFAULT '',
	[RST_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RST_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[RST_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RST_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([RST_SysStartTime], [RST_SysEndTime]), 
	CONSTRAINT [PK_RefShippingLineMessagingRequirementType] PRIMARY KEY CLUSTERED ([RST_PK] ASC),
	CONSTRAINT [CK_RefShippingLineMessagingRequirementType_RST_Code] CHECK (LEN([RST_Code])=3),
	CONSTRAINT [CK_RefShippingLineMessagingRequirementType_RST_Description] CHECK ([RST_Description] <> '')
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefShippingLineMessagingRequirementTypeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefShippingLineMessagingRequirementType_RST_Code_RST_Description ON [RefShippingLineMessagingRequirementType] ([RST_Code] ASC, [RST_Description] ASC)
GO
ALTER TABLE RefShippingLineMessagingRequirementType SET (LOCK_ESCALATION = DISABLE);
