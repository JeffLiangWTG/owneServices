CREATE TABLE [RefAirlineCommodityCode]
(
	[RAC_PK] UNIQUEIDENTIFIER NOT NULL,
	[RAC_AirlineID] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineCommodityCode_RAC_AirlineID] DEFAULT '',
	[RAC_Code] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefAirlineCommodityCode_RAC_Code] DEFAULT '',
	[RAC_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefAirlineCommodityCode_RAC_Description] DEFAULT '',
	[RAC_SpecialHandlingCodes] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefAirlineCommodityCode_RAC_SpecialHandlingCodes] DEFAULT '', 
	CONSTRAINT [PK_UX__RAC_PK] PRIMARY KEY NONCLUSTERED ([RAC_PK] ASC),
	CONSTRAINT [Constraint_RAC_CodeNotEmpty] CHECK ([RAC_Code] <> ''),
	CONSTRAINT [Constraint_RAC_DescriptionNotEmpty] CHECK ([RAC_Description] <> '')
	)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAirlineCommodityCode_RAC_AirlineID_RAC_Code] ON [RefAirlineCommodityCode] ([RAC_AirlineID] ASC, [RAC_Code] ASC)
GO
