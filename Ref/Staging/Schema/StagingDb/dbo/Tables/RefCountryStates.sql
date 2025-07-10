CREATE TABLE [RefCountryStates] (
	[RW_PK] [uniqueidentifier] NOT NULL,
	[RW_Description] [nvarchar](35) NOT NULL,
	[RW_RegionName] [nvarchar](35) NOT NULL,
	[RW_IsActive] [bit] NOT NULL,
	[RW_Code] [varchar](3) NOT NULL,
	[RW_RN_NKCountryCode] [char](2) NOT NULL,
	CONSTRAINT [PK_UX__RW_PK] PRIMARY KEY NONCLUSTERED ([RW_PK] ASC)
)
