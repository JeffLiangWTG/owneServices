CREATE TABLE [dbo].[RefCusAUNexdocECMCode](
	[ZY5_PK] [uniqueidentifier] NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_ZY5_PK default newid(),
	[ZY5_CommodityCode] [char](1) NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_ZY5_CommodityCode default '',
	[ZY5_PreservationCode] [varchar](2) NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_ZY5_PreservationCode default '',
	[ZY5_ProductTypeCode] [varchar](3) NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_ZY5_ProductTypeCode default '',
	[ZY5_PackTypeCode] [varchar](2) NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_PackTypeCode default '',
	[ZY5_SupplementaryCode] [varchar](2) NOT NULL CONSTRAINT DF_RefCusAUNexdocECMCode_SupplementaryCode default ''
	CONSTRAINT [PK_RefCusAUNexdocECMCode] PRIMARY KEY CLUSTERED ([ZY5_PK] ASC)
)
GO
