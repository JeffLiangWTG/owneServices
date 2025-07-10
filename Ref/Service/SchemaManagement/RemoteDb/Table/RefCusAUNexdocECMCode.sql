CREATE TABLE RefCusAUNexdocECMCode
(
	[ZY5_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_PK] DEFAULT NEWID(),
	[ZY5_CommodityCode] CHAR(1) NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_CommodityCode] DEFAULT '',
	[ZY5_PreservationCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_PreservationCode] DEFAULT '',
	[ZY5_ProductTypeCode] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_ProductTypeCode] DEFAULT '',
	[ZY5_PackTypeCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_PackTypeCode] DEFAULT '',
	[ZY5_SupplementaryCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_RefCusAUNexdocECMCode_ZY5_SupplementaryCode] DEFAULT '',

	CONSTRAINT PK_RefCusAUNexdocECMCode PRIMARY KEY CLUSTERED( ZY5_PK ASC )
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusAUNexdocECMCode_ZY5_PreservationCode ON RefCusAUNexdocECMCode (ZY5_PreservationCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusAUNexdocECMCode_ZY5_ProductTypeCode ON RefCusAUNexdocECMCode (ZY5_ProductTypeCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusAUNexdocECMCode_ZY5_PackTypeCode ON RefCusAUNexdocECMCode (ZY5_PackTypeCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusAUNexdocECMCode_ZY5_SupplementaryCode ON RefCusAUNexdocECMCode (ZY5_SupplementaryCode)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusAUNexdocECMCode_ZY5_CommodityCode_ZY5_PreservationCode_ZY5_ProductTypeCode_ZY5_PackTypeCode_ZY5_SupplementaryCode ON RefCusAUNexdocECMCode (ZY5_CommodityCode, ZY5_PreservationCode, ZY5_ProductTypeCode, ZY5_PackTypeCode, ZY5_SupplementaryCode)
GO
