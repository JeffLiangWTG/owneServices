CREATE TABLE RefShippingLineMessagingRequirementType(
	[RST_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_PK] DEFAULT (NEWID()),
	[RST_Code] CHAR(3) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_Code] DEFAULT '',
	[RST_Description] VARCHAR(256) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirementType_RST_Description] DEFAULT ''
	CONSTRAINT [PK_RefShippingLineMessagingRequirementType] PRIMARY KEY CLUSTERED ([RST_PK] ASC),
	CONSTRAINT [CK_RefShippingLineMessagingRequirementType_RST_Code] CHECK (LEN([RST_Code])=3),
)
GO
