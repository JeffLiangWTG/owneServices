CREATE TABLE [dbo].[RefFacilityLocalCode](
	[RFL_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefFacilityLocalCode_RFL_PK] DEFAULT(NEWID()),
	[RFL_Usage] CHAR(3) NOT NULL CONSTRAINT [DF_RefFacilityLocalCode_RFL_Usage] DEFAULT '',
	[RFL_RFT_NKFacilityCode] CHAR(11) NOT NULL,
	[RFL_Code] VARCHAR(20) NOT NULL CONSTRAINT [DF_RefFacilityLocalCode_RFL_Code] DEFAULT '',
	[RFL_RN_NKCountryCode] CHAR(2) NOT NULL CONSTRAINT [DF_RefFacilityLocalCode_RFL_RN_NKCountryCode] DEFAULT '',
	CONSTRAINT [PK_RefFacilityLocalCode] PRIMARY KEY NONCLUSTERED ([RFL_PK] ASC),
	CONSTRAINT [CK_RefFacilityLocalCode_RFL_Usage] CHECK (LEN(RFL_Usage) = 3),
	CONSTRAINT [CK_RefFacilityLocalCode_RFL_RN_NKCountryCode] CHECK (LEN(RFL_RN_NKCountryCode) = 2),
	CONSTRAINT [CK_RefFacilityLocalCode_RFL_RFT_NKFacilityCode] CHECK (LEN(RFL_RFT_NKFacilityCode) = 11)
)
GO
