CREATE TABLE ediProd.LicenceDatabase
(
	[LD_PK] [uniqueidentifier] NOT NULL,
	[LD_ServerCode] [varchar](3) NOT NULL,
	[LD_LicenceType] [varchar](3) NOT NULL,
	[LD_LE] [uniqueidentifier] NOT NULL,
	[LD_IsActive] [bit] NOT NULL,
	[LD_DatabaseNumber] [int] NOT NULL,
	[LD_Product] [varchar](3) NOT NULL,
	[LD_HostedLocation] [char](3) NOT NULL,
	[LD_TenantID] [varchar](50) NOT NULL,
	CONSTRAINT [PK_UC__LD_PK] PRIMARY KEY CLUSTERED ([LD_PK] ASC)
);
