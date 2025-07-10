CREATE TABLE RefShippingLineMessagingRequirement(
	[RSR_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_PK] DEFAULT (NEWID()),
	[RSR_RSL_ShippingLine] UNIQUEIDENTIFIER NOT NULL,
	[RSR_RST_NKType] CHAR(3) NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_RST_NKType] DEFAULT '',
	[RSR_IsBookingRequest] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsBookingRequest] DEFAULT 0,
	[RSR_IsShippingOrder] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsShippingOrder] DEFAULT 0,
	[RSR_IsShippingInstruction] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsShippingInstruction] DEFAULT 0,
	[RSR_IsEManifest] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsEManifest] DEFAULT 0,
	[RSR_IsVerifiedGrossContainerWeight] BIT NOT NULL CONSTRAINT [DF_RefShippingLineMessagingRequirement_RSR_IsVerifiedGrossContainerWeight] DEFAULT 0,
	CONSTRAINT [PK_RefShippingLineMessagingRequirement] PRIMARY KEY CLUSTERED ([RSR_PK] ASC),
	CONSTRAINT [FK_RefShippingLineMessagingRequirement_RefShippingLine] FOREIGN KEY([RSR_RSL_ShippingLine]) REFERENCES RefShippingLine ([RSL_PK]),
	CONSTRAINT [CK_RefShippingLineMessagingRequirement_RSR_RST_NKType] CHECK (LEN(RSR_RST_NKType) = 3)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefShippingLineMessagingRequirement_RSR_RSL_ShippingLine] ON [RefShippingLineMessagingRequirement] ([RSR_RSL_ShippingLine] ASC)
GO
