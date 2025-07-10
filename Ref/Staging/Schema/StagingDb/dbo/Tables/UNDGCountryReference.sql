CREATE TABLE [UNDGCountryReference] (
	[DCR_PK] UNIQUEIDENTIFIER NOT NULL,
	[DCR_Type] VARCHAR(5) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Type] DEFAULT '',
	[DCR_RN_NKCountry] VARCHAR(2) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_RN_NKCountry] DEFAULT '',
	[DCR_Code] VARCHAR(6) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Code] DEFAULT '',
	[DCR_Description] VARCHAR(200) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Description] DEFAULT '',
	[DCR_HasFlashPointLower] BIT NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_HasFlashPointLower] DEFAULT 0,
	[DCR_FlashPointLowerCentigrade] DECIMAL(8,1) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_FlashPointLowerCentigrade] DEFAULT 0,
	[DCR_HasFlashPointUpper] BIT NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_HasFlashPointUpper] DEFAULT 0,
	[DCR_FlashPointUpperCentigrade] DECIMAL(8,1) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_FlashPointUpperCentigrade] DEFAULT 0,
	CONSTRAINT [PK_UNDGCountryReference] PRIMARY KEY CLUSTERED  ([DCR_PK] ASC),
	CONSTRAINT [CK_DCR_FlashPointLowerCentigrade] CHECK ([DCR_FlashPointLowerCentigrade] >= -273.15),
	CONSTRAINT [CK_DCR_FlashPointUpperCentigrade] CHECK ([DCR_FlashPointUpperCentigrade] >= -273.15),
)
GO
