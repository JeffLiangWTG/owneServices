CREATE TABLE RefShippingLine(
	[RSL_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_PK] DEFAULT (NEWID()),
	[RSL_IsActive] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_IsActive] DEFAULT 1,
	[RSL_IsNVO] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_IsNVO] DEFAULT 0,
	[RSL_BookingRequestAvailable] BIT NOT NULL CONSTRAINT DF_RefShippingLine_RSL_BookingRequestAvailable DEFAULT 0,
	[RSL_ShippingInstructionAvailable] BIT NOT NULL CONSTRAINT DF_RefShippingLine_RSL_ShippingInstructionAvailable DEFAULT 0,
	[RSL_VerifiedGrossContainerWeightAvailable] BIT NOT NULL CONSTRAINT DF_RefShippingLine_RSL_VerifiedGrossContainerWeightAvailable DEFAULT 0,
	[RSL_ShippingOrderAvailable] BIT NOT NULL CONSTRAINT DF_RefShippingLine_RSL_ShippingOrderAvailable DEFAULT 0,
	[RSL_EManifestAvailable] BIT NOT NULL CONSTRAINT DF_RefShippingLine_RSL_EManifestAvailable DEFAULT 0,
	[RSL_CarrierName] NVARCHAR(75) NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_CarrierName] DEFAULT '',
	[RSL_StandardCarrierAlphaCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_StandardCarrierAlphaCode] DEFAULT '',
	[RSL_CargoWiseOneCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_CargoWiseOneCode] DEFAULT '',
	[RSL_OceanCarrierMessagingAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_OceanCarrierMessagingAvailable] DEFAULT 0,
	[RSL_GlobalSailingScheduleAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_GlobalSailingScheduleAvailable] DEFAULT 0,
	[RSL_ContainerAutomationAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_ContainerAutomationAvailable] DEFAULT 0,
	[RSL_CargoSphereRatesAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_CargoSphereRatesAvailable] DEFAULT 0,
	[RSL_IsShippingLine] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_IsShippingLine] DEFAULT 0,
	[RSL_InvoiceAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_InvoiceAvailable] DEFAULT 0,
	[RSL_IsCW1User] BIT NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_IsCW1User] DEFAULT 0,
	[RSL_EHubIds] NVARCHAR(250) NOT NULL CONSTRAINT [DF_RefShippingLine_RSL_EHubIds] DEFAULT '',
	[RSL_ShippingLineLogo] VARBINARY(MAX) NULL,
	[RSL_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RSL_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[RSL_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RSL_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([RSL_SysStartTime], [RSL_SysEndTime]), 
	CONSTRAINT [PK_RefShippingLine] PRIMARY KEY CLUSTERED ([RSL_PK] ASC),
	CONSTRAINT [CK_RSL_StandardCarrierAlphaCode] CHECK ((LEN([RSL_StandardCarrierAlphaCode])=(4) OR LEN([RSL_StandardCarrierAlphaCode])=(0))),
	CONSTRAINT [CK_RSL_CargoWiseOneCode] CHECK (RSL_CargoWiseOneCode <> ''),
	CONSTRAINT [CK_RSL_CarrierName] CHECK (DATALENGTH(RSL_CarrierName) > 0)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefShippingLineHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefShippingLine_RSL_CargoWiseOneCode ON [RefShippingLine] ([RSL_CargoWiseOneCode] ASC)
WHERE RSL_IsActive = 1
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefShippingLine_RSL_StandardCarrierAlphaCode ON [RefShippingLine] ([RSL_StandardCarrierAlphaCode] ASC)
WHERE RSL_StandardCarrierAlphaCode <> ''
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefShippingLine_RSL_CarrierName ON [RefShippingLine] ([RSL_CarrierName] ASC)
GO
ALTER TABLE RefShippingLine SET (LOCK_ESCALATION = DISABLE);
