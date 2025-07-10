CREATE TABLE [RefAirlineUNDGRule]
(
	[RMD_PK] UNIQUEIDENTIFIER NOT NULL,
	[RMD_AirlineID] VARCHAR(40) NOT NULL CONSTRAINT [DF_RefAirlineUNDGRule_RMD_AirlineID] DEFAULT '',
	[RMD_AirlineIDType] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineUNDGRule_RMD_AirlineIDType] DEFAULT '',
	[RMD_RuleName] VARCHAR(70) NOT NULL CONSTRAINT [DF_RefAirlineUNDGRule_RMD_RuleName] DEFAULT '',
	[RMD_StartDate] DATE NOT NULL,
	[RMD_EndDate] DATE NULL,
	[RMD_Description] VARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefAirlineUNDGRule_RMD_Description] DEFAULT '',

	CONSTRAINT [PK_UX__RMD_PK] PRIMARY KEY NONCLUSTERED ([RMD_PK] ASC),
	CONSTRAINT [Constraint_RMD_AirlineIDNotEmpty] CHECK ([RMD_AirlineID] <> ''),
	CONSTRAINT [Constraint_RMD_RuleNameNotEmpty] CHECK ([RMD_RuleName] <> ''),
	CONSTRAINT [Constraint_RMD_AirlineIDType] CHECK ([RMD_AirlineIDType]='TLC' OR [RMD_AirlineIDType]='EAC' OR [RMD_AirlineIDType]='ALN'),
	CONSTRAINT [CK_RefAirlineUNDGRule_RMD_StartDate_RMD_EndDate] CHECK ([RMD_EndDate] IS NULL OR [RMD_StartDate] <= [RMD_EndDate]),
	CONSTRAINT [Constraint_RMD_DescriptionNotEmpty] CHECK ([RMD_Description] <> '')
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAirlineUNDGRule_RMD_AirlineID_RMD_AirlineIDType_RMD_RuleName] ON [RefAirlineUNDGRule] ([RMD_AirlineID] ASC,[RMD_AirlineIDType] ASC,[RMD_RuleName] ASC)
GO
