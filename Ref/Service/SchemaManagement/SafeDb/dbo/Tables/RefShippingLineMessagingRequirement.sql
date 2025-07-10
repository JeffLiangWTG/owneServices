CREATE TABLE RefShippingLineMessagingRequirement(
	[RSR_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_PK] DEFAULT (NEWID()),
	[RSR_RSL_ShippingLine] UNIQUEIDENTIFIER NOT NULL,
	[RSR_RST_NKType] CHAR(3) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_RST_NKType] DEFAULT '',
	[RSR_IsBookingRequest] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsBookingRequest] DEFAULT 0,
	[RSR_IsShippingOrder] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsShippingOrder] DEFAULT 0,
	[RSR_IsShippingInstruction] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsShippingInstruction] DEFAULT 0,
	[RSR_IsEManifest] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsEManifest] DEFAULT 0,
	[RSR_IsVerifiedGrossContainerWeight] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsVerifiedGrossContainerWeight] DEFAULT 0,
	[RSR_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RSR_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[RSR_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RSR_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([RSR_SysStartTime], [RSR_SysEndTime]),
	CONSTRAINT [PK_RefShippingLineMessagingRequirement] PRIMARY KEY CLUSTERED ([RSR_PK] ASC),
	CONSTRAINT [FK_RefShippingLineMessagingRequirement_RefShippingLine] FOREIGN KEY([RSR_RSL_ShippingLine]) REFERENCES RefShippingLine ([RSL_PK]),
	CONSTRAINT [CK_RefShippingLineMessagingRequirement_RSR_RST_NKType] CHECK (LEN(RSR_RST_NKType) = 3)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefShippingLineMessagingRequirementHistory))
GO
CREATE NONCLUSTERED INDEX [IX_RefShippingLineMessagingRequirement_RSR_RSL_ShippingLine] ON [RefShippingLineMessagingRequirement] ([RSR_RSL_ShippingLine] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RSR_RSL_ShippingLine_RSR_RST_NKType] ON [RefShippingLineMessagingRequirement] (RSR_RSL_ShippingLine ASC, RSR_RST_NKType ASC)
GO
ALTER TABLE RefShippingLineMessagingRequirement SET (LOCK_ESCALATION = DISABLE);
