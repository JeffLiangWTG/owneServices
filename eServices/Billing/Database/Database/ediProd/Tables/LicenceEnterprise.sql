CREATE TABLE ediProd.LicenceEnterprise
(
	[LE_PK] [uniqueidentifier] NOT NULL,
	[LE_EnterpriseCode] [varchar](3) NOT NULL,
	[LE_IsInternal] [bit] NOT NULL,
 CONSTRAINT [PK_UC__LE_PK] PRIMARY KEY CLUSTERED ([LE_PK] ASC)
);
