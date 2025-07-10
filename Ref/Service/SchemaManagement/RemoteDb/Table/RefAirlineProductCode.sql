CREATE TABLE RefAirlineProductCode
(
	[RAR_PK] UNIQUEIDENTIFIER NOT NULL,
	[RAR_AirlineID] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineProductCode_RAR_AirlineID] DEFAULT '',
	[RAR_Code] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirlineProductCode_RAR_Code] DEFAULT '',
	[RAR_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefAirlineProductCode_RAR_Description] DEFAULT ''

	CONSTRAINT [PK_RefAirlineProductCode] PRIMARY KEY NONCLUSTERED ([RAR_PK] ASC),
	CONSTRAINT [CK_RefAirlineProductCode_RAR_CodeNotEmpty] CHECK ([RAR_Code] <> ''),
	CONSTRAINT [CK_RefAirlineProductCode_RAR_DescriptionNotEmpty] CHECK ([RAR_Description] <> '')
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAirlineProductCode_RAR_AirlineID_RAR_Code] ON [RefAirlineProductCode] ([RAR_AirlineID] ASC, [RAR_Code] ASC)
GO
