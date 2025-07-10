CREATE TABLE RefShippingLineEBLProvider(
	[RSE_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefShippingLineEBLProvider_RSE_PK] DEFAULT (NEWID()),
	[RSE_RSL_ShippingLine] UNIQUEIDENTIFIER NOT NULL,
	[RSE_Name] VARCHAR(255) NOT NULL CONSTRAINT [DF_RefShippingLineEBLProvider_RSE_Name] DEFAULT '',
	[RSE_IsAvailable] BIT NOT NULL CONSTRAINT [DF_RefShippingLineEBLProvider_RSE_IsAvailable] DEFAULT 0,
	[RSE_IsDefault] BIT NOT NULL CONSTRAINT [DF_RefShippingLineEBLProvider_RSE_IsDefault] DEFAULT 0,
	CONSTRAINT [PK_RefShippingLineEBLProvider] PRIMARY KEY NONCLUSTERED ([RSE_PK] ASC),
	CONSTRAINT [FK_RefShippingLineEBLProvider_RefShippingLine] FOREIGN KEY([RSE_RSL_ShippingLine]) REFERENCES RefShippingLine ([RSL_PK])
)
GO
CREATE CLUSTERED INDEX [IX_RefShippingLineEBLProvider_RSE_RSL_ShippingLine] ON [RefShippingLineEBLProvider] ([RSE_RSL_ShippingLine] ASC)
GO
